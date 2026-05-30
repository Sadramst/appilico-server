using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.DataAccess.Repositories;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.UnitTests.Services;

public class AccessControlServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AccessControlService _sut;

    public AccessControlServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _sut = new AccessControlService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CanAccessCustomerAsync_AllowsOwnerAndPrivilegedUser_BlocksOtherUser()
    {
        var customer = new Customer { UserId = "owner", CustomerCode = "CUST-1" };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        (await _sut.CanAccessCustomerAsync("owner", false, customer.Id)).Should().BeTrue();
        (await _sut.CanAccessCustomerAsync("other", false, customer.Id)).Should().BeFalse();
        (await _sut.CanAccessCustomerAsync("other", true, customer.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessOrderAndPaymentAsync_UsesOwningCustomerUserId()
    {
        var customer = new Customer { UserId = "owner", CustomerCode = "CUST-1" };
        var order = new Order { Customer = customer, OrderNumber = "ORD-1", TotalAmount = 100m };
        var payment = new Payment { Order = order, Amount = 100m, Status = PaymentStatus.Paid };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        (await _sut.CanAccessOrderAsync("owner", false, order.Id)).Should().BeTrue();
        (await _sut.CanAccessOrderAsync("other", false, order.Id)).Should().BeFalse();
        (await _sut.CanAccessPaymentAsync("owner", false, payment.Id)).Should().BeTrue();
        (await _sut.CanAccessPaymentAsync("other", false, payment.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessReviewAsync_UsesReviewCustomerOwner()
    {
        var customer = new Customer { UserId = "owner", CustomerCode = "CUST-1" };
        var review = new ProductReview { Customer = customer, ProductId = Guid.NewGuid(), Rating = 5 };
        _db.ProductReviews.Add(review);
        await _db.SaveChangesAsync();

        (await _sut.CanAccessReviewAsync("owner", false, review.Id)).Should().BeTrue();
        (await _sut.CanAccessReviewAsync("other", false, review.Id)).Should().BeFalse();
        (await _sut.CanAccessReviewAsync("other", true, review.Id)).Should().BeTrue();
    }
}