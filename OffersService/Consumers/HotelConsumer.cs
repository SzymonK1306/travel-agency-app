using MassTransit;
using OffersService.Messages;

namespace OffersService.Consumers
{
    public class HotelConsumer : IConsumer<HotelMessage>
    {
        public async Task Consume(ConsumeContext<HotelMessage> context)
        {
            var message = context.Message.Content;
            Console.WriteLine($"Received from HotelsService: {message}");
            // Zapewnij odpowiednią logikę obsługi
        }
    }
}