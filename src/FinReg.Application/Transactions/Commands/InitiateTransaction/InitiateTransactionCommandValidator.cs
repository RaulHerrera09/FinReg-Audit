using FluentValidation;

namespace FinReg.Application.Transactions.Commands.InitiateTransaction;

public sealed class InitiateTransactionCommandValidator : AbstractValidator<InitiateTransactionCommand>
{
    public InitiateTransactionCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches("^[A-Z]{3}$");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}
