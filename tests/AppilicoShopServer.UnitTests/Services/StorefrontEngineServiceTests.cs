using AppilicoShopServer.Business.DTOs.Storefront;
using AppilicoShopServer.Business.Options;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace AppilicoShopServer.UnitTests.Services;

/// <summary>Covers the generic storefront engine features: persistence, shipping/tax quoting, multi-store and homepage.</summary>
public class StorefrontEngineServiceTests
{
    [Fact]
    public async Task UpdateConfigAsync_PersistsOverride_AndIsReturnedOnNextRead()
    {
        var service = CreateService(out _);

        var update = new StorefrontEditableConfigDto
        {
            StoreName = "Persisted Store",
            Tagline = "Persisted tagline",
            Currency = "USD"
        };

        var saved = await service.UpdateConfigAsync(update, "admin@appilico.com");
        saved.Success.Should().BeTrue();
        saved.Data!.StoreName.Should().Be("Persisted Store");

        var reloaded = await service.GetEditableConfigAsync();
        reloaded.Data!.StoreName.Should().Be("Persisted Store");
        reloaded.Data.Tagline.Should().Be("Persisted tagline");
        reloaded.Data.Currency.Should().Be("USD");
        reloaded.Data.UpdatedBy.Should().Be("admin@appilico.com");
    }

