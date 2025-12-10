using MediatR;

namespace MedicalApp.Application.Features.Patients.Commands.DeletePatient;

public record DeletePatientCommand(Guid Id) : IRequest;