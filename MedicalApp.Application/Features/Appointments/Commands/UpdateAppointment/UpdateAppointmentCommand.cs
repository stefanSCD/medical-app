using MediatR;

namespace MedicalApp.Application.Features.Appointments.Commands.UpdateAppointment;

public record UpdateAppointmentCommand(
    Guid Id,
    Guid DoctorId,
    Guid PatientId,
    DateTime StartDate,
    DateTime EndDate,
    string Status
) : IRequest;