namespace MySso.Application.Features.UserProfile;

public sealed record UpdateUserProfileCommand(Guid UserId, string GivenName, string FamilyName);