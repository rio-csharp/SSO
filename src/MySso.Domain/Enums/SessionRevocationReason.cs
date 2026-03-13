namespace MySso.Domain.Enums;

public enum SessionRevocationReason
{
    UserRequested = 1,
    AdministratorForced = 2,
    SecurityIncident = 3,
    PasswordChanged = 4,
    AccountDisabled = 5
}