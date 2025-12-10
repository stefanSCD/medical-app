using FluentValidation;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MedicalApp.Application.Features.Authentication.Commands.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator(IPatientRepository repository, UserManager<ApplicationUser> userManager)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .MustAsync(async (email, ct) =>
            {
                var existingUser = await userManager.FindByEmailAsync(email);
                return existingUser == null;
            })
            .WithMessage("A user with this email address already exists.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(4).WithMessage("Password must be at least 4 characters long");

        RuleFor(c => c.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("LastName is required")
            .MaximumLength(50).WithMessage("LastName cannot exceed 50 characters");

        RuleFor(c => c.PersonalNumericCode)
            .NotEmpty().WithMessage("Personal numeric code is required")
            .Length(13).WithMessage("Personal numeric code should have 13 digits")
            .Matches("^[0-9]*$").WithMessage("Personal numeric code must contain only digits")
            .MustAsync(async (cnp, ct) =>
            {
                var existingPatient = await repository.GetByCnpAsync(cnp, ct);
                return existingPatient is null;
            })
            .WithMessage("A patient with the specified personal numeric code already exists");
    }
}