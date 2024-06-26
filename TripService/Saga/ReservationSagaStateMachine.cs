using MassTransit;
using Shared.Flight.Events;
using Shared.Hotel.Events;
using Shared.Payment.Events;
using Shared.Trip.Events;

namespace TripService.Saga
{
    public class ReservationStateMachine : MassTransitStateMachine<ReservationState>
    {
        public State AskForReservation { get; set; } = default!;
        public State HotelAndFlightReserved { get; set; } = default!;
        public State AwaitingForHotelReservation { get; set; } = default!;
        public State AwaitingForFlightReservation { get; set; } = default!;
        public State AwaitingForFlightAndHotelReservation { get; set; } = default!;
        public State HotelAndTransportSuccessfullyReserved { get; set; } = default!;
        public State AwaitingForPayment { get; set; } = default!;
        public State TripReservedCorrectly { get; set; } = default!;
        public State ReservationFailed { get; set; } = default!;

        public Event<ReservationTripEvent> ReservationTripEvent {get; set;} = default!;
        public Event<ReserveSeatsReplyEvent> ReserveSeatsReplyEvent {get; set;} = default!;
        public Event<ReserveRoomReplyEvent> ReserveRoomReplyEvent {get; set;} = default!;
        public Event<ProcessPaymentCustomerReplyEvent> ProcessPaymentCustomerReplyEvent {get; set;} = default!;
        public Schedule<ReservationState, ReservationTimeoutEvent> ReservationTimeoutEvent { get; set; } = default!;

        public ReservationStateMachine()
        {
            InstanceState(x => x.CurrentState, AskForReservation, AwaitingForHotelReservation, AwaitingForFlightReservation, AwaitingForFlightAndHotelReservation, 
                HotelAndFlightReserved, AwaitingForPayment, TripReservedCorrectly, ReservationFailed);

            Event(()=>ReservationTripEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); x.SelectId(context => context.Message.CorrelationId); });
            Event(()=>ReserveSeatsReplyEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            Event(()=>ReserveRoomReplyEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });
            Event(()=>ProcessPaymentCustomerReplyEvent, x => { x.CorrelateById(context => context.Message.CorrelationId); });

            Schedule(() => ReservationTimeoutEvent, instance => instance.ReservationTimeoutEventId, s =>
            {
                s.Delay = TimeSpan.FromSeconds(60);
                s.Received = r => r.CorrelateById(context => context.Message.CorrelationId);
            });

            Console.WriteLine("I'm in SAGA!");

