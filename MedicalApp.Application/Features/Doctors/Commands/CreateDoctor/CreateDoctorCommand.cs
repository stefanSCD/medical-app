using MediatR;

namespace MedicalApp.Application.Features.Doctors.Commands.CreateDoctor;

public record CreateDoctorCommand(
    string FirstName,
    string LastName,
    string Specialization,
    string Email,
    string Password
) : IRequest<Guid>;