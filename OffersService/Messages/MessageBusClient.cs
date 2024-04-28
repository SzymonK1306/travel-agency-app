using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using OffersServise.Models;

namespace OffersService.Messaging {
    public class MessageBusClient : IMessageBusClient, IDisposable {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;

        public MessageBusClient(IConfiguration configuration) {
            _configuration = configuration;
            var factory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"]) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void SendOffer(Offer offer) { 
            var json = JsonSerializer.Serialize(offer);
            var body = Encoding.UTF8.GetBytes(json);
            _channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
        }

        public void Dispose() {
            if (_channel.IsOpen) {
                _channel.Close();
                _connection.Close();
            }
        }
    }
}