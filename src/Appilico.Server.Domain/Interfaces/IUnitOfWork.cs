namespace Appilico.Server.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Gets the product repository.</summary>
    IProductRepository Products { get; }

    /// <summary>Gets the category repository.</summary>
    ICategoryRepository Categories { get; }

    /// <summary>Gets the brand repository.</summary>
    IBrandRepository Brands { get; }

    /// <summary>Gets the customer repository.</summary>
    ICustomerRepository Customers { get; }

    /// <summary>Gets the order repository.</summary>
    IOrderRepository Orders { get; }

    /// <summary>Gets the cart repository.</summary>
    ICartRepository Carts { get; }

    /// <summary>Gets the discount repository.</summary>
    IDiscountRepository Discounts { get; }

    /// <summary>Gets the voucher repository.</summary>
    IVoucherRepository Vouchers { get; }

    /// <summary>Gets the special offer repository.</summary>
    ISpecialOfferRepository SpecialOffers { get; }

    /// <summary>Gets the payment repository.</summary>
    IPaymentRepository Payments { get; }

    /// <summary>Gets the review repository.</summary>
    IReviewRepository Reviews { get; }

    /// <summary>Gets the wishlist repository.</summary>
    IWishlistRepository Wishlists { get; }

    /// <summary>Gets the inventory repository.</summary>
    IInventoryRepository Inventory { get; }

    /// <summary>Gets the settings repository.</summary>
    IAppSettingRepository Settings { get; }

    /// <summary>Gets the audit log repository.</summary>
    IAuditLogRepository AuditLogs { get; }

    /// <summary>Gets the refresh token repository.</summary>
    IRefreshTokenRepository RefreshTokens { get; }

    /// <summary>Gets the blog post repository.</summary>
    IBlogPostRepository BlogPosts { get; }

    /// <summary>Gets the visual repository.</summary>
    IVisualRepository Visuals { get; }

    /// <summary>Gets the subscription repository.</summary>
    ISubscriptionRepository Subscriptions { get; }

    /// <summary>Gets the newsletter subscriber repository.</summary>
    INewsletterSubscriberRepository NewsletterSubscribers { get; }

    /// <summary>Gets the waitlist repository.</summary>
    IWaitlistRepository WaitlistEntries { get; }

    /// <summary>Gets the contact message repository.</summary>
    IContactMessageRepository ContactMessages { get; }

    /// <summary>Gets the external webhook event repository.</summary>
    IExternalWebhookEventRepository ExternalWebhookEvents { get; }

    /// <summary>Saves all pending changes to the database.</summary>
    Task<int> SaveChangesAsync();

    /// <summary>Begins a new database transaction.</summary>
    Task BeginTransactionAsync();

    /// <summary>Commits the current transaction.</summary>
    Task CommitTransactionAsync();

    /// <summary>Rolls back the current transaction.</summary>
    Task RollbackTransactionAsync();
}
