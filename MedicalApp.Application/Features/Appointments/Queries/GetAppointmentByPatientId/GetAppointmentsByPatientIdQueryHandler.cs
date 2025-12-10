using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Appointments.Queries.GetAppointmentByPatientId;

public class GetAppointmentsByPatientIdQueryHandler(IAppointmentRepository repository) : IRequestHandler<GetAppointmentsByPatientIdQuery, List<AppointmentDto>> 
{
    public async Task<List<AppointmentDto>> Handle(GetAppointmentsByPatientIdQuery request, CancellationToken cancellationToken)
    {
        var appointments = await repository.GetByPatientIdAsync(request.Id, cancellationToken);
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