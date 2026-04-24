using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Domain.Interfaces;

/// <summary>
/// Repository interface for Customer-specific operations.
/// </summary>
public interface ICustomerRepository : IGenericRepository<Customer>
{
    /// <summary>Gets a customer by user ID.</summary>
    Task<Customer?> GetByUserIdAsync(string userId);

    /// <summary>Gets a customer with addresses.</summary>
    Task<Customer?> GetWithAddressesAsync(Guid id);

    /// <summary>Gets a customer with orders.</summary>
    Task<Customer?> GetWithOrdersAsync(Guid id);

    /// <summary>Searches customers by name, email, or customer code.</summary>
    Task<(IReadOnlyList<Customer> Items, int TotalCount)> SearchAsync(
        string? searchTerm, int page, int pageSize);
}
