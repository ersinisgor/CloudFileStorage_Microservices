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
                .IsInEnum().WithMessage("Invalid permission value.");
        }
    }
}