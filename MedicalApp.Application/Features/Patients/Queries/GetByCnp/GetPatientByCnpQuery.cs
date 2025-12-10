using MediatR;

namespace MedicalApp.Application.Features.Patients.Queries.GetByCnp;

public record GetPatientByCnpQuery(string Cnp) : IRequest<PatientDto>;