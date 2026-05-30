using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>
/// Repository interface for Voucher-specific operations.
/// </summary>
public interface IVoucherRepository : IGenericRepository<Voucher>
{
    /// <summary>Gets a voucher by code.</summary>
    Task<Voucher?> GetByCodeAsync(string code);

    /// <summary>Gets active vouchers.</summary>
    Task<IReadOnlyList<Voucher>> GetActiveVouchersAsync();
}
