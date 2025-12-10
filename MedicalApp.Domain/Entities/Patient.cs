namespace MedicalApp.Domain.Entities;

public class Patient : BaseEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PersonalNumericCode { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = [];
}