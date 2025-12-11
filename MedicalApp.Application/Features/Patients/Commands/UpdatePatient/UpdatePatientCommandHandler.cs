using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandHandler(
    IPatientRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<UpdatePatientCommand>
{
    public async Task Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var existingPatient = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (existingPatient == null)
        {
            throw new KeyNotFoundException($"Patient with ID {request.Id} not found.");
        }

        var currentUserId = currentUserService.UserId;
        var isAdmin = currentUserService.IsInRole("Admin");
        var isOwner = existingPatient.UserId != null && existingPatient.UserId == currentUserId;

        if (!isAdmin && !isOwner)
        {
            throw new UnauthorizedAccessException("You are not authorized to modify this patient profile.");
        }

        var patientWithSameCnp = await repository.GetByCnpAsync(request.PersonalNumericCode, cancellationToken);

        if (patientWithSameCnp != null && patientWithSameCnp.Id != existingPatient.Id)
        {
            throw new InvalidOperationException($"A patient with CNP {request.PersonalNumericCode} already exists.");
        }

        existingPatient.FirstName = request.FirstName;
        existingPatient.LastName = request.LastName;
        existingPatient.PersonalNumericCode = request.PersonalNumericCode;

        await repository.UpdateAsync(existingPatient, cancellationToken);
    }
}