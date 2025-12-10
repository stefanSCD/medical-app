using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Appointments.Queries.GetAppointmentByPatientId;

public class GetAppointmentsByPatientIdQueryHandler(
    IAppointmentRepository repository,
    IPatientRepository patientRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAppointmentsByPatientIdQuery, List<AppointmentDto>>
{
    public async Task<List<AppointmentDto>> Handle(GetAppointmentsByPatientIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var currentPatient = await patientRepository.GetByUserIdAsync(userId!, cancellationToken);

        if (currentPatient == null)
        {
            throw new InvalidOperationException("Current user is not a valid patient.");
        }
        
        if (request.Id != currentPatient.Id)
        {
            throw new UnauthorizedAccessException("You can only view your own appointments.");
        }

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