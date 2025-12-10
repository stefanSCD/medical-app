using MedicalApp.Domain.Enums;

namespace MedicalApp.Domain.Entities;

public class Appointment : BaseEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }
    public AppointmentStatus Status { get; set; }
}