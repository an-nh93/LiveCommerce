using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace LiveCommerce.Infrastructure.Messaging;

public interface IRabbitMQPublisher
{
    void PublishCommentIngestion(string payloadJson);
}

public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly RabbitMQ.Client.IConnection? _connection;
    private readonly RabbitMQ.Client.IModel? _channel;

    public RabbitMQPublisher(IOptions<RabbitMQOptions> options)
    {
        _options = options.Value;
        try
        {
            var factory = new RabbitMQ.Client.ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_options.CommentIngestionQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }
        catch (Exception)
        {
            _connection = null;
            _channel = null;
        }
    }

    public void PublishCommentIngestion(string payloadJson)
    {
        if (_channel == null) return;
        var body = Encoding.UTF8.GetBytes(payloadJson);
        _channel.BasicPublish("", _options.CommentIngestionQueue, false, null, new ReadOnlyMemory<byte>(body));
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
