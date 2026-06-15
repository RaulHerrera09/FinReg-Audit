using FinReg.Domain.ValueObjects;

namespace FinReg.Domain.Exceptions;

public sealed class InsufficientFundsException : DomainException
{
    public InsufficientFundsException(Guid accountId, Money required, Money available)
        : base($"Insufficient funds on account {accountId}: required {required}, available {available}.") { }
}
