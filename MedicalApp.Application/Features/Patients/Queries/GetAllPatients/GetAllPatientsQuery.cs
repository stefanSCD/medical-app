using MediatR;

namespace MedicalApp.Application.Features.Patients.Queries.GetAllPatients;

public record GetAllPatientsQuery : IRequest<List<PatientDto>>;