using FluentValidation;

namespace FinReg.Application.Accounts.Commands.SuspendAccount;

public sealed class SuspendAccountCommandValidator : AbstractValidator<SuspendAccountCommand>
{
    public SuspendAccountCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
