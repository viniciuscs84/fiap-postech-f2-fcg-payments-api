namespace PaymentsAPI.Payments;

public sealed class PaymentSimulationOptions
{
    public const string SectionName = "PaymentSimulation";

    public string Outcome { get; init; } = PaymentOutcomes.Approved;
}
