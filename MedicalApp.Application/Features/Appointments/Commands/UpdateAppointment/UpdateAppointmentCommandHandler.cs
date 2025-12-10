using MediatR;
using MedicalApp.Application.Interfaces;
namespace MedicalApp.Application.Features.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentCommandHandler(IAppointmentRepository repository)
    : IRequestHandler<UpdateAppointmentCommand>
{
    public async Task Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Appointment appointment = new Domain.Entities.Appointment
        {
            Id = request.Id,
            DoctorId = request.DoctorId,
            PatientId = request.PatientId,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        await repository.UpdateAsync(appointment, cancellationToken);
    }
}