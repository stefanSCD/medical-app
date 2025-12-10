using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Doctors.Commands.DeleteDoctor;

public class DeleteDoctorCommandHandler(IDoctorRepository repository) : IRequestHandler<DeleteDoctorCommand>
{
    public async Task Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(request.Id, cancellationToken);
    }
}