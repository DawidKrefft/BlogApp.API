using BlogApp.API.Models.DTO;
using FluentValidation;

namespace BlogApp.API.Validations
{
    public class ImageUploadValidator : AbstractValidator<ImageUploadRequestDto>
    {
        public ImageUploadValidator(IConfiguration configuration)
        {
            RuleFor(dto => dto.File).NotNull().WithMessage("File is required.");
            RuleFor(dto => dto.FileName).NotEmpty().WithMessage("File name is required.");
            RuleFor(dto => dto.Title).NotEmpty().WithMessage("Title is required");

            RuleFor(dto => dto.File)
                .Custom(
                    (file, context) =>
                    {
                        if (file != null)
                        {
                            var allowedExtensions = configuration
                                .GetSection("AppSettings:AllowedExtensions")
                                .Get<string[]>();

                            if (
                                !allowedExtensions.Contains(
                                    Path.GetExtension(file.FileName).ToLower()
                                )
                            )
                            {
                                context.AddFailure("Unsupported file format.");
                            }

                            if (file.Length > 10485760)
                            {
                                context.AddFailure("File size cannot be more than 10MB.");
                            }
                        }
                    }
                );
        }
    }
}
