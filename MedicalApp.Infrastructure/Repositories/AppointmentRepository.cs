using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Domain.Enums;
using MedicalApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.Infrastructure.Repositories;

public class AppointmentRepository(AppDbContext context) : IAppointmentRepository
{
    public async Task<Appointment> AddAsync(Appointment appointment, CancellationToken ct = default)
    {
        await context.Appointments.AddAsync(appointment, ct);
        await context.SaveChangesAsync(ct);
        return appointment;
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken ct = default)
    {
        var rowsAffected = await context.Appointments
            .Where(a => a.Id == appointment.Id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.DoctorId, appointment.DoctorId)
                    .SetProperty(a => a.PatientId, appointment.PatientId)
                    .SetProperty(a => a.StartDate, appointment.StartDate)
                    .SetProperty(a => a.EndDate, appointment.EndDate)
                    .SetProperty(a => a.Status, appointment.Status),
                ct);
        if (rowsAffected == 0)
            throw new ArgumentException($"Appointment with id {appointment.Id} not found");
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rowsDeleted = await context.Appointments
            .Where(a => a.Id == id)
            .ExecuteDeleteAsync(ct);
        if (rowsDeleted == 0)
            throw new ArgumentException($"Appointment with id {id} does not exist");
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<List<Appointment>> GetByDoctorIdAsync(Guid doctorId, CancellationToken ct = default)
    {
        return await context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .Include(a => a.Patient)
            .ToListAsync(ct);
    }

    public async Task<List<Appointment>> GetByPatientIdAsync(Guid patientId, CancellationToken ct = default)
    {
        return await context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .Include(a => a.Doctor)
            .ToListAsync(ct);
    }

    public async Task<bool> IsOverlappingAsync(Guid doctorId, DateTime start, DateTime end,
        Guid? excludeAppointmentId = null,
        CancellationToken ct = default)
    {
        var query = context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .Where(a => a.Status != AppointmentStatus.Canceled)
            .Where(a => a.StartDate < end && a.EndDate > start);

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        return await query.AnyAsync(ct);
    }
}