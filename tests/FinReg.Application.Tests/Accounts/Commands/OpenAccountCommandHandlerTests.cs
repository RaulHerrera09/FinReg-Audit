using FinReg.Application.Accounts.Commands.OpenAccount;
using FinReg.Application.Common.Interfaces;
using FinReg.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace FinReg.Application.Tests.Accounts.Commands;

public sealed class OpenAccountCommandHandlerTests
{
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();
    private readonly OpenAccountCommandHandler _handler;

    public OpenAccountCommandHandlerTests() =>
        _handler = new OpenAccountCommandHandler(_repository);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithAccountId()
    {
        var command = new OpenAccountCommand("Jane Smith", "GBP");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveAsync()
    {
        var command = new OpenAccountCommand("Jane Smith", "GBP");

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).SaveAsync(
            Arg.Is<Account>(a => a.HolderName == "Jane Smith"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_AccountIsActive()
    {
        Account? savedAccount = null;
        await _repository.SaveAsync(Arg.Do<Account>(a => savedAccount = a), Arg.Any<CancellationToken>());

        var command = new OpenAccountCommand("Jane Smith", "GBP");
        await _handler.Handle(command, CancellationToken.None);

        savedAccount.Should().NotBeNull();
        savedAccount!.HolderName.Should().Be("Jane Smith");
        savedAccount.Balance.Currency.Should().Be("GBP");
        savedAccount.DomainEvents.Should().HaveCount(1);
    }
}
