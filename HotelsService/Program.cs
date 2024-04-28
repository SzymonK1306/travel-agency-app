using MassTransit;
using HotelsService.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("offer-queue", e =>
        {
            e.Handler<HotelMessage>(context =>
            {
                var receivedContent = context.Message.Content;
                Console.WriteLine($"Received message: {receivedContent}");
                return context.RespondAsync(new HotelMessage { Content = "how are you" });
            });
        });
    });
});

var app = builder.Build();
app.Run();