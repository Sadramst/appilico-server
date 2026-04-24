using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Settings;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Settings service interface.</summary>
public interface ISettingsService
{
    /// <summary>Gets all settings.</summary>
    Task<ApiResponse<List<AppSettingDto>>> GetAllAsync();

    /// <summary>Gets settings by group.</summary>
    Task<ApiResponse<List<AppSettingDto>>> GetByGroupAsync(string group);

    /// <summary>Gets a setting by key.</summary>
    Task<ApiResponse<AppSettingDto>> GetByKeyAsync(string key);

    /// <summary>Updates settings.</summary>
    Task<ApiResponse<bool>> UpdateAsync(UpdateSettingsRequest request, string userId);
}
