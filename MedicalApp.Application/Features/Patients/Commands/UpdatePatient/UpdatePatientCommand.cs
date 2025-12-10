using MediatR;

namespace MedicalApp.Application.Features.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand(Guid Id, string FirstName, string LastName, string PersonalNumericCode) : IRequest;