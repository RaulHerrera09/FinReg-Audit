using FinReg.Application.Common.Interfaces;
using FinReg.Application.Transactions.Commands.InitiateTransaction;
using FinReg.Domain.Entities;
using FinReg.Domain.Enums;
using FinReg.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace FinReg.Application.Tests.Transactions.Commands;

public sealed class InitiateTransactionCommandHandlerTests
{
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();
    private readonly InitiateTransactionCommandHandler _handler;

    public InitiateTransactionCommandHandlerTests() =>
        _handler = new InitiateTransactionCommandHandler(_repository);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsTransactionId()
    {
        var account = Account.Open("Test User", "GBP");
        _repository.GetByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);

        var command = new InitiateTransactionCommand(account.Id, 500m, "GBP", TransactionType.Credit, "Salary");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);
        account.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_AccountNotFound_Throws()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Account?)null);

        var act = async () => await _handler.Handle(
            new InitiateTransactionCommand(id, 100m, "GBP", TransactionType.Credit, "Test"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AccountNotFoundException>();
    }

    [Fact]
    public async Task Handle_SuspendedAccount_ThrowsAccountSuspendedException()
    {
        var account = Account.Open("Test User", "GBP");
        account.Suspend("Fraud");
        _repository.GetByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);

        var act = async () => await _handler.Handle(
            new InitiateTransactionCommand(account.Id, 100m, "GBP", TransactionType.Credit, "Test"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AccountSuspendedException>();
    }
}
