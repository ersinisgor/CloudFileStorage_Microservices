using FluentValidation;
using FileMetadataAPI.Commands;
using FileMetadataAPI.Models;

namespace FileMetadataAPI.Validators
{
    internal class UpdateFileCommandValidator : AbstractValidator<UpdateFileCommand>
    {
        public UpdateFileCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("The {PropertyName} must be a positive value.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("File name is required.")
                .MaximumLength(255).WithMessage("The file name cannot be longer than {MaxLength} characters.");
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("The {PropertyName} cannot be longer than {MaxLength} characters.");
            RuleFor(x => x.Visibility)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .Must(v => Enum.TryParse<Visibility>(v, out _))
                .WithMessage("Invalid visibility value (Private, Public, Shared).");
            When(x => x.Visibility == "Private" || x.Visibility == "Public", () =>
            {
                RuleFor(x => x.FileShares).Empty().WithMessage("FileShares must be empty when visibility is Private or Public.");
            });
            When(x => x.Visibility == "Shared", () =>
            {
                RuleFor(x => x.FileShares).NotEmpty().WithMessage("FileShares must not be empty when visibility is Shared.");
            });
            RuleForEach(x => x.FileShares)
                .SetValidator(new FileShareDTOValidator());
        }
    }
}