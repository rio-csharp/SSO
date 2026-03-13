namespace MySso.Application.Common.Interfaces;

public interface IIdentityAccountProvisioningService
{
    Task ProvisionLocalUserAsync(
        Guid domainUserId,
        string email,
        string givenName,
        string familyName,
        string password,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken);
}