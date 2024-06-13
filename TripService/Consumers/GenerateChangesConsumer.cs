using MassTransit;
using TripService.Data;
using Shared.Trip.Events;
using Shared.Trip.Dtos;
using TripService.Models;
using Shared.Hotel.Events;
using Shared.Flight.Events;

namespace TripService.Consumers
{
    public class GenerateChangesConsumer : IConsumer<GenerateChangesEvent>
    {
        private readonly ITripRepo _tripRepo;
        
        public GenerateChangesConsumer(ITripRepo tripRepo)
        {
            _tripRepo = tripRepo;
        }

        public async Task Consume(ConsumeContext<GenerateChangesEvent> context)
        {
            Console.WriteLine("Data generating...");
            var changedTrip = _tripRepo.GetRandomTripAndGenerateChanges();
            if (changedTrip != null) 
            {
                Console.WriteLine("Generation successful!");
                await context.Publish(changedTrip);
                if (changedTrip.PreviousValue == "Zarezerwowana")
                {
                    Trip changedTripInRepo = _tripRepo.GetTripByGuid(changedTrip.OfferId);
                    Guid corrId = Guid.NewGuid();

                    Console.WriteLine("Unreserving room");
                    await context.Publish(new UnreserveRoomWithoutIdEvent{
                        CorrelationId = corrId,
                        ClientId = changedTripInRepo.ClientId ?? Guid.Empty,
                        ArrivalDate = changedTripInRepo.DepartureDate,
                        ReturnDate = changedTripInRepo.ReturnDate,
                        HotelId = changedTripInRepo.HotelId,
                        NumOfPeople = changedTripInRepo.NumOfAdults + changedTripInRepo.NumOfKidsTo18 + changedTripInRepo.NumOfKidsTo10 + changedTripInRepo.NumOfKidsTo3,
                        RoomType = changedTripInRepo.RoomType,
                        Features = changedTripInRepo.Features,
                    });
                    
                    Console.WriteLine("Unreserving flight");
                    await context.Publish(new ReserveSeatsEvent{
                        CorrelationId = corrId,
                        FlightId = changedTripInRepo.FlightId,
                        Seats = (-1) * (changedTripInRepo.NumOfAdults + changedTripInRepo.NumOfKidsTo18 + changedTripInRepo.NumOfKidsTo10)
                    });
                }
            } 
            else 
            {
                Console.WriteLine("Generation failed!");
            }

            await Task.Yield();     // Ensures that method runs asynchronously and avoids the warning
        }
    }
}