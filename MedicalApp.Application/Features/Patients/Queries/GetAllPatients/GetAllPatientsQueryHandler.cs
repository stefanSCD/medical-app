using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Patients.Queries.GetAllPatients;

public class GetAllPatientsQueryHandler(IPatientRepository repository)
    : IRequestHandler<GetAllPatientsQuery, List<PatientDto>>
{
    public async Task<List<PatientDto>> Handle(GetAllPatientsQuery request, CancellationToken cancellationToken)
    {
        var patients = await repository.GetAllAsync(cancellationToken);
        return patients.Select(p => new PatientDto(
            p.Id,
            $"{p.FirstName} {p.LastName}",
            p.PersonalNumericCode
        )).ToList();
    }
}