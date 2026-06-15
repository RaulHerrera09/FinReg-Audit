using FinReg.Application.Accounts.Commands.SuspendAccount;
using FinReg.Application.Common.Interfaces;
using FinReg.Domain.Entities;
using FinReg.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace FinReg.Application.Tests.Accounts.Commands;

public sealed class SuspendAccountCommandHandlerTests
{
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();
    private readonly SuspendAccountCommandHandler _handler;

    public SuspendAccountCommandHandlerTests() =>
        _handler = new SuspendAccountCommandHandler(_repository);

    [Fact]
    public async Task Handle_ExistingAccount_SuspendsAndSaves()
    {
        var account = Account.Open("Test User", "GBP");
        _repository.GetByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);

        var command = new SuspendAccountCommand(account.Id, "Fraud detected");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        account.Status.Should().Be(FinReg.Domain.Enums.AccountStatus.Suspended);
        await _repository.Received(1).SaveAsync(account, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AccountNotFound_ThrowsAccountNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Account?)null);

        var act = async () => await _handler.Handle(new SuspendAccountCommand(id, "Reason"), CancellationToken.None);

        await act.Should().ThrowAsync<AccountNotFoundException>();
    }
}
