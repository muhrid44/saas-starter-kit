using Microsoft.EntityFrameworkCore.Storage;

namespace SaasStarterKit.Application.Common.Interfaces
{
    public interface IDbTransactionService
    {
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    }
}
