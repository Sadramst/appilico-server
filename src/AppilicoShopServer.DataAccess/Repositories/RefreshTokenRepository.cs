using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>Refresh token repository.</summary>
public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    /// <summary>Initializes the repository.</summary>
    public RefreshTokenRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(refreshToken => refreshToken.Token == token);
    }
}