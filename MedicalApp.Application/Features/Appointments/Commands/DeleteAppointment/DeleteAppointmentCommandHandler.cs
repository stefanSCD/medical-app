using MediatR;
using MedicalApp.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MedicalApp.Application.Features.Appointments.Commands.DeleteAppointment;

public class DeleteAppointmentCommandHandler(
    IAppointmentRepository repository,
    IAppointmentSecurityService securityService)
    : IRequestHandler<DeleteAppointmentCommand>
{
    public async Task Handle(DeleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var canDelete = await securityService.CanModifyAsync(request.Id, cancellationToken);
        if (!canDelete)
        {
            throw new UnauthorizedAccessException("Not authorized to delete this appointment.");
        }

        await repository.DeleteAsync(request.Id, cancellationToken);
    }
}