using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Patients.Commands.DeletePatient;

public class DeletePatientCommandHandler(IPatientRepository repository) : IRequestHandler<DeletePatientCommand>
{
    public async Task Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(request.Id, cancellationToken);
    }
}