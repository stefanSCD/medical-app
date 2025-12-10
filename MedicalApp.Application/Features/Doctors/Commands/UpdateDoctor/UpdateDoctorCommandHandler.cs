using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;

namespace MedicalApp.Application.Features.Doctors.Commands.UpdateDoctor;

public class UpdateDoctorCommandHandler(IDoctorRepository repository) : IRequestHandler<UpdateDoctorCommand>
{
    public async Task Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        Doctor newDoctor = new Doctor
        {
            Id = request.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Specialization = request.Specialization
        };

        await repository.UpdateAsync(newDoctor, cancellationToken);
    }
}