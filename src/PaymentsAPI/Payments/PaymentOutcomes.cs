namespace PaymentsAPI.Payments;

public static class PaymentOutcomes
{
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public static bool IsSupported(string outcome) =>
        string.Equals(outcome, Approved, StringComparison.Ordinal) ||
        string.Equals(outcome, Rejected, StringComparison.Ordinal);
}
