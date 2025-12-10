using MedicalApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Doctor>()
            .HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Patient>()
            .HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Doctor>()
            .Property(d => d.Specialization)
            .HasMaxLength(50);
        
        modelBuilder.Entity<Patient>()
            .Property(p => p.PersonalNumericCode)
            .IsFixedLength()
            .HasMaxLength(13);
    }
}