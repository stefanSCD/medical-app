using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.Infrastructure.Repositories;

public class DoctorRepository(AppDbContext context) : IDoctorRepository
{
    public async Task<Doctor> AddAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        await context.Doctors.AddAsync(doctor, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return doctor;
    }

    public async Task<List<Doctor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Doctors
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rowsDeleted = await context.Doctors
            .Where(d => d.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsDeleted == 0)
            throw new ArgumentException($"Doctor with id {id} not found");
    }

    public async Task UpdateAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await context.Doctors
            .Where(d => d.Id == doctor.Id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(d => d.FirstName, doctor.FirstName)
                    .SetProperty(d => d.LastName, doctor.LastName)
                    .SetProperty(d => d.Specialization, doctor.Specialization),
                cancellationToken
            );
        if (rowsAffected == 0)
            throw new ArgumentException($"Doctor with id {doctor.Id} not found");
    }

    public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<List<Doctor>> GetBySpecializationAsync(
        string specialization, CancellationToken cancellationToken = default)
    {
        return await context.Doctors
            .AsNoTracking()
            .Where(d => d.Specialization == specialization)
            .ToListAsync(cancellationToken);
    }
}