            Initially(
                When(ReservationTripEvent)
                    .Then(async context => {
                        if (!context.TryGetPayload(out SagaConsumeContext<ReservationState, ReservationTripEvent>? payload))
                        {
                            throw new Exception("Unable to retrieve payload from reservation");
                        }
                        if (payload == null)
                        {
                            throw new Exception("Payload should not be null");
                        }
                        
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        context.Saga.OfferId = payload.Message.OfferId;
                        Console.WriteLine("Offert Id in Saga " + context.Saga.OfferId);
                        Console.WriteLine("Offert Id in Message " + context.Message.OfferId);
                        context.Saga.ClientId = payload.Message.ClientId;
                        context.Saga.FlightId = payload.Message.FlightId;
                        context.Saga.HotelId = payload.Message.HotelId;
                        context.Saga.Name = payload.Message.Name;
                        context.Saga.Country = payload.Message.Country;
                        context.Saga.DeparturePlace = payload.Message.DeparturePlace;
                        context.Saga.NumOfAdults = payload.Message.NumOfAdults;
                        context.Saga.NumOfKidsTo18 = payload.Message.NumOfKidsTo18;
                        context.Saga.NumOfKidsTo10 = payload.Message.NumOfKidsTo10;
                        context.Saga.NumOfKidsTo3 = payload.Message.NumOfKidsTo3;
                        context.Saga.DepartureDate = payload.Message.DepartureDate;
                        context.Saga.ReturnDate = payload.Message.ReturnDate;
                        context.Saga.TransportType = payload.Message.TransportType;
                        context.Saga.Price = payload.Message.Price;
                        context.Saga.MealsType = payload.Message.MealsType;
                        context.Saga.RoomType = payload.Message.RoomType;
                        context.Saga.DiscountPercents = payload.Message.DiscountPercents;
                        context.Saga.NumOfNights = payload.Message.NumOfNights;
                        context.Saga.Status = payload.Message.Status;
                        context.Saga.TravelReservationSuccesful = false;
                        context.Saga.HotelReservationSuccesful = false;
                        context.Saga.PaymentSuccesful = false;
                        
                        await Task.Yield();
                    })
                    .PublishAsync(context => context.Init<ReserveRoomEvent>(    //.RespondAsync(context => context.Init<Reserve)    // Maybe we can ReplyEvent to API gateway
                        new ReserveRoomEvent() 
                        { 
                            CorrelationId = context.Saga.CorrelationId,
                            ClientId = context.Saga.ClientId,
                            Name = context.Saga.Name,
                            Country = context.Saga.Country,
                            City = context.Saga.City,
                            NumOfAdults = context.Saga.NumOfAdults,
                            NumOfKidsTo18 = context.Saga.NumOfKidsTo18,
                            NumOfKidsTo10 = context.Saga.NumOfKidsTo10,
                            NumOfKidsTo3 = context.Saga.NumOfKidsTo3,
                            ArrivalDate = context.Saga.DepartureDate,
                            ReturnDate = context.Saga.ReturnDate,
                            RoomType = context.Saga.RoomType,
                            NumOfNights = context.Saga.NumOfNights
                    })) 
                    // Respond to API Gateway
                    .RespondAsync(context => context.Init<ReservationTripReplyEvent>(
                        new ReservationTripReplyEvent()
                        {
                            Id = context.Saga.CorrelationId,
                            CorrelationId = context.Saga.CorrelationId
                    }))
                    .PublishAsync(context => context.Init<ChangeReservationStatusEvent>(
                        new ChangeReservationStatusEvent() 
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            ClientId = context.Saga.ClientId,
                            OfferId = context.Saga.OfferId,
                            Status = "W toku"
                    }))
                    // Check if flight reserve is needed
                    .IfElse(
                        context => context.Saga.TransportType == "Samolot", 
                        context => context
                            .PublishAsync(context => context.Init<ReserveSeatsEvent>(
                                new ReserveSeatsEvent()
                                {
                                    CorrelationId = context.Saga.CorrelationId,
                                    FlightId = context.Saga.FlightId,
                                    Seats = context.Saga.NumOfAdults + context.Saga.NumOfKidsTo18 + context.Saga.NumOfKidsTo10

                            }))
                            .TransitionTo(AwaitingForFlightAndHotelReservation), 
                        context => context
                            .Then(context => context.Saga.TravelReservationSuccesful = true)
                            .TransitionTo(AwaitingForHotelReservation)
                    ));

                // Awaiting for Flight and Hotel Reservation Events
                WhenEnter(AwaitingForFlightAndHotelReservation, binder => 
                    binder.Then(context => 
                    {
                        Console.WriteLine("-> SAGA STATE: Awaiting For Hotel Reservation and Flight Reservation");
                    })
                );

                During(AwaitingForFlightAndHotelReservation,
                    When(ReserveRoomReplyEvent)
                        .Then(context => 
                        {
                            if (!context.TryGetPayload(out SagaConsumeContext<ReservationState, ReserveRoomReplyEvent>? payload))
                            {
                                throw new Exception("Unable to retrieve payload with hotels response");
                            }
                            if (payload == null)
                            {
                                throw new Exception("Payload should not be null");
                            }

                            context.Saga.HotelReservationSuccesful = payload.Message.SuccessfullyReserved;
                            context.Saga.ReservedRoomId = payload.Message.RoomId;
                        })
                        .TransitionTo(AwaitingForFlightReservation),
                    When(ReserveSeatsReplyEvent)
                        .Then(context => 
                        {
                            if (!context.TryGetPayload(out SagaConsumeContext<ReservationState, ReserveSeatsReplyEvent>? payload))
                            {
                                throw new Exception("Unable to retrieve payload with hotels response");
                            }
                            if (payload == null)
                            {
                                throw new Exception("Payload should not be null");
                            }

                            context.Saga.TravelReservationSuccesful = payload.Message.Success;
                        })
                        .TransitionTo(AwaitingForHotelReservation)
                );  // What if someone has just reserved offer??

                // Awaiting Flight Reservation Event
                WhenEnter(AwaitingForFlightReservation, binder => 
                    binder.Then(context => 
                    {
                        Console.WriteLine("-> SAGA STATE: Awaiting For Flight Reservation");
                    })
                );

                During(AwaitingForFlightReservation, 
                    When(ReserveSeatsReplyEvent)
                        .Then(context => 
                        {
                            if (!context.TryGetPayload(out SagaConsumeContext<ReservationState, ReserveSeatsReplyEvent>? payload))
                            {
                                throw new Exception("Unable to retrieve payload with hotels response");
                            }
                            if (payload == null)
                            {
                                throw new Exception("Payload should not be null");
                            }
                            context.Saga.TravelReservationSuccesful = payload.Message.Success;
                        })
                        .TransitionTo(HotelAndFlightReserved)
                    
                );  // What if someone has just reserved offer??

                // Awaiting Hotel Reservation Event
                WhenEnter(AwaitingForHotelReservation, binder => 
                    binder.Then(context => 
                    {
                        Console.WriteLine("-> SAGA STATE: Awaiting For Hotel Reservation");
                    })
                );

                During(AwaitingForHotelReservation, 
                    When(ReserveRoomReplyEvent)
                        .Then(context => 
                        {
                            if (!context.TryGetPayload(out SagaConsumeContext<ReservationState, ReserveRoomReplyEvent>? payload))
                            {
                                throw new Exception("Unable to retrieve payload with hotels response");
                            }
                            if (payload == null)
                            {
                                throw new Exception("Payload should not be null");
                            }
                            context.Saga.HotelReservationSuccesful = payload.Message.SuccessfullyReserved;
                            context.Saga.ReservedRoomId = payload.Message.RoomId;
                        })
                        .TransitionTo(HotelAndFlightReserved)
                );  // What if someone has just reserved offer??

                // Hotel and Flight reserved - waiting for payment
                WhenEnter(HotelAndFlightReserved, binder => 
                    binder.Then(context => 
                    {
                        Console.WriteLine("-> SAGA STATE: Hotel and Flight reserved");
                        Console.WriteLine("Hotel State: " + context.Saga.HotelReservationSuccesful);
                        Console.WriteLine("Flight State: " + context.Saga.TravelReservationSuccesful);
                    })
                    .IfElse(
                        context => context.Saga.HotelReservationSuccesful && context.Saga.TravelReservationSuccesful,
                        context => context
                            // Start Timer
                            .Schedule(ReservationTimeoutEvent, context => context.Init<ReservationTimeoutEvent>(
                                new ReservationTimeoutEvent() 
                                {
                                    CorrelationId = context.Saga.CorrelationId
                                }))
                            // Change status reservation
                            .PublishAsync(context => context.Init<ChangeReservationStatusEvent>(
                                new ChangeReservationStatusEvent {
                                    CorrelationId = context.Saga.CorrelationId,
                                    ClientId = context.Saga.ClientId,
                                    OfferId = context.Saga.OfferId,
                                    Status = "Oczekiwanie na płatność"
                                }))
                            // TODO payment
                            .TransitionTo(AwaitingForPayment),
                        context => context.TransitionTo(ReservationFailed)
                    )
                );

                During(AwaitingForPayment,
                    When(ProcessPaymentCustomerReplyEvent)
                        .Then(context =>
                        {
                            if (!context.TryGetPayload(out SagaConsumeContext<ReservationState, ProcessPaymentCustomerReplyEvent>? payload))
                            {
                                throw new Exception("Unable to retrieve payload with hotels response");
                            }
                            if (payload == null)
                            {
                                throw new Exception("Payload should not be null");
                            }

                            if (payload.Message.result)
                            {
                                context.Saga.PaymentSuccesful = true;
                            }
                            else
                            {
                                context.Saga.PaymentSuccesful = false;
                            }
                            
                        })
                        .Unschedule(ReservationTimeoutEvent)
                        .IfElse(context => context.Saga.PaymentSuccesful,
                            context => context.TransitionTo(TripReservedCorrectly),
                            context => context.TransitionTo(ReservationFailed)),
                    When(ReservationTimeoutEvent is not null ? ReservationTimeoutEvent.Received : throw new Exception("ReservationTimeoutEvent is null"))
                        .Unschedule(ReservationTimeoutEvent)
                        .TransitionTo(ReservationFailed)
                );

                WhenEnter(TripReservedCorrectly, binder => 
                    binder.Then(context =>
                    {
                        Console.WriteLine("-> SAGA STATE: Successfully booked");
                    })
                    .PublishAsync(context => context.Init<ChangeReservationStatusEvent>(
                        new ChangeReservationStatusEvent() {
                            CorrelationId = context.Saga.CorrelationId,
                            OfferId = context.Saga.OfferId,
                            ClientId = context.Saga.ClientId,
                            Status = "Zarezerwowana"
                    }))
                    .Finalize()
                ); // Event change status reservation
                
                WhenEnter(ReservationFailed, binder => 
                    binder.Then(context =>
                    {
                        Console.WriteLine("-> SAGA STATE: Reservation Failed");
                    })
                    .PublishAsync(context => context.Init<ChangeReservationStatusEvent>(
                        new ChangeReservationStatusEvent
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            ClientId = null,
                            OfferId = context.Saga.OfferId,
                            Status = "Dostępna"
                        }))
                    .IfElse(context => context.Saga.HotelReservationSuccesful,
                        context => context.PublishAsync(context => context.Init<UnreserveRoomEvent>(
                            new UnreserveRoomEvent
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                ClientId = context.Saga.ClientId,
                                RoomId = context.Saga.ReservedRoomId,
                                ArrivalDate = context.Saga.DepartureDate,
                                ReturnDate = context.Saga.ReturnDate
                            })),
                        context => context)
                    .IfElse(context => context.Saga.TravelReservationSuccesful,
                        context => context.PublishAsync(context => context.Init<ReserveSeatsEvent>(
                            new ReserveSeatsEvent
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                FlightId = context.Saga.FlightId,
                                Seats = (-1) * (context.Saga.NumOfAdults + context.Saga.NumOfKidsTo18 + context.Saga.NumOfKidsTo10)
                            })),
                        context => context)
                    .Finalize()
                );

                SetCompletedWhenFinalized();
        }
    }
}