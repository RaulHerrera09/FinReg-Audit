using FinReg.Domain.ValueObjects;
using FluentAssertions;

namespace FinReg.Domain.Tests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_ValidArgs_SetsPropertiesAndNormalisesCurrency()
    {
        var money = new Money(100.505m, "gbp");

        money.Amount.Should().Be(100.51m);
        money.Currency.Should().Be("GBP");
    }

    [Fact]
    public void Create_NegativeAmount_Throws()
    {
        var act = () => new Money(-0.01m, "GBP");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("GB")]
    [InlineData("GBPX")]
    [InlineData("   ")]
    public void Create_InvalidCurrencyCode_Throws(string currency)
    {
        var act = () => new Money(100m, currency);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSummed()
    {
        var result = new Money(100m, "GBP").Add(new Money(50m, "GBP"));
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("GBP");
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        var result = new Money(100m, "GBP").Subtract(new Money(30m, "GBP"));
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Subtract_MoreThanAvailable_Throws()
    {
        var act = () => new Money(10m, "GBP").Subtract(new Money(20m, "GBP"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Add_DifferentCurrency_Throws()
    {
        var act = () => new Money(100m, "GBP").Add(new Money(100m, "EUR"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_DifferentCurrency_Throws()
    {
        var act = () => new Money(100m, "GBP").Subtract(new Money(50m, "EUR"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        var a = new Money(100m, "GBP");
        var b = new Money(100m, "GBP");
        a.Should().Be(b);
    }

    [Fact]
    public void ToString_ReturnsFormattedValue()
    {
        new Money(1234.5m, "GBP").ToString().Should().Be("1234.50 GBP");
    }
}
