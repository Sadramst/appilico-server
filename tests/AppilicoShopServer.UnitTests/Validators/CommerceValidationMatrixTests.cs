using AppilicoShopServer.Business.DTOs.Auth;
using AppilicoShopServer.Business.DTOs.Brand;
using AppilicoShopServer.Business.DTOs.Cart;
using AppilicoShopServer.Business.DTOs.Category;
using AppilicoShopServer.Business.DTOs.Discount;
using AppilicoShopServer.Business.DTOs.Inventory;
using AppilicoShopServer.Business.DTOs.Order;
using AppilicoShopServer.Business.DTOs.Payment;
using AppilicoShopServer.Business.DTOs.Product;
using AppilicoShopServer.Business.DTOs.Review;
using AppilicoShopServer.Business.DTOs.Voucher;
using AppilicoShopServer.Business.Validators.Auth;
using AppilicoShopServer.Business.Validators.Brand;
using AppilicoShopServer.Business.Validators.Cart;
using AppilicoShopServer.Business.Validators.Category;
using AppilicoShopServer.Business.Validators.Discount;
using AppilicoShopServer.Business.Validators.Inventory;
using AppilicoShopServer.Business.Validators.Order;
using AppilicoShopServer.Business.Validators.Payment;
using AppilicoShopServer.Business.Validators.Product;
using AppilicoShopServer.Business.Validators.Review;
using AppilicoShopServer.Business.Validators.Voucher;
using AppilicoShopServer.Domain.Enums;
using FluentAssertions;

namespace AppilicoShopServer.UnitTests.Validators;

public class CommerceValidationMatrixTests
{
    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public void InvalidCommerceRequests_ShouldFail(string scenario, object validator, object request)
    {
        IsValid(validator, request).Should().BeFalse(scenario);
    }

    [Theory]
    [MemberData(nameof(ValidRequests))]
    public void ValidCommerceRequests_ShouldPass(string scenario, object validator, object request)
    {
        IsValid(validator, request).Should().BeTrue(scenario);
    }

