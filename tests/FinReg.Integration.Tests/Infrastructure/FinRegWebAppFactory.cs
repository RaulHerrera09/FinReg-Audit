using FinReg.Application.Common.Interfaces;
using FinReg.Infrastructure.Persistence;
using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace FinReg.Integration.Tests.Infrastructure;

public sealed class FinRegWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.EnsureCreatedAsync();
        await SeedTestUserAsync(db, hasher);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    private static async Task SeedTestUserAsync(AppDbContext db, IPasswordHasher hasher)
    {
        if (await db.Users.AnyAsync(u => u.Email == TestCredentials.Email))
            return;

        db.Users.Add(new UserRecord
        {
            Id = Guid.NewGuid(),
            Email = TestCredentials.Email,
            PasswordHash = hasher.Hash(TestCredentials.Password),
            Role = "ComplianceOfficer",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

public static class TestCredentials
{
    public const string Email = "test@finreg.test";
    public const string Password = "Test@12345";
}
