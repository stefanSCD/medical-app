using MedicalApp.Domain.Entities;

namespace MedicalApp.Application.Interfaces;

public interface IDoctorRepository
{
    Task<Doctor> AddAsync(Doctor doctor, CancellationToken cancellationToken = default);
    Task<List<Doctor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Doctor doctor, CancellationToken cancellationToken = default);
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Doctor>> GetBySpecializationAsync(string specialization, CancellationToken cancellationToken = default);
}