using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Patients.Queries.GetByCnp;

public class GetPatientByCnpQueryHandler(IPatientRepository repository) : IRequestHandler<GetPatientByCnpQuery, PatientDto>
{
    public async Task<PatientDto> Handle(GetPatientByCnpQuery request, CancellationToken cancellationToken)
    {
        var patient = await repository.GetByCnpAsync(request.Cnp, cancellationToken);

        if (patient is null)
        {
            throw new ArgumentException($"Patient with CNP {request.Cnp} was not found.");
        }

        return new PatientDto(
            patient.Id,
            $"{patient.FirstName} {patient.LastName}",
            patient.PersonalNumericCode
        );
    }
}