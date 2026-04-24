using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>
/// Repository for AuditLog-specific operations.
/// </summary>
public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    /// <summary>Initializes a new instance of the <see cref="AuditLogRepository"/> class.</summary>
    public AuditLogRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, string entityId)
    {
        return await _dbSet
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
