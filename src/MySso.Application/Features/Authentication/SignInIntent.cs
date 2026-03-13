namespace MySso.Application.Features.Authentication;

public sealed record SignInIntent(string LoginHint, string? ReturnUrl);