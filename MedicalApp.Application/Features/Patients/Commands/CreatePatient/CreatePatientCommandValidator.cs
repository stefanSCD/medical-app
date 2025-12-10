using System.Security.Cryptography;
using FluentValidation;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator(IPatientRepository repository)
    {
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