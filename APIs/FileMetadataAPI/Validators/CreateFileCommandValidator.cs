using FluentValidation;
using FileMetadataAPI.Commands;
using FileMetadataAPI.Models;

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
                .Must(v => Enum.TryParse<Visibility>(v?.Trim(), ignoreCase: true, out _))
                .WithMessage("Invalid visibility value (Private, Public, Shared).");
            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required.")
                .Must(file => file.Length > 0).WithMessage("File cannot be empty.");
                //.Must(file => new[] { ".pdf", ".jpg" }.Contains(Path.GetExtension(file.FileName).ToLower()))
                //.WithMessage("Only .pdf and .jpg files are allowed.");
            RuleForEach(x => x.FileShares)
                .SetValidator(new FileShareDTOValidator());
        }
    }
}