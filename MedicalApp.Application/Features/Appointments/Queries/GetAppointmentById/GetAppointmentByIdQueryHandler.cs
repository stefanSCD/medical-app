using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryHandler(IAppointmentRepository repository)
    : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto>
{
    public async Task<AppointmentDto> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
            throw new ArgumentException($"Appointment with id {request.Id} not found");
        return new AppointmentDto(
            appointment.Id,
            appointment.DoctorId,
            $"{appointment.Doctor.FirstName} {appointment.Doctor.LastName}",
            appointment.PatientId,
            $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
            appointment.StartDate,
            appointment.EndDate,
            appointment.Status.ToString()
        );
    }
}