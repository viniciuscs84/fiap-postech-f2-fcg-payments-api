using PaymentsAPI.Contracts;

namespace PaymentsAPI.Payments;

public interface IPaymentResultPublisher
{
    Task PublishAsync(PaymentProcessedEvent payment, CancellationToken cancellationToken);
}
