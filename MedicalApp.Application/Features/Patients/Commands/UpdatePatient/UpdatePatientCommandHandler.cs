using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;

namespace MedicalApp.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandHandler(IPatientRepository repository) : IRequestHandler<UpdatePatientCommand>
{
    public async Task Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        Patient patient = new Patient
        {
            Id = request.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PersonalNumericCode = request.PersonalNumericCode
        };
        await repository.UpdateAsync(patient, cancellationToken);
    }
}