using System.Text.Json;
using PaymentsAPI.Contracts;
using PaymentsAPI.Payments;
using RabbitMQ.Client;

namespace PaymentsAPI.Messaging;

public sealed class RabbitMqPaymentResultPublisher(RabbitMqOptions options) :
    IPaymentResultPublisher,
    IAsyncDisposable
{
    private const string RoutingKey = "payment.processed";
    private static readonly string[] ConsumerQueues =
    [
        "catalog.payment-processed",
        "notifications.payment-processed"
    ];

    private readonly SemaphoreSlim publishGate = new(1, 1);
    private IConnection? connection;
    private IChannel? channel;

    public async Task PublishAsync(PaymentProcessedEvent payment, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(payment);
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Type = nameof(PaymentProcessedEvent)
        };

        await publishGate.WaitAsync(cancellationToken);
        try
        {
            var activeChannel = await GetChannelAsync(cancellationToken);
            await activeChannel.BasicPublishAsync(
                options.Exchange,
                RoutingKey,
                mandatory: false,
                properties,
                body,
                cancellationToken);
        }
        finally
        {
            publishGate.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (channel is { IsOpen: true } && connection is { IsOpen: true })
        {
            return channel;
        }

        var factory = RabbitMqConnectionFactory.Create(options, "payments-api-publisher");
        connection = await factory.CreateConnectionAsync(cancellationToken);
        channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            options.Exchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        foreach (var queue in ConsumerQueues)
        {
            await channel.QueueDeclareAsync(
                queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);
            await channel.QueueBindAsync(
                queue,
                options.Exchange,
                RoutingKey,
                cancellationToken: cancellationToken);
        }

        return channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (channel is not null)
        {
            await channel.DisposeAsync();
        }

        if (connection is not null)
        {
            await connection.DisposeAsync();
        }

        publishGate.Dispose();
    }
}
