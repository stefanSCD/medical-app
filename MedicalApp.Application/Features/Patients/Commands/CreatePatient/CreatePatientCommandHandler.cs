using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;

namespace MedicalApp.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandler(
    IPatientRepository repository,
    ICurrentUserService currentUserService) : IRequestHandler<CreatePatientCommand, Guid>
{
    public async Task<Guid> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsInRole("Admin"))
        {
            throw new UnauthorizedAccessException("Only administrators are authorized to create patient profiles.");
        }
        
        var existingPatient = await repository.GetByCnpAsync(request.PersonalNumericCode, cancellationToken);
        if (existingPatient != null)
        {
            throw new InvalidOperationException($"A patient with CNP {request.PersonalNumericCode} already exists.");
        }
        
        Patient patient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PersonalNumericCode = request.PersonalNumericCode,
            UserId = null
        };
        
        await repository.AddAsync(patient, cancellationToken);
        return patient.Id;
    }
}