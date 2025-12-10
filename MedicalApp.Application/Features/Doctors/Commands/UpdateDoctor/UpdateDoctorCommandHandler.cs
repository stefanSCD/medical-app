using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Doctors.Commands.UpdateDoctor;

public class UpdateDoctorCommandHandler(
    IDoctorRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateDoctorCommand>
{
    public async Task Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsInRole("Admin"))
        {
            throw new UnauthorizedAccessException("Only administrators are authorized to modify doctor accounts.");
        }
        
        var existingDoctor = await repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (existingDoctor == null)
        {
            throw new KeyNotFoundException($"Doctor with ID {request.Id} not found.");
        }
        
        existingDoctor.FirstName = request.FirstName;
        existingDoctor.LastName = request.LastName;
        existingDoctor.Specialization = request.Specialization;
        
        await repository.UpdateAsync(existingDoctor, cancellationToken);
    }
}