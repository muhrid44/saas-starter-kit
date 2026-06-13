using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Common;
using SaasStarterKit.Domain.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace SaasStarterKit.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        private readonly ITenantService _tenantService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _tenantService = tenantService;
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditLogs = GenerateAuditLogs();
            var result = await base.SaveChangesAsync(cancellationToken);

            if (auditLogs.Any())
            {
                await AuditLogs.AddRangeAsync(auditLogs, cancellationToken);
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        private List<AuditLog> GenerateAuditLogs()
        {
            var auditLogs = new List<AuditLog>();
            var changedBy = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value;
            var tenantId = _tenantService.GetCurrentTenantId();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog) continue;
                if (entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged) continue;

                var action = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted => "Deleted",
                    _ => "Unknown"
                };

                var oldValues = entry.State == EntityState.Modified
                    ? JsonSerializer.Serialize(entry.OriginalValues.Properties
                        .ToDictionary(p => p.Name, p => entry.OriginalValues[p]?.ToString()))
                    : null;

                var newValues = entry.State != EntityState.Deleted
                    ? JsonSerializer.Serialize(entry.CurrentValues.Properties
                        .ToDictionary(p => p.Name, p => entry.CurrentValues[p]?.ToString()))
                    : null;

                auditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = entry.Entity.GetType().Name,
                    Action = action,
                    OldValues = oldValues,
                    NewValues = newValues,
                    ChangedBy = changedBy,
                    ChangedAt = DateTime.UtcNow,
                    TenantId = tenantId == Guid.Empty ? null : tenantId
                });
            }

            return auditLogs;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // This is required for setup the Identity framework
            base.OnModelCreating(modelBuilder);

            // Auto-apply tenant filter to all ITenantEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType)
                    && entityType.ClrType != typeof(ApplicationUser))
                {
                    var method = typeof(ApplicationDbContext)
                        .GetMethod(nameof(ApplyTenantFilter),
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(this, new object[] { modelBuilder });
                }
            }

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                entity.Property(t => t.Slug).IsRequired().HasMaxLength(100);
                entity.HasIndex(t => t.Slug).IsUnique();
            });


            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.HasOne(u => u.Tenant)
                      .WithMany()
                      .HasForeignKey(u => u.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Token).IsRequired();
                entity.HasIndex(r => r.Token).IsUnique();
                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Action).IsRequired().HasMaxLength(50);
                entity.Property(a => a.ChangedBy).HasMaxLength(255);
            });
        }

        private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
       where TEntity : class, ITenantEntity
        {
            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(e => e.TenantId == _tenantService.GetCurrentTenantId());
        }
    }
}
