using MedicalApp.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MedicalApp.Application.Services;

public class AppointmentSecurityService(
    IAppointmentRepository appointmentRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    ICurrentUserService currentUserService,
    IHttpContextAccessor httpContextAccessor) : IAppointmentSecurityService
{
    public async Task<bool> CanModifyAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null)
            throw new KeyNotFoundException("Appointment not found");

        var currentUserId = currentUserService.UserId;
        var user = httpContextAccessor.HttpContext?.User;
        var isAdmin = user?.IsInRole("Admin") ?? false;

        if (isAdmin) return true;

        var currentPatient = await patientRepository.GetByUserIdAsync(currentUserId!, cancellationToken);
        if (currentPatient != null && appointment.PatientId == currentPatient.Id)
        {
            return true;
        }

        var currentDoctor = await doctorRepository.GetByUserIdAsync(currentUserId!, cancellationToken);
        if (currentDoctor != null && appointment.DoctorId == currentDoctor.Id)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> CanViewAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        return await CanModifyAsync(appointmentId, cancellationToken);
    }
}