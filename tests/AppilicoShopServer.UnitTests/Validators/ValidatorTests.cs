using FluentAssertions;
using AppilicoShopServer.Business.Validators.Product;
using AppilicoShopServer.Business.Validators.Auth;
using AppilicoShopServer.Business.Validators.Category;
using AppilicoShopServer.Business.Validators.Brand;
using AppilicoShopServer.Business.Validators.Cart;
using AppilicoShopServer.Business.Validators.Discount;
using AppilicoShopServer.Business.Validators.Order;
using AppilicoShopServer.Business.Validators.Payment;
using AppilicoShopServer.Business.Validators.Review;
using AppilicoShopServer.Business.Validators.Inventory;
using AppilicoShopServer.Business.Validators.Voucher;
using AppilicoShopServer.Business.DTOs.Product;
using AppilicoShopServer.Business.DTOs.Auth;
using AppilicoShopServer.Business.DTOs.Category;
using AppilicoShopServer.Business.DTOs.Brand;
using AppilicoShopServer.Business.DTOs.Cart;
using AppilicoShopServer.Business.DTOs.Discount;
using AppilicoShopServer.Business.DTOs.Order;
using AppilicoShopServer.Business.DTOs.Payment;
using AppilicoShopServer.Business.DTOs.Review;
using AppilicoShopServer.Business.DTOs.Inventory;
using AppilicoShopServer.Business.DTOs.Voucher;
using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.UnitTests.Validators;