    public static IEnumerable<object[]> InvalidRequests()
    {
        yield return Invalid("Create product requires name", new CreateProductRequestValidator(), Product(request => request.Name = string.Empty));
        yield return Invalid("Create product rejects long name", new CreateProductRequestValidator(), Product(request => request.Name = new string('A', 301)));
        yield return Invalid("Create product requires SKU", new CreateProductRequestValidator(), Product(request => request.SKU = string.Empty));
        yield return Invalid("Create product rejects long SKU", new CreateProductRequestValidator(), Product(request => request.SKU = new string('S', 51)));
        yield return Invalid("Create product requires category", new CreateProductRequestValidator(), Product(request => request.CategoryId = Guid.Empty));
        yield return Invalid("Create product requires brand", new CreateProductRequestValidator(), Product(request => request.BrandId = Guid.Empty));
        yield return Invalid("Create product rejects zero base price", new CreateProductRequestValidator(), Product(request => request.BasePrice = 0));
        yield return Invalid("Create product rejects negative base price", new CreateProductRequestValidator(), Product(request => request.BasePrice = -1));
        yield return Invalid("Create product rejects negative cost", new CreateProductRequestValidator(), Product(request => request.CostPrice = -0.01m));
        yield return Invalid("Create product rejects negative stock", new CreateProductRequestValidator(), Product(request => request.StockQuantity = -1));
        yield return Invalid("Create product rejects negative minimum stock", new CreateProductRequestValidator(), Product(request => request.MinStockLevel = -1));

        yield return Invalid("Update product requires name", new UpdateProductRequestValidator(), UpdateProduct(request => request.Name = string.Empty));
        yield return Invalid("Update product rejects long name", new UpdateProductRequestValidator(), UpdateProduct(request => request.Name = new string('A', 301)));
        yield return Invalid("Update product requires category", new UpdateProductRequestValidator(), UpdateProduct(request => request.CategoryId = Guid.Empty));
        yield return Invalid("Update product requires brand", new UpdateProductRequestValidator(), UpdateProduct(request => request.BrandId = Guid.Empty));
        yield return Invalid("Update product rejects zero base price", new UpdateProductRequestValidator(), UpdateProduct(request => request.BasePrice = 0));
        yield return Invalid("Update product rejects negative base price", new UpdateProductRequestValidator(), UpdateProduct(request => request.BasePrice = -1));
        yield return Invalid("Update product rejects negative cost", new UpdateProductRequestValidator(), UpdateProduct(request => request.CostPrice = -1));
        yield return Invalid("Update product rejects negative stock", new UpdateProductRequestValidator(), UpdateProduct(request => request.StockQuantity = -1));
        yield return Invalid("Update product rejects negative minimum stock", new UpdateProductRequestValidator(), UpdateProduct(request => request.MinStockLevel = -1));

        yield return Invalid("Variant requires name", new CreateProductVariantRequestValidator(), Variant(request => request.VariantName = string.Empty));
        yield return Invalid("Variant rejects long name", new CreateProductVariantRequestValidator(), Variant(request => request.VariantName = new string('V', 201)));
        yield return Invalid("Variant requires SKU", new CreateProductVariantRequestValidator(), Variant(request => request.SKU = string.Empty));
        yield return Invalid("Variant rejects long SKU", new CreateProductVariantRequestValidator(), Variant(request => request.SKU = new string('S', 51)));
        yield return Invalid("Variant rejects zero price", new CreateProductVariantRequestValidator(), Variant(request => request.Price = 0));
        yield return Invalid("Variant rejects negative price", new CreateProductVariantRequestValidator(), Variant(request => request.Price = -1));
        yield return Invalid("Variant rejects negative stock", new CreateProductVariantRequestValidator(), Variant(request => request.StockQuantity = -1));

        yield return Invalid("Login requires email", new LoginRequestValidator(), new LoginRequest { Email = string.Empty, Password = "Password1!" });
        yield return Invalid("Login rejects malformed email", new LoginRequestValidator(), new LoginRequest { Email = "not-an-email", Password = "Password1!" });
        yield return Invalid("Login requires password", new LoginRequestValidator(), new LoginRequest { Email = "buyer@example.test", Password = string.Empty });
        yield return Invalid("Login rejects short password", new LoginRequestValidator(), new LoginRequest { Email = "buyer@example.test", Password = "12345" });

        yield return Invalid("Register requires first name", new RegisterRequestValidator(), Register(request => request.FirstName = string.Empty));
        yield return Invalid("Register rejects long first name", new RegisterRequestValidator(), Register(request => request.FirstName = new string('F', 101)));
        yield return Invalid("Register requires last name", new RegisterRequestValidator(), Register(request => request.LastName = string.Empty));
        yield return Invalid("Register rejects long last name", new RegisterRequestValidator(), Register(request => request.LastName = new string('L', 101)));
        yield return Invalid("Register requires email", new RegisterRequestValidator(), Register(request => request.Email = string.Empty));
        yield return Invalid("Register rejects malformed email", new RegisterRequestValidator(), Register(request => request.Email = "buyer-at-example"));
        yield return Invalid("Register requires password", new RegisterRequestValidator(), Register(request => request.Password = string.Empty, syncConfirmPassword: false));
        yield return Invalid("Register rejects short password", new RegisterRequestValidator(), Register(request => request.Password = "Short1!"));
        yield return Invalid("Register requires uppercase password", new RegisterRequestValidator(), Register(request => request.Password = "lowercase1!"));
        yield return Invalid("Register requires lowercase password", new RegisterRequestValidator(), Register(request => request.Password = "UPPERCASE1!"));
        yield return Invalid("Register requires digit password", new RegisterRequestValidator(), Register(request => request.Password = "NoDigit!"));
        yield return Invalid("Register requires special character", new RegisterRequestValidator(), Register(request => request.Password = "NoSpecial123"));
        yield return Invalid("Register requires matching confirmation", new RegisterRequestValidator(), Register(request => request.ConfirmPassword = "Different1!", syncConfirmPassword: false));

        yield return Invalid("Forgot password requires email", new ForgotPasswordRequestValidator(), new ForgotPasswordRequest { Email = string.Empty });
        yield return Invalid("Forgot password rejects malformed email", new ForgotPasswordRequestValidator(), new ForgotPasswordRequest { Email = "bad-email" });
        yield return Invalid("Reset password requires email", new ResetPasswordRequestValidator(), ResetPassword(request => request.Email = string.Empty));
        yield return Invalid("Reset password rejects malformed email", new ResetPasswordRequestValidator(), ResetPassword(request => request.Email = "bad-email"));
        yield return Invalid("Reset password requires token", new ResetPasswordRequestValidator(), ResetPassword(request => request.Token = string.Empty));
        yield return Invalid("Reset password requires new password", new ResetPasswordRequestValidator(), ResetPassword(request => request.NewPassword = string.Empty));
        yield return Invalid("Reset password rejects short new password", new ResetPasswordRequestValidator(), ResetPassword(request => request.NewPassword = "1234567"));
        yield return Invalid("Update profile requires first name", new UpdateProfileRequestValidator(), Profile(request => request.FirstName = string.Empty));
        yield return Invalid("Update profile rejects long first name", new UpdateProfileRequestValidator(), Profile(request => request.FirstName = new string('F', 101)));
        yield return Invalid("Update profile requires last name", new UpdateProfileRequestValidator(), Profile(request => request.LastName = string.Empty));
        yield return Invalid("Update profile rejects long last name", new UpdateProfileRequestValidator(), Profile(request => request.LastName = new string('L', 101)));

        yield return Invalid("Create brand requires name", new CreateBrandRequestValidator(), new CreateBrandRequest { Name = string.Empty });
        yield return Invalid("Create brand rejects long name", new CreateBrandRequestValidator(), new CreateBrandRequest { Name = new string('B', 201) });
        yield return Invalid("Update brand requires name", new UpdateBrandRequestValidator(), new UpdateBrandRequest { Name = string.Empty });
        yield return Invalid("Update brand rejects long name", new UpdateBrandRequestValidator(), new UpdateBrandRequest { Name = new string('B', 201) });
        yield return Invalid("Create category requires name", new CreateCategoryRequestValidator(), new CreateCategoryRequest { Name = string.Empty });
        yield return Invalid("Create category rejects long name", new CreateCategoryRequestValidator(), new CreateCategoryRequest { Name = new string('C', 201) });
        yield return Invalid("Update category requires name", new UpdateCategoryRequestValidator(), new UpdateCategoryRequest { Name = string.Empty });
        yield return Invalid("Update category rejects long name", new UpdateCategoryRequestValidator(), new UpdateCategoryRequest { Name = new string('C', 201) });

        yield return Invalid("Add to cart requires product", new AddToCartRequestValidator(), new AddToCartRequest { ProductId = Guid.Empty, Quantity = 1 });
        yield return Invalid("Add to cart rejects zero quantity", new AddToCartRequestValidator(), new AddToCartRequest { ProductId = Guid.NewGuid(), Quantity = 0 });
        yield return Invalid("Add to cart rejects negative quantity", new AddToCartRequestValidator(), new AddToCartRequest { ProductId = Guid.NewGuid(), Quantity = -3 });
        yield return Invalid("Update cart item rejects zero quantity", new UpdateCartItemRequestValidator(), new UpdateCartItemRequest { Quantity = 0 });
        yield return Invalid("Update cart item rejects negative quantity", new UpdateCartItemRequestValidator(), new UpdateCartItemRequest { Quantity = -2 });

        yield return Invalid("Create discount requires code", new CreateDiscountRequestValidator(), Discount(request => request.Code = string.Empty));
        yield return Invalid("Create discount rejects long code", new CreateDiscountRequestValidator(), Discount(request => request.Code = new string('D', 51)));
        yield return Invalid("Create discount requires name", new CreateDiscountRequestValidator(), Discount(request => request.Name = string.Empty));
        yield return Invalid("Create discount rejects long name", new CreateDiscountRequestValidator(), Discount(request => request.Name = new string('N', 201)));
        yield return Invalid("Create discount rejects zero value", new CreateDiscountRequestValidator(), Discount(request => request.Value = 0));
        yield return Invalid("Create discount rejects negative value", new CreateDiscountRequestValidator(), Discount(request => request.Value = -1));
        yield return Invalid("Create discount rejects same start and end", new CreateDiscountRequestValidator(), Discount(request => request.EndDate = request.StartDate));
        yield return Invalid("Create discount rejects end before start", new CreateDiscountRequestValidator(), Discount(request => request.EndDate = request.StartDate.AddDays(-1)));

        yield return Invalid("Create voucher requires code", new CreateVoucherRequestValidator(), Voucher(request => request.Code = string.Empty));
        yield return Invalid("Create voucher rejects long code", new CreateVoucherRequestValidator(), Voucher(request => request.Code = new string('V', 51)));
        yield return Invalid("Create voucher rejects zero value", new CreateVoucherRequestValidator(), Voucher(request => request.Value = 0));
        yield return Invalid("Create voucher rejects negative value", new CreateVoucherRequestValidator(), Voucher(request => request.Value = -1));
        yield return Invalid("Create voucher rejects same start and expiry", new CreateVoucherRequestValidator(), Voucher(request => request.ExpiryDate = request.StartDate));
        yield return Invalid("Create voucher rejects expiry before start", new CreateVoucherRequestValidator(), Voucher(request => request.ExpiryDate = request.StartDate.AddDays(-1)));

        yield return Invalid("Create order requires shipping address", new CreateOrderRequestValidator(), Order(request => request.ShippingAddressId = Guid.Empty));
        yield return Invalid("Create order requires billing address", new CreateOrderRequestValidator(), Order(request => request.BillingAddressId = Guid.Empty));
        yield return Invalid("Update order status rejects invalid enum", new UpdateOrderStatusRequestValidator(), new UpdateOrderStatusRequest { NewStatus = (OrderStatus)999 });
        yield return Invalid("Create payment requires order", new CreatePaymentRequestValidator(), Payment(request => request.OrderId = Guid.Empty));
        yield return Invalid("Create payment rejects zero amount", new CreatePaymentRequestValidator(), Payment(request => request.Amount = 0));
        yield return Invalid("Create payment rejects negative amount", new CreatePaymentRequestValidator(), Payment(request => request.Amount = -10));
        yield return Invalid("Create payment rejects invalid method", new CreatePaymentRequestValidator(), Payment(request => request.PaymentMethod = (PaymentMethod)999));
        yield return Invalid("Create refund rejects zero amount", new CreateRefundRequestValidator(), new CreateRefundRequest { Amount = 0 });
        yield return Invalid("Create refund rejects negative amount", new CreateRefundRequestValidator(), new CreateRefundRequest { Amount = -5 });

        yield return Invalid("Create review requires product", new CreateReviewRequestValidator(), Review(request => request.ProductId = Guid.Empty));
        yield return Invalid("Create review rejects low rating", new CreateReviewRequestValidator(), Review(request => request.Rating = 0));
        yield return Invalid("Create review rejects high rating", new CreateReviewRequestValidator(), Review(request => request.Rating = 6));
        yield return Invalid("Update review rejects low rating", new UpdateReviewRequestValidator(), new UpdateReviewRequest { Rating = 0 });
        yield return Invalid("Update review rejects high rating", new UpdateReviewRequestValidator(), new UpdateReviewRequest { Rating = 6 });
        yield return Invalid("Adjust inventory requires product", new AdjustInventoryRequestValidator(), Inventory(request => request.ProductId = Guid.Empty));
        yield return Invalid("Adjust inventory rejects invalid transaction type", new AdjustInventoryRequestValidator(), Inventory(request => request.TransactionType = (InventoryTransactionType)999));
        yield return Invalid("Adjust inventory rejects zero quantity", new AdjustInventoryRequestValidator(), Inventory(request => request.Quantity = 0));
        yield return Invalid("Adjust inventory rejects negative quantity", new AdjustInventoryRequestValidator(), Inventory(request => request.Quantity = -1));
    }

