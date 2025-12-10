using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Doctors.Queries.GetBySpecialization;

public class GetDoctorBySpecializationQueryHandler(IDoctorRepository repository)
    : IRequestHandler<GetDoctorBySpecializationQuery, List<DoctorDto>>
{
    public async Task<List<DoctorDto>> Handle(GetDoctorBySpecializationQuery request, CancellationToken cancellationToken)
    {
        var doctors = await repository.GetBySpecializationAsync(request.Specialization, cancellationToken);
        return doctors.Select(d => new DoctorDto(
            d.Id,
            $"{d.FirstName} {d.LastName}",
            d.Specialization
        )).ToList();
    }
}