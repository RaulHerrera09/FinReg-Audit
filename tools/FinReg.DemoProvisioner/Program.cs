using System.Text;
using FinReg.Infrastructure.Identity;
using FinReg.Infrastructure.Persistence;
using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

// One-shot, operator-run provisioning of the public demo Auditor account.
// Email and role are fixed. Secrets come from the environment, hidden input or stdin, and are never printed.
const string DemoEmail = "auditor@demo.finreg.dev";
const string DemoRole = "Auditor";
const int MinPasswordLength = 12;

var interactive = !Console.IsInputRedirected;

Console.WriteLine($"Demo account provisioning: {DemoEmail} (role {DemoRole}).");

// Prefer the DATABASE_URL already configured where the API runs; otherwise ask for it without echo.
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")?.Trim();
if (string.IsNullOrEmpty(connectionString) && interactive)
    connectionString = ReadSecret("DATABASE_URL (hidden): ").Trim();
if (string.IsNullOrEmpty(connectionString))
{
    Console.Error.WriteLine("DATABASE_URL is required. No changes made.");
    return 2;
}

// Interactive runs prompt twice without echo; non-interactive runs read a single line from stdin.
string password;
if (interactive)
{
    password = ReadSecret($"New password (hidden, min {MinPasswordLength} chars): ");
    if (ReadSecret("Confirm password (hidden): ") != password)
    {
        Console.Error.WriteLine("Passwords do not match. No changes made.");
        return 2;
    }
}
else
{
    // Read stdin as UTF-8 and drop any byte order mark: some shells prepend one when piping to a native process.
    using var stdin = new StreamReader(Console.OpenStandardInput(), new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);
    password = (stdin.ReadLine() ?? string.Empty).TrimStart((char)0xFEFF).TrimEnd('\r', '\n');
}

if (password.Length < MinPasswordLength)
{
    Console.Error.WriteLine($"Password must be at least {MinPasswordLength} characters. No changes made.");
    return 2;
}

try
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name))
        .Options;

    await using var db = new AppDbContext(options);

    var pending = (await db.Database.GetPendingMigrationsAsync()).Count();
    if (pending > 0)
    {
        Console.Error.WriteLine($"Schema has {pending} pending migration(s). No changes made.");
        return 3;
    }

    var hasher = new BCryptPasswordHasher();
    await using var tx = await db.Database.BeginTransactionAsync();

    var user = await db.Users.SingleOrDefaultAsync(u => u.Email == DemoEmail);
    string outcome;
    var revoked = 0;

    if (user is null)
    {
        db.Users.Add(new UserRecord
        {
            Id = Guid.NewGuid(),
            Email = DemoEmail,
            PasswordHash = hasher.Hash(password),
            Role = DemoRole,
            CreatedAt = DateTime.UtcNow
        });
        outcome = "created";
    }
    else if (user.Role != DemoRole)
    {
        Console.Error.WriteLine($"{DemoEmail} exists with a different role. No changes made.");
        return 4;
    }
    else if (hasher.Verify(password, user.PasswordHash))
    {
        outcome = "unchanged (password already set)";
    }
    else
    {
        user.PasswordHash = hasher.Hash(password);
        revoked = await db.RefreshTokens
            .Where(r => r.UserId == user.Id && !r.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true));
        outcome = "password rotated";
    }

    await db.SaveChangesAsync();
    await tx.CommitAsync();

    Console.WriteLine($"Result: {outcome}. Email: {DemoEmail}. Role: {DemoRole}. Refresh tokens revoked: {revoked}.");
    return 0;
}
catch (ArgumentException)
{
    // Parser messages can echo connection string fragments, so they are never printed.
    Console.Error.WriteLine("DATABASE_URL could not be parsed. Use the Npgsql keyword format (Host=...;Port=...). No changes made.");
    return 1;
}
catch (Exception ex) when (ex is PostgresException || ex.InnerException is PostgresException)
{
    var pg = ex as PostgresException ?? (PostgresException)ex.InnerException!;
    Console.Error.WriteLine($"PostgreSQL error {pg.SqlState}: {pg.MessageText}. No changes committed.");
    return 1;
}
catch (NpgsqlException ex)
{
    Console.Error.WriteLine($"Connection error: {ex.Message}. No changes committed.");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Unexpected {ex.GetType().Name}. No changes committed.");
    return 1;
}

static string ReadSecret(string prompt)
{
    Console.Write(prompt);
    var buffer = new StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter) break;
        if (key.Key == ConsoleKey.Backspace)
        {
            if (buffer.Length > 0) buffer.Length--;
            continue;
        }
        if (!char.IsControl(key.KeyChar)) buffer.Append(key.KeyChar);
    }
    Console.WriteLine();
    return buffer.ToString();
}
