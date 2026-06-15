namespace FinReg.Domain.Exceptions;

public sealed class InvalidTransactionException : DomainException
{
    public InvalidTransactionException(Guid transactionId)
        : base($"Transaction {transactionId} was not found.") { }

    public InvalidTransactionException(Guid transactionId, string reason)
        : base($"Transaction {transactionId} is invalid: {reason}") { }
}
