using FinReg.Application.Common.Interfaces;
using FinReg.Infrastructure.Identity;
using FinReg.Infrastructure.Persistence;
using FinReg.Infrastructure.Persistence.EventStore;
using FinReg.Infrastructure.Persistence.Repositories;
using FinReg.Infrastructure.Services;
using FinReg.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinReg.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(opts =>
        {
            var section = configuration.GetSection("Jwt");
            opts.Secret = section["Secret"] ?? string.Empty;
            opts.Issuer = section["Issuer"] ?? string.Empty;
            opts.Audience = section["Audience"] ?? string.Empty;
            if (int.TryParse(section["AccessTokenExpiryMinutes"], out var m)) opts.AccessTokenExpiryMinutes = m;
            if (int.TryParse(section["RefreshTokenExpiryDays"], out var d)) opts.RefreshTokenExpiryDays = d;
        });

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));

        services.AddScoped<IEventStore, EventStoreRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();

        return services;
    }
}
