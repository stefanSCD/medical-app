using MediatR;

namespace MedicalApp.Application.Features.Appointments.Commands.DeleteAppointment;

public record DeleteAppointmentCommand(Guid Id) : IRequest;