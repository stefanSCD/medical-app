namespace MedicalApp.Application.Features.Appointments.Queries;

public record AppointmentDto(
    Guid Id,
    Guid DoctorId,
    string DoctorName,
    Guid PatientId,
    string PatientName,
    DateTime StartDate,
    DateTime EndDate,
    string Status
);