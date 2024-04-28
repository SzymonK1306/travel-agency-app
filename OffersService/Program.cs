using AutoMapper;
using OffersServise.Data;
using OffersService.Messaging;
using OffersServise.Models; // Użyj modelu Offer
using OffersServise.Profiles;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Timers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
builder.Services.AddScoped<IOfferRepo, OfferRepo>();
builder.Services.AddAutoMapper(typeof(OffersProfile));
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();

var app = builder.Build();

var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Tworzenie kolejki do wysyłania "hello"
channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
// Tworzenie kolejki do odbierania odpowiedzi "how are you"
channel.QueueDeclare(queue: "response", durable: false, exclusive: false, autoDelete: false, arguments: null);

var sendTimer = new System.Timers.Timer(5000); // Wysyłaj "hello" co 5 sekund
sendTimer.Elapsed += (sender, e) => {
    var properties = channel.CreateBasicProperties();
    var body = Encoding.UTF8.GetBytes("hello");
    channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: properties, body: body);
};
sendTimer.Start();

var receiveTimer = new System.Timers.Timer(10000); // Sprawdzaj odpowiedzi co 10 sekund, może być inne tempo
receiveTimer.Elapsed += (sender, e) => {
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) => {
        var responseBody = ea.Body.ToArray();
        var responseMessage = Encoding.UTF8.GetString(responseBody);
        Console.WriteLine($"Received from HotelsService: {responseMessage}");
    };
    channel.BasicConsume(queue: "response", autoAck: true, consumer: consumer);
};
receiveTimer.Start();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

PrepDb.PrepPopulation(app);

app.Run();