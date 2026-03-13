namespace MySso.Application.Features.IdentityUsers;

public sealed record CreateUserCommand(string Email, string GivenName, string FamilyName);