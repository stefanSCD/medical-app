using MediatR;

namespace MedicalApp.Application.Features.Patients.Commands.CreatePatient;

public record CreatePatientCommand(string FirstName, string LastName, string PersonalNumericCode) : IRequest<Guid>;
