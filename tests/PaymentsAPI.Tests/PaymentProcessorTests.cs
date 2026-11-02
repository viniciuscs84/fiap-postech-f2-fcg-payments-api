using PaymentsAPI.Contracts;
using PaymentsAPI.Payments;

namespace PaymentsAPI.Tests;

public sealed class PaymentProcessorTests
{
    [Theory]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    public async Task ProcessAsync_PublishesConfiguredOutcome(string outcome)
    {
        var publisher = new RecordingPaymentResultPublisher();
        var processor = new PaymentProcessor(
            new PaymentSimulationOptions { Outcome = outcome },
            publisher);
        var order = new OrderPlacedEvent(
            Guid.NewGuid(), Guid.NewGuid(), 49.90m, "user@example.com");

        await processor.ProcessAsync(order, CancellationToken.None);

        var payment = Assert.Single(publisher.Published);
        Assert.Equal(order.UserId, payment.UserId);
        Assert.Equal(order.GameId, payment.GameId);
        Assert.Equal(outcome, payment.Status);
        Assert.Equal(order.RecipientEmail, payment.RecipientEmail);
    }

    [Fact]
    public void Constructor_RejectsUnsupportedOutcome()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new PaymentProcessor(
                new PaymentSimulationOptions { Outcome = "Pending" },
                new RecordingPaymentResultPublisher()));

        Assert.Contains("Approved", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Rejected", exception.Message, StringComparison.Ordinal);
    }

    private sealed class RecordingPaymentResultPublisher : IPaymentResultPublisher
    {
        public List<PaymentProcessedEvent> Published { get; } = [];

        public Task PublishAsync(PaymentProcessedEvent payment, CancellationToken cancellationToken)
        {
            Published.Add(payment);
            return Task.CompletedTask;
        }
    }
}
