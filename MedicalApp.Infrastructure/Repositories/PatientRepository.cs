using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.Infrastructure.Repositories;

public class PatientRepository(AppDbContext context) : IPatientRepository
{
    public async Task<Patient> AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await context.Patients.AddAsync(patient, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public async Task<List<Patient>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Patients
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rowsDeleted = await context.Patients
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        
        if(rowsDeleted == 0)
            throw new ArgumentException($"Patient with id {id} not found");
    }

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await context.Patients
            .Where(p => p.Id == patient.Id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.FirstName, patient.FirstName)
                    .SetProperty(p => p.LastName, patient.LastName)
                    .SetProperty(p => p.PersonalNumericCode, patient.PersonalNumericCode),
                cancellationToken
            );
        
        if(rowsAffected == 0)
            throw new ArgumentException($"Patient with id {patient.Id} not found");
    }

    public async Task<Patient?> GetByCnpAsync(string cnp, CancellationToken cancellationToken = default)
    {
        return await context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PersonalNumericCode == cnp, cancellationToken);
    }
}