    [Fact]
    public async Task UpdateConfigAsync_NullPayload_Fails()
    {
        var service = CreateService(out _);

        var result = await service.UpdateConfigAsync(null!, "admin");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateConfigAsync_PartialOverride_KeepsDefaultsForUntouchedFields()
    {
        var options = new StorefrontOptions { StoreName = "Default Store", Tagline = "Default tagline", Currency = "AUD" };
        var service = CreateService(out _, options);

        await service.UpdateConfigAsync(new StorefrontEditableConfigDto { StoreName = "Only Name Changed" }, "admin");

        var reloaded = await service.GetEditableConfigAsync();
        reloaded.Data!.StoreName.Should().Be("Only Name Changed");
        reloaded.Data.Tagline.Should().Be("Default tagline");
        reloaded.Data.Currency.Should().Be("AUD");
    }

    [Theory]
    [InlineData("flat", 50, 9.99, false)]
    [InlineData("flat", 120, 0, true)]
    public async Task GetShippingQuoteAsync_FlatStrategy_AppliesFreeShippingThreshold(string strategy, decimal subtotal, decimal expectedShipping, bool expectedFree)
    {
        var options = new StorefrontOptions
        {
            Shipping = new StorefrontShippingPolicyOptions { Strategy = strategy, FlatRate = 9.99m, FreeShippingThreshold = 100m, HandlingFee = 0m, Currency = "AUD" }
        };
        var service = CreateService(out _, options);

        var result = await service.GetShippingQuoteAsync(new ShippingQuoteRequestDto { Subtotal = subtotal, ItemCount = 2 });

        result.Success.Should().BeTrue();
        result.Data!.ShippingCost.Should().Be(expectedShipping);
        result.Data.FreeShippingEligible.Should().Be(expectedFree);
    }

    [Fact]
    public async Task GetShippingQuoteAsync_WeightStrategy_ChargesPerKilogram()
    {
        var options = new StorefrontOptions
        {
            Shipping = new StorefrontShippingPolicyOptions { Strategy = "weight", PerKgRate = 5m, FreeShippingThreshold = 0m, HandlingFee = 2m, Currency = "AUD" }
        };
        var service = CreateService(out _, options);

        var result = await service.GetShippingQuoteAsync(new ShippingQuoteRequestDto { Subtotal = 40m, ItemCount = 1, TotalWeightKg = 3m });

        result.Data!.ShippingCost.Should().Be(15m);
        result.Data.HandlingFee.Should().Be(2m);
        result.Data.Total.Should().Be(17m);
        result.Data.FreeShippingEligible.Should().BeFalse();
    }

    [Fact]
    public async Task GetShippingQuoteAsync_FreeStrategy_AlwaysFree()
    {
        var options = new StorefrontOptions
        {
            Shipping = new StorefrontShippingPolicyOptions { Strategy = "free", FreeShippingThreshold = 0m }
        };
        var service = CreateService(out _, options);

        var result = await service.GetShippingQuoteAsync(new ShippingQuoteRequestDto { Subtotal = 10m, ItemCount = 1 });

        result.Data!.ShippingCost.Should().Be(0m);
        result.Data.Total.Should().Be(0m);
    }

    [Fact]
    public async Task GetShippingQuoteAsync_NegativeSubtotal_Fails()
    {
        var service = CreateService(out _);

        var result = await service.GetShippingQuoteAsync(new ShippingQuoteRequestDto { Subtotal = -1m });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetTaxQuoteAsync_PercentageStrategy_AddsExclusiveTax()
    {
        var options = new StorefrontOptions
        {
            Tax = new StorefrontTaxPolicyOptions { Strategy = "percentage", RatePercent = 10m, AppliesToShipping = false, Label = "GST" }
        };
        var service = CreateService(out _, options);

        var result = await service.GetTaxQuoteAsync(new TaxQuoteRequestDto { Subtotal = 100m, ShippingAmount = 10m });

        result.Data!.Strategy.Should().Be("percentage");
        result.Data.TaxAmount.Should().Be(10m);
        result.Data.TotalWithTax.Should().Be(120m);
    }

    [Fact]
    public async Task GetTaxQuoteAsync_InclusiveStrategy_ExtractsEmbeddedTax()
    {
        var options = new StorefrontOptions
        {
            Tax = new StorefrontTaxPolicyOptions { Strategy = "inclusive", RatePercent = 10m }
        };
        var service = CreateService(out _, options);

        var result = await service.GetTaxQuoteAsync(new TaxQuoteRequestDto { Subtotal = 110m, ShippingAmount = 0m });

        result.Data!.Strategy.Should().Be("inclusive");
        result.Data.PricesIncludeTax.Should().BeTrue();
        result.Data.TaxAmount.Should().Be(10m);
        result.Data.TotalWithTax.Should().Be(110m);
    }

    [Fact]
    public async Task GetTaxQuoteAsync_NoneStrategy_ChargesNoTax()
    {
        var options = new StorefrontOptions
        {
            Tax = new StorefrontTaxPolicyOptions { Strategy = "none" }
        };
        var service = CreateService(out _, options);

        var result = await service.GetTaxQuoteAsync(new TaxQuoteRequestDto { Subtotal = 100m, ShippingAmount = 10m });

        result.Data!.TaxAmount.Should().Be(0m);
        result.Data.TotalWithTax.Should().Be(110m);
    }

    [Fact]
    public async Task ListStorefrontsAsync_AlwaysIncludesDefault()
    {
        var service = CreateService(out _);

        var result = await service.ListStorefrontsAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().ContainSingle(s => s.IsDefault);
    }

    [Fact]
    public async Task UpsertStorefrontAsync_AddsTenant_ThenListedAndDeletable()
    {
        var service = CreateService(out _);

        var upsert = await service.UpsertStorefrontAsync(new StorefrontUpsertRequestDto
        {
            StorefrontKey = "tenant-a",
            StoreName = "Tenant A Store",
            Tagline = "Tenant A"
        }, "admin");

        upsert.Success.Should().BeTrue();
        upsert.Data!.StorefrontKey.Should().Be("tenant-a");
        upsert.Data.IsDefault.Should().BeFalse();

        var list = await service.ListStorefrontsAsync();
        list.Data.Should().Contain(s => s.StorefrontKey == "tenant-a" && s.StoreName == "Tenant A Store");

        var deleted = await service.DeleteStorefrontAsync("tenant-a", "admin");
        deleted.Success.Should().BeTrue();

        var afterDelete = await service.ListStorefrontsAsync();
        afterDelete.Data.Should().NotContain(s => s.StorefrontKey == "tenant-a");
    }

    [Fact]
    public async Task UpsertStorefrontAsync_MissingKey_Fails()
    {
        var service = CreateService(out _);

        var result = await service.UpsertStorefrontAsync(new StorefrontUpsertRequestDto { StorefrontKey = "" }, "admin");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteStorefrontAsync_DefaultKey_IsRejected()
    {
        var service = CreateService(out _);

        var result = await service.DeleteStorefrontAsync("default", "admin");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetHomePageAsync_ReturnsEnabledSectionsInSortOrder()
    {
        var service = CreateService(out _);

        var result = await service.GetHomePageAsync();

        result.Success.Should().BeTrue();
        result.Data!.Sections.Should().NotBeEmpty();
        result.Data.Sections.Should().BeInAscendingOrder(s => s.SortOrder);
        result.Data.Sections.Should().OnlyContain(s => s.Enabled);
    }

    /// <summary>Creates a service backed by an in-memory <see cref="AppSetting"/> store so persistence round-trips work.</summary>
    private static StorefrontService CreateService(out Dictionary<string, AppSetting> store, StorefrontOptions? options = null)
    {
        var backing = new Dictionary<string, AppSetting>(StringComparer.OrdinalIgnoreCase);
        store = backing;

        var repo = new Mock<IAppSettingRepository>();
        repo.Setup(r => r.GetByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync((string key) => backing.TryGetValue(key, out var setting) ? setting : null);
        repo.Setup(r => r.AddAsync(It.IsAny<AppSetting>()))
            .ReturnsAsync((AppSetting setting) => { backing[setting.Key] = setting; return setting; });
        repo.Setup(r => r.Update(It.IsAny<AppSetting>()))
            .Callback((AppSetting setting) => backing[setting.Key] = setting);
        repo.Setup(r => r.SoftDelete(It.IsAny<AppSetting>()))
            .Callback((AppSetting setting) => { setting.IsDeleted = true; backing[setting.Key] = setting; });

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Settings).Returns(repo.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        return new StorefrontService(Microsoft.Extensions.Options.Options.Create(options ?? new StorefrontOptions()), unitOfWork.Object);
    }
}
