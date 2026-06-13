using Asp.Versioning;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaasStarterKit.API.Middleware;
using SaasStarterKit.API.Utils;
using SaasStarterKit.Application.Common.Settings;
using SaasStarterKit.Application.Users.Commands.CreateUser;
using SaasStarterKit.Domain.Entities;
using SaasStarterKit.Infrastructure;

namespace SaasStarterKit
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ServicesRegistration.Register(builder);

            builder.Services.AddHttpContextAccessor();

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).
            AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Add services to the container.
            // Add MediatR
            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(
                    typeof(RegisterUserCommandHandler).Assembly));

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("NpgSqlConnection")));

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Add Hangfire
            builder.Services.AddHangfire(config =>
                config.UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(
                        builder.Configuration.GetConnectionString("NpgSqlConnection"))));

            builder.Services.AddHangfireServer();

            // Add Redis caching
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
                options.InstanceName = "SaasStarterKit:";
            });

            builder.Services.Configure<ApiSettings>(
                builder.Configuration.GetSection("ApiSettings"));

            var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>();
            var versionParts = apiSettings.DefaultVersion.Split('.');
            var majorVersion = int.Parse(versionParts[0]);
            var minorVersion = versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0;

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(majorVersion, minorVersion);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            }).AddMvc();

            var app = builder.Build();

            //Automatically apply pending migrations on application startup
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();

                //Seed test user
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // bypass tenant filter for seeding
                var testUser = await db.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Email == "test@test.com"); var defaultTenantId = new Guid("00000000-0000-0000-0000-000000000001");

                if (testUser == null)
                {
                    var newUser = new ApplicationUser
                    {
                        UserName = "test@test.com",
                        Email = "test@test.com",
                        FullName = "Test User",
                        IsActive = true,
                        CreateAt = DateTime.UtcNow,
                        TenantId = defaultTenantId
                    };

                    var result = await userManager.CreateAsync(newUser, "Test@123456!");

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                            Console.WriteLine($"Seed error: {error.Code} - {error.Description}");
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<TenantMiddleware>();

            // Add Hangfire dashboard
            app.UseHangfireDashboard("/hangfire");

            app.MapControllers();

            app.Run();
        }
    }
}
