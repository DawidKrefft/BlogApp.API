using BlogApp.API.Models.DTO;
using FluentValidation;

namespace BlogApp.API.Validations
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryRequestDto>
    {
        public UpdateCategoryValidator()
        {
            RuleFor(category => category.Name)
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("Name is required and must be less than 50 characters.")
                .Matches("^[a-zA-Z0-9-]+$")
                .WithMessage("Name can only contain letters, numbers, and hyphens.");

            RuleFor(category => category.UrlHandle)
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("URL handle is required and must be less than 50 characters.")
                .Matches("^[a-z0-9-]+$")
                .WithMessage(
                    "URL handle can only contain lowercase letters, numbers, and hyphens."
                );
        }
    }
}
