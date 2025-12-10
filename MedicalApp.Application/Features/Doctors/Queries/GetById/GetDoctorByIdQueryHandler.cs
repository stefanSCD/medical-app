using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;

namespace MedicalApp.Application.Features.Doctors.Queries.GetById;

public class GetDoctorByIdQueryHandler(IDoctorRepository repository) : IRequestHandler<GetDoctorByIdQuery, DoctorDto>
{
    public async Task<DoctorDto> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        Doctor? doctor = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (doctor == null)
            throw new ArgumentException($"Doctor with id {request.Id} does not exists");
        return new DoctorDto(
            doctor.Id,
            $"{doctor.FirstName} {doctor.LastName}",
            doctor.Specialization
            );
    }
}