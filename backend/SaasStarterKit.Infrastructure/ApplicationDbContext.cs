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

        private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "Token",           // RefreshToken
            "NormalizedEmail",
            "NormalizedUserName",
            "PhoneNumber",
            "TwoFactorEnabled",
            "LockoutEnd",
            "LockoutEnabled",
            "AccessFailedCount",
            "EmailConfirmed",
            "PhoneNumberConfirmed"
        };

        private static readonly HashSet<string> IgnoredEntities =
        [
            nameof(RefreshToken),
            "IdentityUserRole`1",
            "IdentityUserClaim`1",
            "IdentityUserToken`1",
            "IdentityRoleClaim`1",
            "IdentityUserLogin`1"
        ];

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _tenantService = tenantService;
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
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
                entity.Property(u => u.CreatedDate).HasColumnName("CreatedDate");
                entity.HasOne(u => u.Tenant)
                      .WithMany()
                      .HasForeignKey(u => u.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Token).IsRequired();
                entity.Property(r => r.CreatedDate).HasColumnName("CreatedDate");
                entity.HasIndex(r => r.Token).IsUnique();
                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.EventName).IsRequired().HasMaxLength(100);
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
