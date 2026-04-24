using AutoMapper;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Settings;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>Settings service implementation.</summary>
public class SettingsService : ISettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SettingsService> _logger;

    /// <summary>Initializes a new instance of SettingsService.</summary>
    public SettingsService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SettingsService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<AppSettingDto>>> GetAllAsync()
    {
        var settings = await _unitOfWork.Settings.GetAllAsync();
        return ApiResponse<List<AppSettingDto>>.SuccessResponse(_mapper.Map<List<AppSettingDto>>(settings));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<AppSettingDto>>> GetByGroupAsync(string group)
    {
        var settings = await _unitOfWork.Settings.GetByGroupAsync(group);
        return ApiResponse<List<AppSettingDto>>.SuccessResponse(_mapper.Map<List<AppSettingDto>>(settings));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<AppSettingDto>> GetByKeyAsync(string key)
    {
        var setting = await _unitOfWork.Settings.GetByKeyAsync(key);
        if (setting == null)
            return ApiResponse<AppSettingDto>.FailResponse("Setting not found");

        return ApiResponse<AppSettingDto>.SuccessResponse(_mapper.Map<AppSettingDto>(setting));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> UpdateAsync(UpdateSettingsRequest request, string userId)
    {
        foreach (var item in request.Settings)
        {
            var setting = await _unitOfWork.Settings.GetByKeyAsync(item.Key);
            if (setting == null) continue;

            setting.Value = item.Value;
            setting.UpdatedBy = userId;
            _unitOfWork.Settings.Update(setting);
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Settings updated by {UserId}", userId);

        return ApiResponse<bool>.SuccessResponse(true, "Settings updated successfully");
    }
}
