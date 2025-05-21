using FluentValidation;
using AuthenticationAPI.Commands;

namespace AuthenticationAPI.Validators
{
    internal class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .MaximumLength(50).WithMessage("The {PropertyName} cannot be longer than {MaxLength} characters.");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .EmailAddress().WithMessage("Enter a valid email address.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .MinimumLength(8).WithMessage("The {PropertyName} must be at least {MinLength} characters.")
                .Matches(@"[A-Z]").WithMessage("The password must contain at least one capital letter.")
                .Matches(@"[0-9]").WithMessage("The password must contain at least one number.")
                .Matches(@"[!@#$%^&*]").WithMessage("The password must contain at least one special character (!@#$%^&*).");
        }
    }
}