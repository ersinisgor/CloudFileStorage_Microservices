using FluentValidation;
using FileStorageAPI.DTOs;

namespace FileStorageAPI.Validators
{
    internal class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
    {
        private readonly IConfiguration _configuration;

        public UploadFileRequestValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public UploadFileRequestValidator()
        {

            var maxFileSize = _configuration.GetValue<int>("FileStorage:maxFileSize") * 1024 * 1024;
            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required.")
                .Must(file => file.Length > 0).WithMessage("File cannot be empty.")
                .Must(file => file.Length <= maxFileSize).WithMessage($"File size cannot exceed {maxFileSize / (1024 * 1024)}MB.");
            //RuleFor(x => x.File.FileName)
            //    .Must(fileName => _configuration.GetSection("FileStorage:AllowedExtensions").Get<List<string>>()
            //        .Contains(Path.GetExtension(fileName).ToLower()))
            //    .WithMessage("Only .pdf and .jpg files are allowed.");
        }
    }
}