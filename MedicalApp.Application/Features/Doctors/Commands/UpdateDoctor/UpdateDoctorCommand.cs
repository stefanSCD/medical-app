using MediatR;

namespace MedicalApp.Application.Features.Doctors.Commands.UpdateDoctor;

public record UpdateDoctorCommand(Guid Id, string FirstName, string LastName, string Specialization) : IRequest;
