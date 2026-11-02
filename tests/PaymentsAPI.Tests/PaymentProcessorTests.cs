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

    [Theory]
    [InlineData(true, false, 10, "user@example.com")]
    [InlineData(false, true, 10, "user@example.com")]
    [InlineData(false, false, -1, "user@example.com")]
    [InlineData(false, false, 10, "")]
    [InlineData(false, false, 10, "user@example.com\nInjected log line")]
    public async Task ProcessAsync_RejectsMalformedOrdersWithoutPublishing(
        bool missingUser, bool missingGame, decimal price, string email)
    {
        var publisher = new RecordingPaymentResultPublisher();
        var processor = new PaymentProcessor(new PaymentSimulationOptions(), publisher);
        var order = new OrderPlacedEvent(
            missingUser ? Guid.Empty : Guid.NewGuid(),
            missingGame ? Guid.Empty : Guid.NewGuid(), price, email);

        await Assert.ThrowsAsync<ArgumentException>(() => processor.ProcessAsync(order, CancellationToken.None));
        Assert.Empty(publisher.Published);
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
