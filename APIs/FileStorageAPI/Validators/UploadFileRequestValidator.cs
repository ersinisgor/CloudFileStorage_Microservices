using FluentValidation;
using FileStorageAPI.DTOs;

namespace FileStorageAPI.Validators
{
    public class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
    {
        public UploadFileRequestValidator()
        {
            Int32 maxFileSize = 10 * 1024 * 1024;
            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required.")
                .Must(file => file.Length > 0).WithMessage("File cannot be empty.")
                .Must(file => file.Length <= maxFileSize).WithMessage("File size cannot exceed 10MB.");
        }
    }
}