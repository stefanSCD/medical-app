using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Enums;

namespace MedicalApp.Application.Features.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentCommandHandler(
    IAppointmentRepository repository,
    IAppointmentSecurityService securityService)
    : IRequestHandler<UpdateAppointmentCommand>
{
    public async Task Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var canModify = await securityService.CanModifyAsync(request.Id, cancellationToken);

        if (!canModify)
        {
            throw new UnauthorizedAccessException("Not authorized to modify this appointment.");
        }

        var existingAppointment = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (existingAppointment == null)
        {
            throw new KeyNotFoundException($"Appointment with id {request.Id} not found.");
        }

        var isOverlapping = await repository.IsOverlappingAsync(
            request.DoctorId, request.StartDate, request.EndDate, request.Id, cancellationToken);

        if (isOverlapping)
        {
            throw new InvalidOperationException("Doctor is already booked.");
        }

        bool success = Enum.TryParse(
            request.Status,
            true,
            out AppointmentStatus status
        );

        if (!success)
        {
            var validStatuses = string.Join(", ", Enum.GetNames(typeof(AppointmentStatus)));
            throw new ArgumentException($"Status '{request.Status}' is invalid. Valid values are: {validStatuses}");
        }

        existingAppointment.DoctorId = request.DoctorId;
        existingAppointment.StartDate = request.StartDate;
        existingAppointment.EndDate = request.EndDate;
        existingAppointment.Status = status;
        await repository.UpdateAsync(existingAppointment, cancellationToken);
    }
}