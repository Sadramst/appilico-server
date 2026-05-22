using Appilico.Server.Business.Exceptions;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Business.Options;
using Appilico.Server.Business.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Appilico.Server.UnitTests.Services;

public class FailClosedIntegrationTests
{
    [Fact]
    public async Task StripePaymentIntent_WhenDisabled_ThrowsControlledProviderException()
    {
        var service = new StripePaymentService(
            Microsoft.Extensions.Options.Options.Create(new StripeOptions { Enabled = false }),
            new Mock<ILogger<StripePaymentService>>().Object);

        var action = () => service.CreatePaymentIntentAsync(new StripePaymentIntentRequest(
            100m,
            "aud",
            "test order",
            "test-key",
            new Dictionary<string, string> { ["orderId"] = "order-1" }));

        await action.Should().ThrowAsync<PaymentProviderException>()
            .WithMessage("*Stripe is not configured*");
    }

    [Fact]
    public void StripeWebhookVerification_WhenNotImplemented_ReturnsFalse()
    {
        var service = new StripePaymentService(
            Microsoft.Extensions.Options.Options.Create(new StripeOptions
            {
                Enabled = true,
                SecretKey = "sk_test",
                PublishableKey = "pk_test",
                WebhookSecret = "whsec_test",
                StarterPriceId = "price_starter",
                ProfessionalPriceId = "price_professional",
                EnterprisePriceId = "price_enterprise"
            }),
            new Mock<ILogger<StripePaymentService>>().Object);

        var result = service.VerifyWebhookSignature("{}", "signature", "whsec_test");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AzureBlobStorage_WhenNotImplemented_ThrowsControlledNotSupportedException()
    {
        var service = new AzureBlobStorageService(
            Microsoft.Extensions.Options.Options.Create(new AzureStorageOptions { Enabled = false }),
            new Mock<ILogger<AzureBlobStorageService>>().Object);

        var upload = () => service.UploadAsync(Stream.Null, "visual.pbiviz", "application/octet-stream");
        var download = () => service.GetPresignedUrlAsync("https://example.test/visual.pbiviz");

        await upload.Should().ThrowAsync<StorageProviderException>()
            .WithMessage("*Azure Blob Storage is not configured*");
        await download.Should().ThrowAsync<StorageProviderException>()
            .WithMessage("*Azure Blob Storage is not configured*");
    }
}