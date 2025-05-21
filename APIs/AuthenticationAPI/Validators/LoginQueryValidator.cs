using FluentValidation;
using AuthenticationAPI.Queries;

namespace AuthenticationAPI.Validators
{
    internal class LoginQueryValidator : AbstractValidator<LoginQuery>
    {
        public LoginQueryValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .EmailAddress().WithMessage("Enter a valid email address.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("The {PropertyName} field is required.");
        }
    }
}