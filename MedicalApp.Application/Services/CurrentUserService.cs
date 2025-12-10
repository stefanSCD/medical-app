using System.Security.Claims;
using MedicalApp.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MedicalApp.Application.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool IsInRole(string role)
    {
        return httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}