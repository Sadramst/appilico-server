using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

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