    public static IEnumerable<object[]> ValidRequests()
    {
        foreach (var name in new[] { "A", "Everyday Backpack", new string('P', 300) })
            yield return Valid($"Create product accepts name length {name.Length}", new CreateProductRequestValidator(), Product(request => request.Name = name));

        foreach (var sku in new[] { "S", "SKU-123", new string('S', 50) })
            yield return Valid($"Create product accepts SKU length {sku.Length}", new CreateProductRequestValidator(), Product(request => request.SKU = sku));

        foreach (var price in new[] { 0.01m, 1m, 1999.95m })
            yield return Valid($"Create product accepts base price {price}", new CreateProductRequestValidator(), Product(request => request.BasePrice = price));

        foreach (var stock in new[] { 0, 1, 1000 })
            yield return Valid($"Create product accepts stock {stock}", new CreateProductRequestValidator(), Product(request => request.StockQuantity = stock));

        foreach (var name in new[] { "A", "Updated Product", new string('U', 300) })
            yield return Valid($"Update product accepts name length {name.Length}", new UpdateProductRequestValidator(), UpdateProduct(request => request.Name = name));

        foreach (var variantName in new[] { "S", "Large / Blue", new string('V', 200) })
            yield return Valid($"Variant accepts name length {variantName.Length}", new CreateProductVariantRequestValidator(), Variant(request => request.VariantName = variantName));

        foreach (var variantSku in new[] { "V", "VAR-001", new string('K', 50) })
            yield return Valid($"Variant accepts SKU length {variantSku.Length}", new CreateProductVariantRequestValidator(), Variant(request => request.SKU = variantSku));

        foreach (var email in new[] { "buyer@example.test", "first.last+shop@example.co", "customer_1@example.com" })
            yield return Valid($"Login accepts {email}", new LoginRequestValidator(), new LoginRequest { Email = email, Password = "Password1!" });

        foreach (var password in new[] { "Strong1!", "LongerPassword2@", "Reusable-Shop-3#" })
            yield return Valid("Register accepts strong password", new RegisterRequestValidator(), Register(request => request.Password = password));

        foreach (var email in new[] { "buyer@example.test", "shop.owner@example.co" })
            yield return Valid($"Forgot password accepts {email}", new ForgotPasswordRequestValidator(), new ForgotPasswordRequest { Email = email });

        foreach (var newPassword in new[] { "12345678", "new-password", "Another1!" })
            yield return Valid("Reset password accepts minimum valid password", new ResetPasswordRequestValidator(), ResetPassword(request => request.NewPassword = newPassword));

        foreach (var brandName in new[] { "A", "Northwind", new string('B', 200) })
            yield return Valid($"Create brand accepts name length {brandName.Length}", new CreateBrandRequestValidator(), new CreateBrandRequest { Name = brandName });

        foreach (var categoryName in new[] { "A", "Home Goods", new string('C', 200) })
            yield return Valid($"Create category accepts name length {categoryName.Length}", new CreateCategoryRequestValidator(), new CreateCategoryRequest { Name = categoryName });

        foreach (var quantity in new[] { 1, 2, 100, int.MaxValue })
            yield return Valid($"Add to cart accepts quantity {quantity}", new AddToCartRequestValidator(), new AddToCartRequest { ProductId = Guid.NewGuid(), Quantity = quantity });

        foreach (var quantity in new[] { 1, 25, 500 })
            yield return Valid($"Update cart item accepts quantity {quantity}", new UpdateCartItemRequestValidator(), new UpdateCartItemRequest { Quantity = quantity });

        foreach (var discountType in Enum.GetValues<DiscountType>())
            yield return Valid($"Create discount accepts {discountType}", new CreateDiscountRequestValidator(), Discount(request => request.DiscountType = discountType));

        foreach (var value in new[] { 0.01m, 10m, 75.5m })
            yield return Valid($"Create discount accepts value {value}", new CreateDiscountRequestValidator(), Discount(request => request.Value = value));

        foreach (var voucherType in Enum.GetValues<VoucherType>())
            yield return Valid($"Create voucher accepts {voucherType}", new CreateVoucherRequestValidator(), Voucher(request => request.VoucherType = voucherType));

        foreach (var valueType in Enum.GetValues<VoucherValueType>())
            yield return Valid($"Create voucher accepts {valueType}", new CreateVoucherRequestValidator(), Voucher(request => request.ValueType = valueType));

        foreach (var paymentMethod in Enum.GetValues<PaymentMethod>())
        {
            yield return Valid($"Create order accepts payment method {paymentMethod}", new CreateOrderRequestValidator(), Order(request => request.PaymentMethod = paymentMethod));
            yield return Valid($"Create payment accepts payment method {paymentMethod}", new CreatePaymentRequestValidator(), Payment(request => request.PaymentMethod = paymentMethod));
        }

        foreach (var orderStatus in Enum.GetValues<OrderStatus>())
            yield return Valid($"Update order status accepts {orderStatus}", new UpdateOrderStatusRequestValidator(), new UpdateOrderStatusRequest { NewStatus = orderStatus });

        foreach (var amount in new[] { 0.01m, 1m, 9999m })
        {
            yield return Valid($"Create payment accepts amount {amount}", new CreatePaymentRequestValidator(), Payment(request => request.Amount = amount));
            yield return Valid($"Create refund accepts amount {amount}", new CreateRefundRequestValidator(), new CreateRefundRequest { Amount = amount });
        }

        foreach (var rating in Enumerable.Range(1, 5))
        {
            yield return Valid($"Create review accepts rating {rating}", new CreateReviewRequestValidator(), Review(request => request.Rating = rating));
            yield return Valid($"Update review accepts rating {rating}", new UpdateReviewRequestValidator(), new UpdateReviewRequest { Rating = rating });
        }

        foreach (var transactionType in Enum.GetValues<InventoryTransactionType>())
            yield return Valid($"Adjust inventory accepts {transactionType}", new AdjustInventoryRequestValidator(), Inventory(request => request.TransactionType = transactionType));

        foreach (var quantity in new[] { 1, 10, 1000 })
            yield return Valid($"Adjust inventory accepts quantity {quantity}", new AdjustInventoryRequestValidator(), Inventory(request => request.Quantity = quantity));
    }

