namespace PaymentsAPI.Contracts;

public sealed record OrderPlacedEvent(
    Guid UserId,
    Guid GameId,
    decimal Price,
    string RecipientEmail);
