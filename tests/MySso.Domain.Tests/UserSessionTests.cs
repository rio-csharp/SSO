using MySso.Domain.Entities;
using MySso.Domain.Enums;

namespace MySso.Domain.Tests;

public sealed class UserSessionTests
{
    [Fact]
    public void Revoke_Changes_State_And_Records_Metadata()
    {
        var createdAt = new DateTimeOffset(2026, 3, 13, 0, 0, 0, TimeSpan.Zero);
        var session = UserSession.Start(Guid.NewGuid(), Guid.NewGuid(), "user-1", "client-1", createdAt, createdAt.AddHours(1));

        session.Revoke("admin-1", SessionRevocationReason.SecurityIncident, createdAt.AddMinutes(5));

        Assert.True(session.IsRevoked);
        Assert.Equal("admin-1", session.RevokedBy);
        Assert.Equal(SessionRevocationReason.SecurityIncident, session.RevocationReason);
    }

    [Fact]
    public void Revoke_Cannot_Run_Twice()
    {
        var createdAt = new DateTimeOffset(2026, 3, 13, 0, 0, 0, TimeSpan.Zero);
        var session = UserSession.Start(Guid.NewGuid(), Guid.NewGuid(), "user-1", "client-1", createdAt, createdAt.AddHours(1));

        session.Revoke("admin-1", SessionRevocationReason.SecurityIncident, createdAt.AddMinutes(5));

        Assert.Throws<InvalidOperationException>(() => session.Revoke("admin-1", SessionRevocationReason.UserRequested, createdAt.AddMinutes(10)));
    }
}