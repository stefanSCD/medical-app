namespace MedicalApp.Application.Interfaces;

public interface IAppointmentSecurityService
{
    Task<bool> CanModifyAsync(Guid appointmentId, CancellationToken cancellationToken);
    Task<bool> CanViewAsync(Guid appointmentId, CancellationToken cancellationToken);
}