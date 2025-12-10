using MediatR;

namespace MedicalApp.Application.Features.Doctors.Commands.DeleteDoctor;

public record DeleteDoctorCommand(Guid Id) : IRequest;