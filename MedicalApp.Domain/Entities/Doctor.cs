namespace MedicalApp.Domain.Entities;

public class Doctor : BaseEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Specialization { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = [];
}