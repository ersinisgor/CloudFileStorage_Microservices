using FluentValidation;
using FileMetadataAPI.Commands;

namespace FileMetadataAPI.Validators
{
    internal class ShareFileCommandValidator : AbstractValidator<ShareFileCommand>
    {
        public ShareFileCommandValidator()
        {
            RuleFor(x => x.FileId)
                .GreaterThan(0).WithMessage("The {PropertyName} must be a positive value.");
            RuleFor(x => x.Visibility)
                .NotEmpty().WithMessage("The {PropertyName} field is required.")
                .Must(v => Enum.TryParse<Models.Visibility>(v, out _))
                .WithMessage("Invalid visibility value (Private, Public, Shared).");
            RuleForEach(x => x.FileShares)
                .SetValidator(new FileShareDTOValidator());
        }
    }
}
