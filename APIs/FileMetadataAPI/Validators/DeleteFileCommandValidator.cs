using FluentValidation;
using FileMetadataAPI.Commands;

namespace FileMetadataAPI.Validators
{
    internal class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
    {
        public DeleteFileCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("The {PropertyName} must be a positive value.");
        }
    }
}