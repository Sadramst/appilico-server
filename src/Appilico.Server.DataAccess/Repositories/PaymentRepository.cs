using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>
/// Repository for Payment-specific operations.
/// </summary>
public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    /// <summary>Initializes a new instance of the <see cref="PaymentRepository"/> class.</summary>
    public PaymentRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Payment>> GetByOrderIdAsync(Guid orderId)
    {
        return await _dbSet
            .Include(p => p.Refunds)
            .Where(p => p.OrderId == orderId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Payment?> GetWithRefundsAsync(Guid id)
    {
        return await _dbSet
            .Include(payment => payment.Refunds)
            .FirstOrDefaultAsync(payment => payment.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
    {
        return await _dbSet
            .Include(payment => payment.Refunds)
            .FirstOrDefaultAsync(payment => payment.TransactionId == transactionId);
    }
}
