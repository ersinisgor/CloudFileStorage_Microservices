using FluentValidation;
using AuthenticationAPI.Commands;

namespace AuthenticationAPI.Validators
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("The {PropertyName} field is required.");
        }
    }
}