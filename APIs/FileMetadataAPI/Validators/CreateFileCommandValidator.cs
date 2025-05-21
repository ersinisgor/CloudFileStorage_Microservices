using FluentValidation;
using FileMetadataAPI.Commands;

namespace FileMetadataAPI.Validators
{
    internal class CreateFileCommandValidator : AbstractValidator<CreateFileCommand>
    {
        public CreateFileCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("File name is required.")
                .MaximumLength(255).WithMessage("The file name cannot be longer than {MaxLength} characters.");
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("The {PropertyName} cannot be longer than {MaxLength} characters.");
            RuleFor(x => x.Visibility)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .Must(v => Enum.TryParse<Models.Visibility>(v, out _))
                .WithMessage("Invalid visibility value (Private, Public, Shared).");
            RuleForEach(x => x.FileShares)
                .SetValidator(new FileShareDTOValidator());
        }
    }
}