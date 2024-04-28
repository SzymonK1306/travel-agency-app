using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OffersServise.Data;
using OffersServise.Profiles;
using System.Timers;
using OffersService.Messages;
using OffersService.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
builder.Services.AddScoped<IOfferRepo, OfferRepo>();
builder.Services.AddAutoMapper(typeof(OffersProfile));

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

        cfg.ReceiveEndpoint("hotel-response", e =>
        {
            e.Consumer<ResponseConsumer>();
        });

        cfg.ReceiveEndpoint("hotel-queue", e =>
        {
            e.Consumer<HotelConsumer>();
        });
    });
});


var app = builder.Build();

var messageTimer = new System.Timers.Timer(10 * 1000); 
int count = 0;
messageTimer.Elapsed += async (sender, e) =>
{
    if (count < 10) 
    {
        using (var scope = app.Services.CreateScope()) 
        {
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publishEndpoint.Publish(new HotelMessage { Content = "hello from OffersService" });
            count++;
        }
    }
    else
    {
        messageTimer.Stop(); 
    }
};
messageTimer.Start();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

PrepDb.PrepPopulation(app);

app.Run();