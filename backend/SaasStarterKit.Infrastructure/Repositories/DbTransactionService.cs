using Microsoft.EntityFrameworkCore.Storage;
using SaasStarterKit.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SaasStarterKit.Infrastructure.Repositories
{
    public class DbTransactionService : IDbTransactionService
    {
        private readonly ApplicationDbContext _context;

        public DbTransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }
    }
}
