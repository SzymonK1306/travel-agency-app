using MassTransit;
using CommonMessages.Events;
using System;
using System.Threading.Tasks;

namespace HotelService.Consumers
{
    public class OfferCreatedConsumer : IConsumer<OfferCreated>
    {
        public async Task Consume(ConsumeContext<OfferCreated> context)
        {
            Console.WriteLine($"Received offer created event: {context.Message.Title}");
        }
    }
}