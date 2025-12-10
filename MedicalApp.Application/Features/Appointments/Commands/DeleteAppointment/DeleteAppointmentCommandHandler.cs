using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Appointments.Commands.DeleteAppointment;

public class DeleteAppointmentCommandHandler(IAppointmentRepository repository)
    : IRequestHandler<DeleteAppointmentCommand>
{
    public async Task Handle(DeleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(request.Id, cancellationToken);
    }
}