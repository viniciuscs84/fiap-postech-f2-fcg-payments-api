using System.Text.Json;
using PaymentsAPI.Contracts;
using PaymentsAPI.Payments;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaymentsAPI.Messaging;

public sealed class OrderPlacedConsumer(
    RabbitMqOptions options,
    IPaymentProcessor processor,
    ILogger<OrderPlacedConsumer> logger) : BackgroundService
{
    private const string QueueName = "payments.order-placed";
    private const string RoutingKey = "order.placed";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = RabbitMqConnectionFactory.Create(options, "payments-api-order-consumer");
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            options.Exchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(
            QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);
        await channel.QueueBindAsync(
            QueueName,
            options.Exchange,
            RoutingKey,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var order = JsonSerializer.Deserialize<OrderPlacedEvent>(delivery.Body.Span)
                    ?? throw new JsonException("OrderPlacedEvent payload is empty.");

                await processor.ProcessAsync(order, stoppingToken);
                await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process OrderPlacedEvent.");
                await channel.BasicNackAsync(
                    delivery.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            QueueName,
            autoAck: false,
            consumer,
            stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
