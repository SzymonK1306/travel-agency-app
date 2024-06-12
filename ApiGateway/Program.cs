using ApiGateway.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Models;
using System;
using ApiGateway.Consumers;
using ApiGateway.Singletons;

var builder = WebApplication.CreateBuilder(args);

string dbConn = builder.Configuration["DATABASE_CONNECTION_STRING"] ??
    "Host=host.docker.internal;Port=5432;Database=userdb;Username=postgres;Password=guest;";
string rabbitmqHost = builder.Configuration["RABBITMQ_HOST"] ?? "rabbitmq";
string rabbitmqPort = builder.Configuration["RABBITMQ_PORT"] ?? "5672";

Console.WriteLine("Database connection string: ", dbConn);
Console.WriteLine("RabbitMQ host: ", rabbitmqHost);
Console.WriteLine("RabbitMQ port: ", rabbitmqPort);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(x => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<UserDbContext>(options => options.UseNpgsql(dbConn));
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddSingleton<GenerationState>();

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<NewDestinationPreferenceConsumer>(context =>
    {
        context.UseMessageRetry(r => r.Interval(3, 1000));
        context.UseInMemoryOutbox();
    });
    cfg.UsingRabbitMq((context, rabbitCfg) =>
    {
        rabbitCfg.Host(new Uri($"rabbitmq://{rabbitmqHost}:{rabbitmqPort}/"), h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        rabbitCfg.ConfigureEndpoints(context);
    });
});

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.MapControllers();

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
