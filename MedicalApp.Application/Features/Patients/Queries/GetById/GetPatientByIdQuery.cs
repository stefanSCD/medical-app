using MediatR;

namespace MedicalApp.Application.Features.Patients.Queries.GetById;

public record GetPatientByIdQuery(Guid Id) : IRequest<PatientDto>;