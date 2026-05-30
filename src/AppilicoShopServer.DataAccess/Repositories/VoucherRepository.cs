using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>
/// Repository for Voucher-specific operations.
/// </summary>
public class VoucherRepository : GenericRepository<Voucher>, IVoucherRepository
{
    /// <summary>Initializes a new instance of the <see cref="VoucherRepository"/> class.</summary>
    public VoucherRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<Voucher?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(v => v.Code == code);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Voucher>> GetActiveVouchersAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(v => v.IsActive && v.StartDate <= now && v.ExpiryDate >= now)
            .ToListAsync();
    }
}
