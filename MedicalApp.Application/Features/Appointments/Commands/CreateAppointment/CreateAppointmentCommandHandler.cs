using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Enums;

namespace MedicalApp.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IPatientRepository patientRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateAppointmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var patient = await patientRepository.GetByUserIdAsync(userId!, cancellationToken);

        if (patient == null)
        {
            throw new InvalidOperationException("Current user is not registered as a patient.");
        }

        var isOverlapping = await appointmentRepository.IsOverlappingAsync(
            request.DoctorId, request.StartDate, request.EndDate, null, cancellationToken);

        if (isOverlapping)
        {
            throw new InvalidOperationException("Doctor is already booked.");
        }

        var appointment = new Domain.Entities.Appointment
        {
            Id = Guid.NewGuid(),
            DoctorId = request.DoctorId,
            PatientId = patient.Id,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = AppointmentStatus.Scheduled
        };
        await appointmentRepository.AddAsync(appointment, cancellationToken);

        return appointment.Id;
    }
}