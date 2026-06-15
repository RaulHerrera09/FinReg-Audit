using FinReg.Domain.Enums;

namespace FinReg.Domain.Exceptions;

public sealed class AccountSuspendedException : DomainException
{
    public AccountSuspendedException(Guid accountId, AccountStatus status)
        : base($"Account {accountId} cannot process operations in status '{status}'.") { }
}
