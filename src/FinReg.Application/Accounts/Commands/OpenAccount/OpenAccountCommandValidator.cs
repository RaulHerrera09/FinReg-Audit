using FluentValidation;

namespace FinReg.Application.Accounts.Commands.OpenAccount;

public sealed class OpenAccountCommandValidator : AbstractValidator<OpenAccountCommand>
{
    public OpenAccountCommandValidator()
    {
        RuleFor(x => x.HolderName)
            .NotEmpty().WithMessage("Holder name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be uppercase letters only.");
    }
}
