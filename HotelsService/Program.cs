using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Timers;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Tworzenie kolejki do odbierania "hello"
channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
// Tworzenie kolejki do wysyłania "how are you"
channel.QueueDeclare(queue: "response", durable: false, exclusive: false, autoDelete: false, arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) => {
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"OffersService said: {message}");

    // Odpowiedz na otrzymaną wiadomość
    if (message == "hello") {
        var responseProperties = channel.CreateBasicProperties();
        var response = Encoding.UTF8.GetBytes("how are you");
        channel.BasicPublish(exchange: "", routingKey: "response", basicProperties: responseProperties, body: response);
        Console.WriteLine("Sent 'how are you' to OffersService");
    }
};

channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

app.Run();