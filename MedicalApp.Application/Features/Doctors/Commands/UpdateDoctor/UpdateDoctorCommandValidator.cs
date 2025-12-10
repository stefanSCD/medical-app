using FluentValidation;

namespace MedicalApp.Application.Features.Doctors.Commands.UpdateDoctor;

public class UpdateDoctorCommandValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorCommandValidator()
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
    }
}