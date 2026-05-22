using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>EF-backed owner-or-staff resource access checks.</summary>
public class AccessControlService : IAccessControlService
{
    private readonly AppDbContext _db;

    /// <summary>Initializes the access control service.</summary>
    public AccessControlService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<bool> CanAccessCustomerAsync(string userId, bool isPrivilegedUser, Guid customerId)
    {
        return isPrivilegedUser || await _db.Customers.AnyAsync(customer => customer.Id == customerId && customer.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task<bool> CanAccessOrderAsync(string userId, bool isPrivilegedUser, Guid orderId)
    {
        return isPrivilegedUser || await _db.Orders
            .Join(_db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new { order.Id, customer.UserId })
            .AnyAsync(order => order.Id == orderId && order.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task<bool> CanAccessOrderPaymentsAsync(string userId, bool isPrivilegedUser, Guid orderId)
    {
        return await CanAccessOrderAsync(userId, isPrivilegedUser, orderId);
    }

    /// <inheritdoc/>
    public async Task<bool> CanAccessPaymentAsync(string userId, bool isPrivilegedUser, Guid paymentId)
    {
        return isPrivilegedUser || await _db.Payments
            .Join(_db.Orders,
                payment => payment.OrderId,
                order => order.Id,
                (payment, order) => new { payment.Id, order.CustomerId })
            .Join(_db.Customers,
                paymentOrder => paymentOrder.CustomerId,
                customer => customer.Id,
                (paymentOrder, customer) => new { paymentOrder.Id, customer.UserId })
            .AnyAsync(payment => payment.Id == paymentId && payment.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task<bool> CanAccessReviewAsync(string userId, bool isPrivilegedUser, Guid reviewId)
    {
        return isPrivilegedUser || await _db.ProductReviews
            .Join(_db.Customers,
                review => review.CustomerId,
                customer => customer.Id,
                (review, customer) => new { review.Id, customer.UserId })
            .AnyAsync(review => review.Id == reviewId && review.UserId == userId);
    }
}