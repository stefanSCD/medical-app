using MediatR;

namespace MedicalApp.Application.Features.Appointments.Queries.GetAppointmentsByDoctorId;

public record GetAppointmentsByDoctorIdQuery(Guid Id) : IRequest<List<AppointmentDto>>;