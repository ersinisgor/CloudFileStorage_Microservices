using FluentValidation;
using AuthenticationAPI.DTOs;

namespace AuthenticationAPI.Validators
{
    internal class UserDTOValidator : AbstractValidator<UserDTO>
    {
        public UserDTOValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("The user ID must be a positive value.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .MaximumLength(50).WithMessage("The {PropertyName} cannot be longer than {MaxLength} characters.");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .EmailAddress().WithMessage("Enter a valid email address.");
        }
    }
}