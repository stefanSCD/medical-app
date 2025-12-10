using MediatR;

namespace MedicalApp.Application.Features.Appointments.Queries.GetAppointmentByPatientId;

public record GetAppointmentsByPatientIdQuery(Guid Id) : IRequest<List<AppointmentDto>>;