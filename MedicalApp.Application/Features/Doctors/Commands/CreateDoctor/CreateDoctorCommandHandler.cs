using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;

namespace MedicalApp.Application.Features.Doctors.Commands.CreateDoctor;

public class CreateDoctorCommandHandler(IDoctorRepository repository) : IRequestHandler<CreateDoctorCommand, Guid>
{
    public async Task<Guid> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = new Doctor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Specialization = request.Specialization
        };
        await repository.AddAsync(doctor, cancellationToken);
        return doctor.Id;
    }
}