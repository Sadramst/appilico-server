using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>Contact message repository.</summary>
public class ContactMessageRepository : GenericRepository<ContactMessage>, IContactMessageRepository
{
    /// <summary>Initializes the repository.</summary>
    public ContactMessageRepository(AppDbContext context) : base(context) { }
}