using FluentValidation;

namespace MedicalApp.Application.Features.Appointments.Commands.DeleteAppointment;

public class DeleteAppointmentCommandValidator : AbstractValidator<DeleteAppointmentCommand>
{
    public DeleteAppointmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("AppointmentId is required");
    }
}