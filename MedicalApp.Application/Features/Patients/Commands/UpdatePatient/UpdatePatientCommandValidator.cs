using FluentValidation;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator(IPatientRepository repository)
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
            .MustAsync(async (command, cnp, ct) =>
            {
                var existingPatient = await repository.GetByCnpAsync(cnp, ct);
                if (existingPatient is null) 
                    return true;
                return existingPatient.Id == command.Id;
            })
            .WithMessage("This CNP is already used by another patient.");
    }
}