    private static bool IsValid(object validator, object request)
    {
        var result = ((dynamic)validator).Validate((dynamic)request);
        return (bool)result.IsValid;
    }

    private static object[] Invalid(string scenario, object validator, object request)
    {
        return new[] { scenario, validator, request };
    }

    private static object[] Valid(string scenario, object validator, object request)
    {
        return new[] { scenario, validator, request };
    }

    private static CreateProductRequest Product(Action<CreateProductRequest>? mutate = null)
    {
        var request = new CreateProductRequest
        {
            Name = "Reusable Product",
            SKU = "RP-001",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            BasePrice = 29.95m,
            CostPrice = 10m,
            StockQuantity = 12,
            MinStockLevel = 2
        };
        mutate?.Invoke(request);
        return request;
    }

    private static UpdateProductRequest UpdateProduct(Action<UpdateProductRequest>? mutate = null)
    {
        var request = new UpdateProductRequest
        {
            Name = "Reusable Product",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            BasePrice = 29.95m,
            CostPrice = 10m,
            StockQuantity = 12,
            MinStockLevel = 2,
            IsActive = true
        };
        mutate?.Invoke(request);
        return request;
    }

    private static CreateProductVariantRequest Variant(Action<CreateProductVariantRequest>? mutate = null)
    {
        var request = new CreateProductVariantRequest
        {
            VariantName = "Default",
            SKU = "RP-001-DEFAULT",
            Price = 29.95m,
            StockQuantity = 10
        };
        mutate?.Invoke(request);
        return request;
    }

