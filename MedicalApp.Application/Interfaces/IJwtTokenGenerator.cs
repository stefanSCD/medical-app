using MedicalApp.Domain.Entities;

namespace MedicalApp.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}