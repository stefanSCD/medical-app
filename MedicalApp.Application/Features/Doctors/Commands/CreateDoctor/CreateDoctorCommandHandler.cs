using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MedicalApp.Application.Features.Doctors.Commands.CreateDoctor;

public class CreateDoctorCommandHandler(
    IDoctorRepository repository,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateDoctorCommand, Guid>
{
    public async Task<Guid> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsInRole("Admin"))
        {
            throw new UnauthorizedAccessException("Only administrators are authorized to create doctor accounts.");
        }
        
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ArgumentException($"User with email {request.Email} already exists.");
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ArgumentException($"User creation failed: {errors}");
        }

        await userManager.AddToRoleAsync(user, "Doctor");

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Specialization = request.Specialization,
            UserId = user.Id
        };
        await repository.AddAsync(doctor, cancellationToken);
        return doctor.Id;
    }
}