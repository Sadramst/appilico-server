using Appilico.Server.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Data;

/// <summary>
/// Application database context with Identity support.
/// </summary>
public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    /// <summary>Initializes a new instance of the <see cref="AppDbContext"/> class.</summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Gets or sets the Customers table.</summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>Gets or sets the CustomerAddresses table.</summary>
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();

    /// <summary>Gets or sets the RefreshTokens table.</summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>Gets or sets the Categories table.</summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>Gets or sets the Products table.</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>Gets or sets the ProductImages table.</summary>
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    /// <summary>Gets or sets the ProductVariants table.</summary>
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    /// <summary>Gets or sets the Brands table.</summary>
    public DbSet<Brand> Brands => Set<Brand>();

    /// <summary>Gets or sets the PriceHistories table.</summary>
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();

    /// <summary>Gets or sets the Discounts table.</summary>
    public DbSet<Discount> Discounts => Set<Discount>();

    /// <summary>Gets or sets the SpecialOffers table.</summary>
    public DbSet<SpecialOffer> SpecialOffers => Set<SpecialOffer>();

    /// <summary>Gets or sets the SpecialOfferProducts table.</summary>
    public DbSet<SpecialOfferProduct> SpecialOfferProducts => Set<SpecialOfferProduct>();

    /// <summary>Gets or sets the Vouchers table.</summary>
    public DbSet<Voucher> Vouchers => Set<Voucher>();

    /// <summary>Gets or sets the VoucherRedemptions table.</summary>
    public DbSet<VoucherRedemption> VoucherRedemptions => Set<VoucherRedemption>();

    /// <summary>Gets or sets the Carts table.</summary>
    public DbSet<Cart> Carts => Set<Cart>();

    /// <summary>Gets or sets the CartItems table.</summary>
    public DbSet<CartItem> CartItems => Set<CartItem>();

    /// <summary>Gets or sets the Orders table.</summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>Gets or sets the OrderItems table.</summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>Gets or sets the OrderStatusHistories table.</summary>
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    /// <summary>Gets or sets the Sales table.</summary>
    public DbSet<Sale> Sales => Set<Sale>();

    /// <summary>Gets or sets the Payments table.</summary>
    public DbSet<Payment> Payments => Set<Payment>();

    /// <summary>Gets or sets the Refunds table.</summary>
    public DbSet<Refund> Refunds => Set<Refund>();

    /// <summary>Gets or sets the ProductReviews table.</summary>
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

    /// <summary>Gets or sets the Wishlists table.</summary>
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();

    /// <summary>Gets or sets the InventoryTransactions table.</summary>
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    /// <summary>Gets or sets the AppSettings table.</summary>
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    /// <summary>Gets or sets the AuditLogs table.</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <summary>Configures the model using Fluent API.</summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>Overrides SaveChanges to apply audit fields and soft delete query filter.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                    if (entry.Entity.Id == Guid.Empty)
                        entry.Entity.Id = Guid.NewGuid();
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
