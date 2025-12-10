using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MedicalApp.Application.Features.Authentication.Commands.RegisterUser;

public class RegisterUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IPatientRepository patientRepository) : IRequestHandler<RegisterUserCommand>
{
    public async Task Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ArgumentException($"User with email {request.Email} already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ArgumentException($"Registration failed: {errors}");
        }

        await userManager.AddToRoleAsync(user, "Patient");

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserId = user.Id,
            PersonalNumericCode = request.PersonalNumericCode
        };
        await patientRepository.AddAsync(patient, cancellationToken);
    }
}