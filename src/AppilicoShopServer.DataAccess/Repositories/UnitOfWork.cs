using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>
/// Unit of Work implementation managing repository lifecycle and transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private IBrandRepository? _brands;
    private ICustomerRepository? _customers;
    private IOrderRepository? _orders;
    private ICartRepository? _carts;
    private IDiscountRepository? _discounts;
    private IVoucherRepository? _vouchers;
    private ISpecialOfferRepository? _specialOffers;
    private IPaymentRepository? _payments;
    private IReviewRepository? _reviews;
    private IWishlistRepository? _wishlists;
    private IInventoryRepository? _inventory;
    private IAppSettingRepository? _settings;
    private IAuditLogRepository? _auditLogs;
    private IRefreshTokenRepository? _refreshTokens;
    private IBlogPostRepository? _blogPosts;
    private IVisualRepository? _visuals;
    private ISubscriptionRepository? _subscriptions;
    private INewsletterSubscriberRepository? _newsletterSubscribers;
    private IWaitlistRepository? _waitlistEntries;
    private IContactMessageRepository? _contactMessages;
    private IExternalWebhookEventRepository? _externalWebhookEvents;

    /// <summary>Initializes a new instance of the <see cref="UnitOfWork"/> class.</summary>
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public IProductRepository Products => _products ??= new ProductRepository(_context);

    /// <inheritdoc/>
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

    /// <inheritdoc/>
    public IBrandRepository Brands => _brands ??= new BrandRepository(_context);

    /// <inheritdoc/>
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);

    /// <inheritdoc/>
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);

    /// <inheritdoc/>
    public ICartRepository Carts => _carts ??= new CartRepository(_context);

    /// <inheritdoc/>
    public IDiscountRepository Discounts => _discounts ??= new DiscountRepository(_context);

    /// <inheritdoc/>
    public IVoucherRepository Vouchers => _vouchers ??= new VoucherRepository(_context);

    /// <inheritdoc/>
    public ISpecialOfferRepository SpecialOffers => _specialOffers ??= new SpecialOfferRepository(_context);

    /// <inheritdoc/>
    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);

    /// <inheritdoc/>
    public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);

    /// <inheritdoc/>
    public IWishlistRepository Wishlists => _wishlists ??= new WishlistRepository(_context);

    /// <inheritdoc/>
    public IInventoryRepository Inventory => _inventory ??= new InventoryRepository(_context);

    /// <inheritdoc/>
    public IAppSettingRepository Settings => _settings ??= new AppSettingRepository(_context);

    /// <inheritdoc/>
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

    /// <inheritdoc/>
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);

    /// <inheritdoc/>
    public IBlogPostRepository BlogPosts => _blogPosts ??= new BlogPostRepository(_context);

    /// <inheritdoc/>
    public IVisualRepository Visuals => _visuals ??= new VisualRepository(_context);

    /// <inheritdoc/>
    public ISubscriptionRepository Subscriptions => _subscriptions ??= new SubscriptionRepository(_context);

    /// <inheritdoc/>
    public INewsletterSubscriberRepository NewsletterSubscribers => _newsletterSubscribers ??= new NewsletterSubscriberRepository(_context);

    /// <inheritdoc/>
    public IWaitlistRepository WaitlistEntries => _waitlistEntries ??= new WaitlistRepository(_context);

    /// <inheritdoc/>
    public IContactMessageRepository ContactMessages => _contactMessages ??= new ContactMessageRepository(_context);

    /// <inheritdoc/>
    public IExternalWebhookEventRepository ExternalWebhookEvents => _externalWebhookEvents ??= new ExternalWebhookEventRepository(_context);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>Disposes the context and transaction.</summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
