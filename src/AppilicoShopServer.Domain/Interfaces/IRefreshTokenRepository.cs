using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>Repository for refresh token persistence.</summary>
public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    /// <summary>Gets a refresh token by its token value.</summary>
    Task<RefreshToken?> GetByTokenAsync(string token);
}