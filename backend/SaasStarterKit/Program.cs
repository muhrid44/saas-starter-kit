using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaasStarterKit.API.Middleware;
using SaasStarterKit.API.Utils;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Common.Services;
using SaasStarterKit.Application.Common.Settings;
using SaasStarterKit.Application.Users.Commands.CreateUser;
using SaasStarterKit.Domain.Entities;
using SaasStarterKit.Infrastructure;
using SaasStarterKit.Infrastructure.Repositories;

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
                    typeof(CreateUserHandler).Assembly));

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("NpgSqlConnection")));

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

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

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMiddleware<TenantMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
