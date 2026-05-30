using AppilicoShopServer.Business.Options;
using FluentAssertions;

namespace AppilicoShopServer.UnitTests.Options;

public class ConfigurationOptionTests
{
    [Fact]
    public void JwtOptions_RejectsPlaceholderOrWeakSecrets()
    {
        new JwtOptions { Secret = "will-be-overridden-by-user-secrets", Issuer = "issuer", Audience = "audience" }
            .HasStrongSecret.Should().BeFalse();

        new JwtOptions { Secret = "short", Issuer = "issuer", Audience = "audience" }
            .HasStrongSecret.Should().BeFalse();

        new JwtOptions { Secret = "StrongJwtSecretValueForTests123456", Issuer = "issuer", Audience = "audience" }
            .HasStrongSecret.Should().BeTrue();
    }

    [Fact]
    public void EnabledIntegrationOptions_RequireNonPlaceholderSettings()
    {
        new StripeOptions { Enabled = true, SecretKey = "will-be-overridden-by-user-secrets" }
            .HasRequiredSettings.Should().BeFalse();

        new AzureStorageOptions { Enabled = true, ConnectionString = "will-be-overridden-by-user-secrets", ContainerName = "visuals" }
            .HasRequiredSettings.Should().BeFalse();

        new EmailOptions { Enabled = true, SmtpHost = "smtp.example.test", SmtpUser = "user", SmtpPass = "pass", FromEmail = "noreply@example.test" }
            .HasRequiredSettings.Should().BeTrue();
    }
}