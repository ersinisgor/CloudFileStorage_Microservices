using FluentValidation;
using AuthenticationAPI.Commands;

namespace AuthenticationAPI.Validators
{
    internal class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("The {PropertyName} field is required.");
        }
    }
}