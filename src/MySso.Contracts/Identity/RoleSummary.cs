namespace MySso.Contracts.Identity;

public sealed record RoleSummary(Guid Id, string Name, string Description, bool IsSystemRole);