using FluentValidation;
using MedicalApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MedicalApp.Application.Features.Doctors.Commands.CreateDoctor;

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator(UserManager<ApplicationUser> userManager)
    {
        RuleFor(c => c.FirstName)
            .NotEmpty().WithMessage("FirstName is required")
            .MaximumLength(50).WithMessage("FirstName cannot exceed 50 characters");

        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("LastName is required")
            .MaximumLength(50).WithMessage("LastName cannot exceed 50 characters");

        RuleFor(c => c.Specialization)
            .NotEmpty().WithMessage("Specialization is required")
            .MinimumLength(3).WithMessage("Specialization must have at least 3 characters");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .MustAsync(async (email, ct) =>
            {
                var existingUser = await userManager.FindByEmailAsync(email);
                return existingUser == null;
            })
            .WithMessage("A user with this email address already exists.");


        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(4).WithMessage("Password must be at least 4 characters long");
    }
}