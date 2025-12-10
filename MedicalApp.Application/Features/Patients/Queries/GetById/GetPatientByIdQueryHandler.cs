using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Patients.Queries.GetById;

public class GetPatientByIdQueryHandler(IPatientRepository repository) : IRequestHandler<GetPatientByIdQuery, PatientDto>
{
    public async Task<PatientDto> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (patient is null)
        {
            throw new ArgumentException($"Patient with id {request.Id} was not found.");
        }

        return new PatientDto(
            patient.Id,
            $"{patient.FirstName} {patient.LastName}",
            patient.PersonalNumericCode
        );
    }
}