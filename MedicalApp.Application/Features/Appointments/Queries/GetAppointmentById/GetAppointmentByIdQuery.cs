using MediatR;
namespace MedicalApp.Application.Features.Appointments.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery(Guid Id) : IRequest<AppointmentDto>;