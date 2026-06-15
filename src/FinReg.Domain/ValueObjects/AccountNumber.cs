using System.Text.RegularExpressions;

namespace FinReg.Domain.ValueObjects;

public sealed record AccountNumber
{
    private static readonly Regex Format = new(@"^ACC-[A-Z0-9]{8}$", RegexOptions.Compiled);

    public string Value { get; }

    public AccountNumber(string value)
    {
        if (!Format.IsMatch(value))
            throw new ArgumentException($"Invalid account number: '{value}'. Expected format: ACC-XXXXXXXX.", nameof(value));
        Value = value;
    }

    public static AccountNumber Generate() =>
        new($"ACC-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}");

    public static implicit operator string(AccountNumber number) => number.Value;

    public override string ToString() => Value;
}