    private static RegisterRequest Register(Action<RegisterRequest>? mutate = null, bool syncConfirmPassword = true)
    {
        var request = new RegisterRequest
        {
            FirstName = "Ada",
            LastName = "Buyer",
            Email = "ada.buyer@example.test",
            Password = "StrongPass1!",
            ConfirmPassword = "StrongPass1!"
        };
        mutate?.Invoke(request);
        if (syncConfirmPassword)
            request.ConfirmPassword = request.Password;
        return request;
    }

    private static ResetPasswordRequest ResetPassword(Action<ResetPasswordRequest>? mutate = null)
    {
        var request = new ResetPasswordRequest
        {
            Email = "buyer@example.test",
            Token = "reset-token",
            NewPassword = "NewPassword1!"
        };
        mutate?.Invoke(request);
        return request;
    }

    private static UpdateProfileRequest Profile(Action<UpdateProfileRequest>? mutate = null)
    {
        var request = new UpdateProfileRequest
        {
            FirstName = "Ada",
            LastName = "Buyer"
        };
        mutate?.Invoke(request);
        return request;
    }

    private static CreateDiscountRequest Discount(Action<CreateDiscountRequest>? mutate = null)
    {
        var request = new CreateDiscountRequest
        {
            Code = "SAVE10",
            Name = "Save 10",
            DiscountType = DiscountType.Percentage,
            Value = 10m,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(7)
        };
        mutate?.Invoke(request);
        return request;
    }

