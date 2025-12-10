using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;

namespace MedicalApp.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandler(IPatientRepository repository) : IRequestHandler<CreatePatientCommand, Guid>
{
    public async Task<Guid> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        Patient patient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PersonalNumericCode = request.PersonalNumericCode
        };
        await repository.AddAsync(patient, cancellationToken);
        return patient.Id;
    }
}