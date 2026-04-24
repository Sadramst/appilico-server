using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Domain.Interfaces;

/// <summary>
/// Repository interface for AuditLog-specific operations.
/// </summary>
public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    /// <summary>Gets audit logs by entity name and ID.</summary>
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, string entityId);
}
