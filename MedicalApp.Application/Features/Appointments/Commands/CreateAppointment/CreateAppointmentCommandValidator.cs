using FluentValidation;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator(IAppointmentRepository repository)
    {
        RuleFor(c => c.DoctorId)
            .NotEmpty().WithMessage("DoctorId is required");
        RuleFor(c => c.StartDate)
            .NotEmpty().WithMessage("StartDate is required")
            .GreaterThan(DateTime.Now).WithMessage("Appointment cannot be in the past");
        RuleFor(c => c.EndDate)
            .NotEmpty().WithMessage("EndDate is required")
            .GreaterThan(c => c.StartDate).WithMessage("EndDate must be after StartDate");
    }
}