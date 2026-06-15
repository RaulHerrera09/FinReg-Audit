using FluentValidation;

namespace FinReg.Application.Transactions.Commands.FlagTransaction;

public sealed class FlagTransactionCommandValidator : AbstractValidator<FlagTransactionCommand>
{
    public FlagTransactionCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
