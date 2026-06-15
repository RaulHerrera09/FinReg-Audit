namespace FinReg.Domain.Exceptions;

public sealed class AccountNotFoundException : DomainException
{
    public AccountNotFoundException(Guid accountId)
        : base($"Account {accountId} was not found.") { }
}
