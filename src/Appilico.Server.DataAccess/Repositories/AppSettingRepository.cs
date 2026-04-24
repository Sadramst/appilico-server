using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>
/// Repository for AppSetting-specific operations.
/// </summary>
public class AppSettingRepository : GenericRepository<AppSetting>, IAppSettingRepository
{
    /// <summary>Initializes a new instance of the <see cref="AppSettingRepository"/> class.</summary>
    public AppSettingRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<AppSetting?> GetByKeyAsync(string key)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Key == key);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AppSetting>> GetByGroupAsync(string group)
    {
        return await _dbSet.Where(s => s.Group == group).ToListAsync();
    }
}
