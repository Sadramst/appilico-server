using FluentAssertions;
using Appilico.Server.Business.Validators.Product;
using Appilico.Server.Business.Validators.Auth;
using Appilico.Server.Business.Validators.Category;
using Appilico.Server.Business.DTOs.Product;
using Appilico.Server.Business.DTOs.Auth;
using Appilico.Server.Business.DTOs.Category;
using Appilico.Server.Business.DTOs.Brand;

namespace Appilico.Server.UnitTests.Validators;

public class ValidatorTests
{
    [Fact]
    public void CreateProductValidator_InvalidRequest_ShouldFail()
    {
        var validator = new CreateProductRequestValidator();
        var request = new CreateProductRequest { Name = "", SKU = "", BasePrice = -1 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateProductValidator_ValidRequest_ShouldPass()
    {
        var validator = new CreateProductRequestValidator();
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            SKU = "TST-001",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            BasePrice = 29.99m,
            CostPrice = 15m,
            StockQuantity = 10,
            MinStockLevel = 5
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LoginValidator_EmptyEmail_ShouldFail()
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest { Email = "", Password = "test" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RegisterValidator_ShortPassword_ShouldFail()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "short",
            ConfirmPassword = "short",
            FirstName = "Test",
            LastName = "User"
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RegisterValidator_ValidRequest_ShouldPass()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "StrongPass@123",
            ConfirmPassword = "StrongPass@123",
            FirstName = "Test",
            LastName = "User"
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateCategoryValidator_EmptyName_ShouldFail()
    {
        var validator = new CreateCategoryRequestValidator();
        var request = new CreateCategoryRequest { Name = "" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateCategoryValidator_ValidRequest_ShouldPass()
    {
        var validator = new CreateCategoryRequestValidator();
        var request = new CreateCategoryRequest { Name = "Electronics", SortOrder = 1 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
