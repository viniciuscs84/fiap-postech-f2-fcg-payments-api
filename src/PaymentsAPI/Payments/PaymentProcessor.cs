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
        var payment = new PaymentProcessedEvent(
            order.UserId,
            order.GameId,
            outcome,
            order.RecipientEmail);

        return publisher.PublishAsync(payment, cancellationToken);
    }
}
