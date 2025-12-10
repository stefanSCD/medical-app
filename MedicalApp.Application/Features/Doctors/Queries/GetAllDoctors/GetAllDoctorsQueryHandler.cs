using MediatR;
using MedicalApp.Application.Interfaces;

namespace MedicalApp.Application.Features.Doctors.Queries.GetAllDoctors;

public class GetAllDoctorsQueryHandler(IDoctorRepository repository)
    : IRequestHandler<GetAllDoctorsQuery, List<DoctorDto>>
{
    public async Task<List<DoctorDto>> Handle(GetAllDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctors = await repository.GetAllAsync(cancellationToken);
        return doctors.Select(d => new DoctorDto(
            d.Id,
            $"{d.FirstName} {d.LastName}",
            d.Specialization
        )).ToList();
    }
}