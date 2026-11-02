using PaymentsAPI.Contracts;

namespace PaymentsAPI.Payments;

public sealed class PaymentProcessor : IPaymentProcessor
{
    private readonly string outcome;
    private readonly IPaymentResultPublisher publisher;

    public PaymentProcessor(
        PaymentSimulationOptions options,
        IPaymentResultPublisher publisher)
    {
        if (!PaymentOutcomes.IsSupported(options.Outcome))
        {
            throw new InvalidOperationException(
                $"PaymentSimulation:Outcome must be '{PaymentOutcomes.Approved}' or '{PaymentOutcomes.Rejected}'.");
        }

        outcome = options.Outcome;
        this.publisher = publisher;
    }

    public Task ProcessAsync(OrderPlacedEvent order, CancellationToken cancellationToken)
    {
        if (order.UserId == Guid.Empty || order.GameId == Guid.Empty ||
            order.Price < 0 || string.IsNullOrWhiteSpace(order.RecipientEmail) ||
            order.RecipientEmail.IndexOfAny(['\r', '\n']) >= 0 ||
            !System.Net.Mail.MailAddress.TryCreate(order.RecipientEmail, out var address) ||
            !string.Equals(address.Address, order.RecipientEmail, StringComparison.Ordinal))
            throw new ArgumentException("OrderPlacedEvent contains invalid purchase data.", nameof(order));

        var payment = new PaymentProcessedEvent(
            order.UserId,
            order.GameId,
            outcome,
            order.RecipientEmail);

        return publisher.PublishAsync(payment, cancellationToken);
    }
}
