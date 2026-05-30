using System.Net;
using System.Net.Http.Json;
using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Constants;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AppilicoShopServer.IntegrationTests;

public class AuthorizationHardeningTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthorizationHardeningTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CustomerEndpoints_BlockNonOwner_AllowOwnerAndAdmin()
    {
        var scenario = await CreateScenarioAsync();

        var ownerResponse = await GetAsync($"/api/customers/{scenario.CustomerOneId}", scenario.CustomerOneUserId, AppConstants.Roles.Customer);
        var nonOwnerResponse = await GetAsync($"/api/customers/{scenario.CustomerOneId}", scenario.CustomerTwoUserId, AppConstants.Roles.Customer);
        var adminResponse = await GetAsync($"/api/customers/{scenario.CustomerOneId}", scenario.AdminUserId, AppConstants.Roles.Admin);

        ownerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        nonOwnerResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OrderEndpoints_BlockNonOwner_AllowOwnerAndAdmin()
    {
        var scenario = await CreateScenarioAsync();

        var ownerResponse = await GetAsync($"/api/orders/{scenario.CustomerOneOrderId}", scenario.CustomerOneUserId, AppConstants.Roles.Customer);
        var nonOwnerResponse = await GetAsync($"/api/orders/{scenario.CustomerOneOrderId}", scenario.CustomerTwoUserId, AppConstants.Roles.Customer);
        var adminResponse = await GetAsync($"/api/orders/{scenario.CustomerOneOrderId}", scenario.AdminUserId, AppConstants.Roles.Admin);

        ownerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        nonOwnerResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PaymentEndpoints_BlockNonOwner_AllowOwnerAndAdmin()
    {
        var scenario = await CreateScenarioAsync();

        var ownerResponse = await GetAsync($"/api/payments/{scenario.CustomerOnePaymentId}", scenario.CustomerOneUserId, AppConstants.Roles.Customer);
        var nonOwnerResponse = await GetAsync($"/api/payments/{scenario.CustomerOnePaymentId}", scenario.CustomerTwoUserId, AppConstants.Roles.Customer);
        var adminResponse = await GetAsync($"/api/payments/{scenario.CustomerOnePaymentId}", scenario.AdminUserId, AppConstants.Roles.Admin);

        ownerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        nonOwnerResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReviewMutationEndpoints_BlockNonOwner_AllowOwnerAndAdmin()
    {
        var scenario = await CreateScenarioAsync();
        var request = new { rating = 4, title = "Updated", comment = "Updated comment" };

        var nonOwnerResponse = await PutAsync($"/api/reviews/{scenario.CustomerOneReviewId}", request, scenario.CustomerTwoUserId, AppConstants.Roles.Customer);
        var ownerResponse = await PutAsync($"/api/reviews/{scenario.CustomerOneReviewId}", request, scenario.CustomerOneUserId, AppConstants.Roles.Customer);
        var adminResponse = await PutAsync($"/api/reviews/{scenario.CustomerTwoReviewId}", request, scenario.AdminUserId, AppConstants.Roles.Admin);

        nonOwnerResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        ownerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<HttpResponseMessage> GetAsync(string url, string userId, string role)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add(TestAuthHandler.UserIdHeader, userId);
        request.Headers.Add(TestAuthHandler.RoleHeader, role);
        return await _client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string url, object body, string userId, string role)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add(TestAuthHandler.UserIdHeader, userId);
        request.Headers.Add(TestAuthHandler.RoleHeader, role);
        return await _client.SendAsync(request);
    }

    private async Task<AuthScenario> CreateScenarioAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        var admin = await EnsureUserAsync(userManager, roleManager, "admin@appilico.com", "Admin@123!", "Admin", "User", AppConstants.Roles.Admin);
        var customerOne = await EnsureCustomerAsync(db, userManager, roleManager, "customer1@appilico.com", "Customer@123!", "John", "Doe");
        var customerTwo = await EnsureCustomerAsync(db, userManager, roleManager, "customer2@appilico.com", "Customer@123!", "Jane", "Smith");

        var product = await db.Products.FirstOrDefaultAsync() ?? new Product
        {
            Name = "Authorization Test Product",
            SKU = $"AUTH-{Guid.NewGuid():N}"[..16],
            BasePrice = 50m,
            CostPrice = 25m,
            StockQuantity = 10,
            CreatedBy = "test"
        };
        if (product.Id == Guid.Empty)
            db.Products.Add(product);
        await db.SaveChangesAsync();

        var addressOne = CreateAddress(customerOne.Id);
        var addressTwo = CreateAddress(customerTwo.Id);
        db.CustomerAddresses.AddRange(addressOne, addressTwo);
        await db.SaveChangesAsync();

        var orderOne = CreateOrder(customerOne.Id, addressOne.Id, 100m);
        var orderTwo = CreateOrder(customerTwo.Id, addressTwo.Id, 125m);
        db.Orders.AddRange(orderOne, orderTwo);
        await db.SaveChangesAsync();

        db.OrderItems.AddRange(
            CreateOrderItem(orderOne.Id, product.Id, 100m),
            CreateOrderItem(orderTwo.Id, product.Id, 125m));
        await db.SaveChangesAsync();

        var paymentOne = CreatePayment(orderOne.Id, orderOne.TotalAmount);
        var paymentTwo = CreatePayment(orderTwo.Id, orderTwo.TotalAmount);
        var reviewOne = CreateReview(product.Id, customerOne.Id);
        var reviewTwo = CreateReview(product.Id, customerTwo.Id);

        db.Payments.AddRange(paymentOne, paymentTwo);
        db.ProductReviews.AddRange(reviewOne, reviewTwo);
        await db.SaveChangesAsync();

        var accessControl = scope.ServiceProvider.GetRequiredService<IAccessControlService>();
        (await accessControl.CanAccessCustomerAsync(customerOne.UserId, false, customerOne.Id)).Should().BeTrue();
        (await accessControl.CanAccessOrderAsync(customerOne.UserId, false, orderOne.Id)).Should().BeTrue();
        (await accessControl.CanAccessPaymentAsync(customerOne.UserId, false, paymentOne.Id)).Should().BeTrue();
        (await accessControl.CanAccessReviewAsync(customerOne.UserId, false, reviewOne.Id)).Should().BeTrue();

        return new AuthScenario(
            admin.Id,
            customerOne.UserId,
            customerTwo.UserId,
            customerOne.Id,
            customerTwo.Id,
            orderOne.Id,
            orderTwo.Id,
            paymentOne.Id,
            paymentTwo.Id,
            reviewOne.Id,
            reviewTwo.Id);
    }

    private static async Task<Customer> EnsureCustomerAsync(
        AppDbContext db,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        string email,
        string password,
        string firstName,
        string lastName)
    {
        var user = await EnsureUserAsync(userManager, roleManager, email, password, firstName, lastName, AppConstants.Roles.Customer);
        var customer = await db.Customers.FirstOrDefaultAsync(value => value.UserId == user.Id);
        if (customer != null)
            return customer;

        customer = new Customer
        {
            UserId = user.Id,
            CustomerCode = $"AUTH-{Guid.NewGuid():N}"[..16],
            JoinDate = DateTime.UtcNow,
            MembershipTier = MembershipTier.Bronze,
            CreatedBy = "test"
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer;
    }

    private static async Task<AppUser> EnsureUserAsync(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new AppRole { Name = role });

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(user, password);
            result.Succeeded.Should().BeTrue(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
        else if (!await userManager.CheckPasswordAsync(user, password))
        {
            if (await userManager.HasPasswordAsync(user))
            {
                var removeResult = await userManager.RemovePasswordAsync(user);
                removeResult.Succeeded.Should().BeTrue(string.Join("; ", removeResult.Errors.Select(error => error.Description)));
            }

            var addPasswordResult = await userManager.AddPasswordAsync(user, password);
            addPasswordResult.Succeeded.Should().BeTrue(string.Join("; ", addPasswordResult.Errors.Select(error => error.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);

        return user;
    }

    private static CustomerAddress CreateAddress(Guid customerId) => new()
    {
        CustomerId = customerId,
        AddressType = AddressType.Shipping,
        Title = "Test Address",
        AddressLine1 = "1 Test Street",
        City = "Perth",
        State = "WA",
        PostalCode = "6000",
        Country = "AU",
        IsDefault = true,
        CreatedBy = "test"
    };

    private static Order CreateOrder(Guid customerId, Guid addressId, decimal total) => new()
    {
        CustomerId = customerId,
        OrderNumber = $"AUTH-{Guid.NewGuid():N}"[..18],
        OrderDate = DateTime.UtcNow,
        OrderStatus = OrderStatus.Confirmed,
        PaymentStatus = PaymentStatus.Paid,
        PaymentMethod = PaymentMethod.CreditCard,
        SubTotal = total,
        TotalAmount = total,
        ShippingAddressId = addressId,
        BillingAddressId = addressId,
        CreatedBy = "test"
    };

    private static OrderItem CreateOrderItem(Guid orderId, Guid productId, decimal total) => new()
    {
        OrderId = orderId,
        ProductId = productId,
        ProductName = "Authorization Test Product",
        UnitPrice = total,
        Quantity = 1,
        TotalPrice = total,
        CreatedBy = "test"
    };

    private static Payment CreatePayment(Guid orderId, decimal amount) => new()
    {
        OrderId = orderId,
        Amount = amount,
        PaymentMethod = PaymentMethod.CreditCard,
        Status = PaymentStatus.Paid,
        PaidAt = DateTime.UtcNow,
        TransactionId = $"TXN-{Guid.NewGuid():N}"[..20],
        CreatedBy = "test"
    };

    private static ProductReview CreateReview(Guid productId, Guid customerId) => new()
    {
        ProductId = productId,
        CustomerId = customerId,
        Rating = 5,
        Title = "Auth test review",
        Comment = "Created by authorization hardening tests",
        IsApproved = false,
        CreatedBy = "test"
    };

    private sealed record AuthScenario(
        string AdminUserId,
        string CustomerOneUserId,
        string CustomerTwoUserId,
        Guid CustomerOneId,
        Guid CustomerTwoId,
        Guid CustomerOneOrderId,
        Guid CustomerTwoOrderId,
        Guid CustomerOnePaymentId,
        Guid CustomerTwoPaymentId,
        Guid CustomerOneReviewId,
        Guid CustomerTwoReviewId);
}