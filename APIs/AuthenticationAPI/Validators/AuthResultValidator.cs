using FluentValidation;
using AuthenticationAPI.DTOs;

namespace AuthenticationAPI.Validators
{
    public class AuthResultValidator : AbstractValidator<AuthResult>
    {
        public AuthResultValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("The JWT {PropertyName} field is required.");
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("The {PropertyName} field is required.");
        }
    }
}