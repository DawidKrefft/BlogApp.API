using BlogApp.API.Models.DTO;
using FluentValidation;

namespace BlogApp.API.Validations
{
    public class RegisterValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterValidator()
        {
            RuleFor(request => request.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MinimumLength(6)
                .WithMessage("Email must be at least 6 characters long")
                .Must(email => email.Contains("@") && email.Contains("."))
                .WithMessage("Email must contain '@' and '.'");

            RuleFor(request => request.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
                .Matches("[0-9]")
                .WithMessage("Password must contain at least one number");
        }
    }
}
