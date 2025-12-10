using MedicalApp.Domain.Entities;

namespace MedicalApp.Application.Interfaces;

public interface IPatientRepository
{
    Task<Patient> AddAsync(Patient patient, CancellationToken cancellationToken = default);
    Task<List<Patient>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default);
    Task<Patient?> GetByCnpAsync(string cnp, CancellationToken cancellationToken = default);
}