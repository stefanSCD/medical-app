namespace MedicalApp.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    bool IsInRole(string role);
}