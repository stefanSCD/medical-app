using MedicalApp.Domain.Entities;

namespace MedicalApp.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment> AddAsync(Appointment appointment, CancellationToken ct = default);
    Task UpdateAsync(Appointment appointment, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Appointment>> GetByDoctorIdAsync(Guid doctorId, CancellationToken ct = default);
    Task<List<Appointment>> GetByPatientIdAsync(Guid patientId, CancellationToken ct = default);

    Task<bool> IsOverlappingAsync(Guid doctorId, DateTime start, DateTime end, Guid? excludeAppointmentId = null,
        CancellationToken ct = default);
}