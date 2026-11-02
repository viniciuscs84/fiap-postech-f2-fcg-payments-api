using PaymentsAPI.Contracts;

namespace PaymentsAPI.Payments;

public interface IPaymentProcessor
{
    Task ProcessAsync(OrderPlacedEvent order, CancellationToken cancellationToken);
}