    private static CreateVoucherRequest Voucher(Action<CreateVoucherRequest>? mutate = null)
    {
        var request = new CreateVoucherRequest
        {
            Code = "WELCOME10",
            VoucherType = VoucherType.Promo,
            ValueType = VoucherValueType.Percentage,
            Value = 10m,
            StartDate = DateTime.UtcNow.Date,
            ExpiryDate = DateTime.UtcNow.Date.AddDays(7)
        };
        mutate?.Invoke(request);
        return request;
    }

    private static CreateOrderRequest Order(Action<CreateOrderRequest>? mutate = null)
    {
        var request = new CreateOrderRequest
        {
            ShippingAddressId = Guid.NewGuid(),
            BillingAddressId = Guid.NewGuid(),
            PaymentMethod = PaymentMethod.CreditCard
        };
        mutate?.Invoke(request);
        return request;
    }

    private static CreatePaymentRequest Payment(Action<CreatePaymentRequest>? mutate = null)
    {
        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            Amount = 49.95m,
            PaymentMethod = PaymentMethod.CreditCard
        };
        mutate?.Invoke(request);
        return request;
    }

    private static CreateReviewRequest Review(Action<CreateReviewRequest>? mutate = null)
    {
        var request = new CreateReviewRequest
        {
            ProductId = Guid.NewGuid(),
            Rating = 5,
            Title = "Great",
            Comment = "Works well"
        };
        mutate?.Invoke(request);
        return request;
    }

    private static AdjustInventoryRequest Inventory(Action<AdjustInventoryRequest>? mutate = null)
    {
        var request = new AdjustInventoryRequest
        {
            ProductId = Guid.NewGuid(),
            TransactionType = InventoryTransactionType.StockIn,
            Quantity = 5
        };
        mutate?.Invoke(request);
        return request;
    }
}
