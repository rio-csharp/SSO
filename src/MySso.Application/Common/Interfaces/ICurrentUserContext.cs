namespace MySso.Application.Common.Interfaces;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    string SubjectId { get; }

    string? DisplayName { get; }

    string? IpAddress { get; }

    bool IsInRole(string roleName);
}