using BlogApp.API.Models.DTO;
using FluentValidation;

namespace BlogApp.API.Validations
{
    public class CreateBlogPostValidator : AbstractValidator<CreateBlogPostRequestDto>
    {
        public CreateBlogPostValidator()
        {
            RuleFor(dto => dto.Title).NotEmpty().MaximumLength(30);

            RuleFor(dto => dto.ShortDescription).NotEmpty().MaximumLength(500);

            RuleFor(dto => dto.Content).NotEmpty();

            RuleFor(dto => dto.FeaturedImageUrl).MaximumLength(255);

            RuleFor(dto => dto.UrlHandle)
                .NotEmpty()
                .MaximumLength(50)
                .Matches("^[a-zA-Z0-9-]+$")
                .WithMessage("URL handle can only contain letters, numbers, and hyphens.");

            RuleFor(dto => dto.PublishedDate).NotEmpty().LessThanOrEqualTo(DateTime.Now);

            RuleFor(dto => dto.Author).NotEmpty().MaximumLength(50);

            RuleFor(dto => dto.Categories)
                .Must(categories => categories.All(category => category != Guid.Empty))
                .WithMessage("All categories must be valid GUIDs.");
        }
    }
}
