namespace MySso.Application.Features.Roles;

public sealed record CreateRoleCommand(string Name, string Description, bool IsSystemRole = false);