namespace MySso.Contracts.Identity;

public sealed record UserSummary(Guid Id, string Email, string GivenName, string FamilyName, bool IsActive);