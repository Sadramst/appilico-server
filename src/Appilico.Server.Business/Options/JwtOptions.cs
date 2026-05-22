namespace Appilico.Server.Business.Options;

/// <summary>JWT configuration bound from the JWT section.</summary>
public sealed class JwtOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "JWT";

    /// <summary>JWT signing secret.</summary>
    public string? Secret { get; set; }

    /// <summary>JWT issuer.</summary>
    public string? Issuer { get; set; }

    /// <summary>JWT audience.</summary>
    public string? Audience { get; set; }

    /// <summary>Access token lifetime in minutes.</summary>
    public int ExpirationInMinutes { get; set; } = 60;

    /// <summary>Refresh token lifetime in days.</summary>
    public int RefreshTokenExpirationInDays { get; set; } = 7;

    /// <summary>Checks whether the configured secret is strong enough for HMAC signing.</summary>
    public bool HasStrongSecret =>
        !string.IsNullOrWhiteSpace(Secret) &&
        Secret.Length >= 32 &&
        !Secret.Contains("will-be-overridden", StringComparison.OrdinalIgnoreCase) &&
        !Secret.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase);

    /// <summary>Checks whether required issuer/audience values are present.</summary>
    public bool HasRequiredIssuerAudience =>
        !string.IsNullOrWhiteSpace(Issuer) &&
        !string.IsNullOrWhiteSpace(Audience);
}