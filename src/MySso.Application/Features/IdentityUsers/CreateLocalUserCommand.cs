namespace MySso.Application.Features.IdentityUsers;

public sealed record CreateLocalUserCommand(
    string Email,
    string GivenName,
    string FamilyName,
    string Password,
    bool AssignAdministratorRole);