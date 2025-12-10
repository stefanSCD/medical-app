using MediatR;

namespace MedicalApp.Application.Features.Doctors.Queries.GetById;

public record GetDoctorByIdQuery(Guid Id) : IRequest<DoctorDto>;
