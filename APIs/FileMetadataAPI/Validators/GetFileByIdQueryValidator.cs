using FluentValidation;
using FileMetadataAPI.Queries;

namespace FileMetadataAPI.Validators
{
    internal class GetFileByIdQueryValidator : AbstractValidator<GetFileByIdQuery>
    {
        public GetFileByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("The file ID must be a positive value.");
        }
    }
}