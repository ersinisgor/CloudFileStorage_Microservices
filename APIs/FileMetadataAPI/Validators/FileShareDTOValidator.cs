using FluentValidation;
using FileMetadataAPI.DTOs;

namespace FileMetadataAPI.Validators
{

    internal class FileShareDTOValidator : AbstractValidator<FileShareDTO>
    {
        public FileShareDTOValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("The {PropertyName} must be a positive value.");
            RuleFor(x => x.Permission)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .Must(p => p == "Read" || p == "Edit")
                .WithMessage("The permission can only be 'Read' or 'Edit'.");
        }
    }
}