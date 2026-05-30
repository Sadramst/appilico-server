namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>Centralized resource ownership checks for API authorization gates.</summary>
public interface IAccessControlService
{
    /// <summary>Checks whether a user can access a customer resource.</summary>
    Task<bool> CanAccessCustomerAsync(string userId, bool isPrivilegedUser, Guid customerId);

    /// <summary>Checks whether a user can access an order resource.</summary>
    Task<bool> CanAccessOrderAsync(string userId, bool isPrivilegedUser, Guid orderId);

    /// <summary>Checks whether a user can access resources attached to an order.</summary>
    Task<bool> CanAccessOrderPaymentsAsync(string userId, bool isPrivilegedUser, Guid orderId);

    /// <summary>Checks whether a user can access a payment resource.</summary>
    Task<bool> CanAccessPaymentAsync(string userId, bool isPrivilegedUser, Guid paymentId);

    /// <summary>Checks whether a user can access a review resource.</summary>
    Task<bool> CanAccessReviewAsync(string userId, bool isPrivilegedUser, Guid reviewId);
}