using MediatR;

namespace MedicalApp.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand(
    Guid DoctorId,
    Guid PatientId,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<Guid>;