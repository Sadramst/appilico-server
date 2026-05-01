using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Visual;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Visuals service interface.</summary>
public interface IVisualService
{
    Task<ApiResponse<List<VisualDto>>> GetAllAsync();
}
