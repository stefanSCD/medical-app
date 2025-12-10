using MediatR;

namespace MedicalApp.Application.Features.Doctors.Queries.GetAllDoctors;

public record GetAllDoctorsQuery : IRequest<List<DoctorDto>>;