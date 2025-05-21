using FluentValidation;
using FileMetadataAPI.DTOs;

namespace FileMetadataAPI.Validators
{
    internal class FileDTOValidator : AbstractValidator<FileDTO>
    {
        public FileDTOValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("The {PropertyName} must be a positive value.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("File name is required.")
                .MaximumLength(255).WithMessage("The file name cannot be longer than {MaxLength} characters.");
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("The {PropertyName} cannot be longer than {MaxLength} characters.");
            RuleFor(x => x.OwnerId)
                .GreaterThan(0).WithMessage("The {PropertyName} must be a positive value.");
            RuleFor(x => x.UploadDate)
                .NotEmpty().WithMessage("The {PropertyName} field is required.");
            RuleFor(x => x.Visibility)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .Must(v => Enum.TryParse<Models.Visibility>(v, out _))
                .WithMessage("Invalid visibility value (Private, Public, Shared).");
            RuleForEach(x => x.FileShares)
                .SetValidator(new FileShareDTOValidator());
        }
    }

}