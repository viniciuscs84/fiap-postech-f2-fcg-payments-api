namespace PaymentsAPI.Contracts;

public sealed record PaymentProcessedEvent(
    Guid UserId,
    Guid GameId,
    string Status,
    string RecipientEmail);
