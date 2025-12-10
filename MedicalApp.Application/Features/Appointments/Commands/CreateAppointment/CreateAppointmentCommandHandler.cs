using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Enums;

namespace MedicalApp.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler(IAppointmentRepository repository)
    : IRequestHandler<CreateAppointmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = new Domain.Entities.Appointment
        {
            DoctorId = request.DoctorId,
            PatientId = request.PatientId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = AppointmentStatus.Scheduled
        };
        await repository.AddAsync(appointment, cancellationToken);

        return appointment.Id;
    }
}