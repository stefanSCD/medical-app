using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Appointments.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQueryHandler(IAppointmentRepository repository)
    : IRequestHandler<GetAppointmentsByDoctorIdQuery, List<AppointmentDto>>
{
    public async Task<List<AppointmentDto>> Handle(GetAppointmentsByDoctorIdQuery request,
        CancellationToken cancellationToken)
    {
        var appointments = await repository.GetByDoctorIdAsync(request.Id, cancellationToken);
        return appointments.Select(a => new AppointmentDto(
            a.Id,
            a.DoctorId,
            $"{a.Doctor.FirstName} {a.Doctor.LastName}",
            a.PatientId,
            $"{a.Patient.FirstName} {a.Patient.LastName}",
            a.StartDate,
            a.EndDate,
            a.Status.ToString()
        )).ToList();
    }
}