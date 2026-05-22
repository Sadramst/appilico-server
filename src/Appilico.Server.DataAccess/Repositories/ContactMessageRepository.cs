using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>Contact message repository.</summary>
public class ContactMessageRepository : GenericRepository<ContactMessage>, IContactMessageRepository
{
    /// <summary>Initializes the repository.</summary>
    public ContactMessageRepository(AppDbContext context) : base(context) { }
}