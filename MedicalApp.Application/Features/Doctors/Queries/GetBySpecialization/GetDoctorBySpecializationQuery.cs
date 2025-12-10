using MediatR;

namespace MedicalApp.Application.Features.Doctors.Queries.GetBySpecialization;

public record GetDoctorBySpecializationQuery(string Specialization) : IRequest<List<DoctorDto>> ;