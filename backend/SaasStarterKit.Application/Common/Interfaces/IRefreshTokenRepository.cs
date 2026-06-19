using SaasStarterKit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Application.Common.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
        Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
        Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
