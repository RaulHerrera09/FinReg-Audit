using FinReg.Domain.ValueObjects;
using FluentAssertions;

namespace FinReg.Domain.Tests.ValueObjects;

public sealed class AccountNumberTests
{
    [Fact]
    public void Generate_ProducesValidFormat()
    {
        var number = AccountNumber.Generate();
        number.Value.Should().MatchRegex(@"^ACC-[A-Z0-9]{8}$");
    }

    [Fact]
    public void Generate_ProducesUniqueValues()
    {
        var numbers = Enumerable.Range(0, 20).Select(_ => AccountNumber.Generate().Value).ToList();
        numbers.Distinct().Should().HaveCount(20);
    }

    [Fact]
    public void Create_ValidFormat_Succeeds()
    {
        var act = () => new AccountNumber("ACC-AB12CD34");
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("acc-ab12cd34")]
    [InlineData("ACC-AB12CD3")]
    [InlineData("ACC-AB12CD345")]
    [InlineData("XCC-AB12CD34")]
    [InlineData("")]
    public void Create_InvalidFormat_Throws(string value)
    {
        var act = () => new AccountNumber(value);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ImplicitConversion_ToStringReturnsValue()
    {
        var number = new AccountNumber("ACC-AB12CD34");
        string asString = number;
        asString.Should().Be("ACC-AB12CD34");
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = new AccountNumber("ACC-AB12CD34");
        var b = new AccountNumber("ACC-AB12CD34");
        a.Should().Be(b);
    }
}
