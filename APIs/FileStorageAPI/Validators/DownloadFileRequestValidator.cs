using FluentValidation;
using FileStorageAPI.DTOs;

namespace FileStorageAPI.Validators
{
    internal class DownloadFileRequestValidator : AbstractValidator<DownloadFileRequest>
    {
        public DownloadFileRequestValidator()
        {
            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage("File path is required.")
                .Must(path => !string.IsNullOrWhiteSpace(path)).WithMessage("File path cannot be whitespace.");
        }
    }
}