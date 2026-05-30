using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>
/// Repository interface for AppSetting-specific operations.
/// </summary>
public interface IAppSettingRepository : IGenericRepository<AppSetting>
{
    /// <summary>Gets a setting by key.</summary>
    Task<AppSetting?> GetByKeyAsync(string key);

    /// <summary>Gets settings by group.</summary>
    Task<IReadOnlyList<AppSetting>> GetByGroupAsync(string group);
}
