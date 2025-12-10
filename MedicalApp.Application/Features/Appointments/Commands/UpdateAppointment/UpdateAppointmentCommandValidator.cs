using FluentValidation;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentCommandValidator : AbstractValidator<UpdateAppointmentCommand>
{
    public UpdateAppointmentCommandValidator(IAppointmentRepository repository)
    {
        RuleFor(c => c.DoctorId)
            .NotEmpty().WithMessage("DoctorId is required");
        RuleFor(c => c.PatientId)
            .NotEmpty().WithMessage("PatientId is required");
        RuleFor(c => c.StartDate)
            .NotEmpty().WithMessage("StartDate is required")
            .GreaterThan(DateTime.Now).WithMessage("Appointment cannot be in the past");
        RuleFor(c => c.EndDate)
            .NotEmpty().WithMessage("EndDate is required")
            .GreaterThan(c => c.StartDate).WithMessage("EndDate must be after StartDate");
        RuleFor(c => c)
            .MustAsync(async (command, ct) =>
            {
                var isOverlapping = await repository.IsOverlappingAsync(
                    command.DoctorId, command.StartDate, command.EndDate, command.Id, ct);
                return !isOverlapping;
            }).WithMessage("The doctor is already booked for this time slot.");
    }
}