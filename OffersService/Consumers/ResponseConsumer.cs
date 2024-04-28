using MassTransit;
using System.Threading.Tasks;
using OffersService.Messages;

namespace OffersService.Consumers
{
    public class ResponseConsumer : IConsumer<HotelMessage>
    {
        public async Task Consume(ConsumeContext<HotelMessage> context)
        {
            var message = context.Message.Content;
            Console.WriteLine($"Received response from HotelsService: {message}");
        }
    }
}