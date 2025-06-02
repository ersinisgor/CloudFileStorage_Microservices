using FluentValidation;
using AuthenticationAPI.DTOs;

namespace AuthenticationAPI.Validators
{
    internal class AuthResultValidator : AbstractValidator<AuthResult>
    {
        public AuthResultValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("The JWT {PropertyName} field is required.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("The {PropertyName} field is required.");

            RuleFor(x => x.User)
                .NotNull().WithMessage("The {PropertyName} field is required.")
                .SetValidator(new UserInfoValidator());
        }
    }

    internal class UserInfoValidator : AbstractValidator<UserInfo>
    {
        public UserInfoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("The {PropertyName} field is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .MaximumLength(50).WithMessage("The {PropertyName} cannot be longer than {MaxLength} characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .EmailAddress().WithMessage("The {PropertyName} must be a valid email address.");

            //RuleFor(x => x.Role)
            //    .NotEmpty().WithMessage("The {PropertyName} field is required.")
            //    .Must(role => role is "User" or "Admin").WithMessage("The {PropertyName} must be either 'User' or 'Admin'.");
        }
    }
}