public class ValidatorTests
{
    // ──────── Product Validators ────────

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
    public void CreateProductValidator_ZeroPrice_ShouldFail()
    {
        var validator = new CreateProductRequestValidator();
        var request = new CreateProductRequest { Name = "Test", SKU = "T1", BasePrice = 0 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    // ──────── Auth Validators ────────

    [Fact]
    public void LoginValidator_EmptyEmail_ShouldFail()
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest { Email = "", Password = "test" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void LoginValidator_EmptyPassword_ShouldFail()
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest { Email = "test@test.com", Password = "" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void LoginValidator_ValidRequest_ShouldPass()
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest { Email = "test@test.com", Password = "MyPassword123!" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
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
    public void RegisterValidator_EmptyFirstName_ShouldFail()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "StrongPass@123",
            ConfirmPassword = "StrongPass@123",
            FirstName = "",
            LastName = "User"
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RegisterValidator_EmptyEmail_ShouldFail()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Email = "",
            Password = "StrongPass@123",
            ConfirmPassword = "StrongPass@123",
            FirstName = "Test",
            LastName = "User"
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    // ──────── Category Validators ────────

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

    // ──────── Brand Validators ────────

    [Fact]
    public void CreateBrandValidator_EmptyName_ShouldFail()
    {
        var validator = new CreateBrandRequestValidator();
        var request = new CreateBrandRequest { Name = "" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateBrandValidator_ValidRequest_ShouldPass()
    {
        var validator = new CreateBrandRequestValidator();
        var request = new CreateBrandRequest { Name = "TechPro" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateBrandValidator_EmptyName_ShouldFail()
    {
        var validator = new UpdateBrandRequestValidator();
        var request = new UpdateBrandRequest { Name = "" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateBrandValidator_ValidRequest_ShouldPass()
    {
        var validator = new UpdateBrandRequestValidator();
        var request = new UpdateBrandRequest { Name = "Updated Brand" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateBrandValidator_NameTooLong_ShouldFail()
    {
        var validator = new CreateBrandRequestValidator();
        var request = new CreateBrandRequest { Name = new string('A', 201) };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    // ──────── Cart Validators ────────

    [Fact]
    public void AddToCartValidator_EmptyProductId_ShouldFail()
    {
        var validator = new AddToCartRequestValidator();
        var request = new AddToCartRequest { ProductId = Guid.Empty, Quantity = 1 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AddToCartValidator_ZeroQuantity_ShouldFail()
    {
        var validator = new AddToCartRequestValidator();
        var request = new AddToCartRequest { ProductId = Guid.NewGuid(), Quantity = 0 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AddToCartValidator_ValidRequest_ShouldPass()
    {
        var validator = new AddToCartRequestValidator();
        var request = new AddToCartRequest { ProductId = Guid.NewGuid(), Quantity = 3 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateCartItemValidator_ZeroQuantity_ShouldFail()
    {
        var validator = new UpdateCartItemRequestValidator();
        var request = new UpdateCartItemRequest { Quantity = 0 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateCartItemValidator_ValidQuantity_ShouldPass()
    {
        var validator = new UpdateCartItemRequestValidator();
        var request = new UpdateCartItemRequest { Quantity = 5 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AddToCartValidator_NegativeQuantity_ShouldFail()
    {
        var validator = new AddToCartRequestValidator();
        var request = new AddToCartRequest { ProductId = Guid.NewGuid(), Quantity = -1 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    // ──────── Discount Validators ────────

    [Fact]
    public void CreateDiscountValidator_EmptyCode_ShouldFail()
    {
        var validator = new CreateDiscountRequestValidator();
        var request = new CreateDiscountRequest
        {
            Code = "", Name = "Test", Value = 10,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateDiscountValidator_ZeroValue_ShouldFail()
    {
        var validator = new CreateDiscountRequestValidator();
        var request = new CreateDiscountRequest
        {
            Code = "TEST", Name = "Test", Value = 0,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateDiscountValidator_EndDateBeforeStart_ShouldFail()
    {
        var validator = new CreateDiscountRequestValidator();
        var request = new CreateDiscountRequest
        {
            Code = "TEST", Name = "Test", Value = 10,
            StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateDiscountValidator_ValidRequest_ShouldPass()
    {
        var validator = new CreateDiscountRequestValidator();
        var request = new CreateDiscountRequest
        {
            Code = "SUMMER10", Name = "Summer Sale", Value = 10,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    // ──────── Order Validators ────────

    [Fact]
    public void CreateOrderValidator_EmptyShippingAddress_ShouldFail()
    {
        var validator = new CreateOrderRequestValidator();
        var request = new CreateOrderRequest { ShippingAddressId = Guid.Empty, BillingAddressId = Guid.NewGuid() };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateOrderValidator_EmptyBillingAddress_ShouldFail()
    {
        var validator = new CreateOrderRequestValidator();
        var request = new CreateOrderRequest { ShippingAddressId = Guid.NewGuid(), BillingAddressId = Guid.Empty };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateOrderValidator_ValidRequest_ShouldPass()
    {
        var validator = new CreateOrderRequestValidator();
        var request = new CreateOrderRequest { ShippingAddressId = Guid.NewGuid(), BillingAddressId = Guid.NewGuid() };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    // ──────── Payment Validators ────────

    [Fact]
    public void CreatePaymentValidator_EmptyOrderId_ShouldFail()
    {
        var validator = new CreatePaymentRequestValidator();
        var request = new CreatePaymentRequest { OrderId = Guid.Empty, Amount = 100m, PaymentMethod = PaymentMethod.CreditCard };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreatePaymentValidator_ZeroAmount_ShouldFail()
    {
        var validator = new CreatePaymentRequestValidator();
        var request = new CreatePaymentRequest { OrderId = Guid.NewGuid(), Amount = 0, PaymentMethod = PaymentMethod.CreditCard };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreatePaymentValidator_ValidRequest_ShouldPass()
    {
        var validator = new CreatePaymentRequestValidator();
        var request = new CreatePaymentRequest { OrderId = Guid.NewGuid(), Amount = 100m, PaymentMethod = PaymentMethod.CreditCard };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateRefundValidator_ZeroAmount_ShouldFail()
    {
        var validator = new CreateRefundRequestValidator();
        var request = new CreateRefundRequest { Amount = 0 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateRefundValidator_ValidAmount_ShouldPass()
    {
        var validator = new CreateRefundRequestValidator();
        var request = new CreateRefundRequest { Amount = 50m, Reason = "Defective" };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    // ──────── Review Validators ────────

    [Fact]
    public void CreateReviewValidator_EmptyProductId_ShouldFail()
    {
        var validator = new CreateReviewRequestValidator();
        var request = new CreateReviewRequest { ProductId = Guid.Empty, Rating = 5 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateReviewValidator_RatingTooHigh_ShouldFail()
    {
        var validator = new CreateReviewRequestValidator();
        var request = new CreateReviewRequest { ProductId = Guid.NewGuid(), Rating = 6 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateReviewValidator_RatingTooLow_ShouldFail()
    {
        var validator = new CreateReviewRequestValidator();
        var request = new CreateReviewRequest { ProductId = Guid.NewGuid(), Rating = 0 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateReviewValidator_ValidRequest_ShouldPass()
    {
        var validator = new CreateReviewRequestValidator();
        var request = new CreateReviewRequest { ProductId = Guid.NewGuid(), Rating = 4 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateReviewValidator_RatingOutOfRange_ShouldFail()
    {
        var validator = new UpdateReviewRequestValidator();
        var request = new UpdateReviewRequest { Rating = 10 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateReviewValidator_ValidRating_ShouldPass()
    {
        var validator = new UpdateReviewRequestValidator();
        var request = new UpdateReviewRequest { Rating = 3 };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    // ──────── Inventory Validators ────────

    [Fact]
    public void AdjustInventoryValidator_EmptyProductId_ShouldFail()
    {
        var validator = new AdjustInventoryRequestValidator();
        var request = new AdjustInventoryRequest { ProductId = Guid.Empty, Quantity = 10, TransactionType = InventoryTransactionType.StockIn };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AdjustInventoryValidator_ZeroQuantity_ShouldFail()
    {
        var validator = new AdjustInventoryRequestValidator();
        var request = new AdjustInventoryRequest { ProductId = Guid.NewGuid(), Quantity = 0, TransactionType = InventoryTransactionType.StockIn };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AdjustInventoryValidator_ValidRequest_ShouldPass()
    {
        var validator = new AdjustInventoryRequestValidator();
        var request = new AdjustInventoryRequest { ProductId = Guid.NewGuid(), Quantity = 10, TransactionType = InventoryTransactionType.StockIn };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    // ──────── Voucher Validators ────────

    [Fact]
    public void CreateVoucherValidator_EmptyCode_ShouldFail()
    {
        var validator = new CreateVoucherRequestValidator();
        var request = new CreateVoucherRequest { Code = "", Value = 10, StartDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddDays(30) };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateVoucherValidator_ZeroValue_ShouldFail()
    {
        var validator = new CreateVoucherRequestValidator();
        var request = new CreateVoucherRequest { Code = "V1", Value = 0, StartDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddDays(30) };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateVoucherValidator_ExpiryBeforeStart_ShouldFail()
    {
        var validator = new CreateVoucherRequestValidator();
        var request = new CreateVoucherRequest { Code = "V1", Value = 10, StartDate = DateTime.UtcNow.AddDays(10), ExpiryDate = DateTime.UtcNow };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateVoucherValidator_ValidRequest_ShouldPass()
    {
        var validator = new CreateVoucherRequestValidator();
        var request = new CreateVoucherRequest { Code = "GIFT50", Value = 50, StartDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddDays(30) };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
