using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MedicalApp.Application.Features.Patients.Commands.DeletePatient;

public class DeletePatientCommandHandler(
    IPatientRepository patientRepository,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUserService) : IRequestHandler<DeletePatientCommand>
{
    public async Task Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patientToDelete = await patientRepository.GetByIdAsync(request.Id, cancellationToken);

        if (patientToDelete == null)
        {
            throw new KeyNotFoundException($"Patient with ID {request.Id} not found.");
        }

        var currentUserId = currentUserService.UserId;
        var isAdmin = currentUserService.IsInRole("Admin");
        var isOwner = patientToDelete.UserId != null && patientToDelete.UserId == currentUserId;

        if (!isAdmin && !isOwner)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this patient profile.");
        }

        if (patientToDelete.UserId != null)
        {
            var user = await userManager.FindByIdAsync(patientToDelete.UserId);
            if (user != null)
            {
                var result = await userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to delete user account: {errors}");
                }
            }
        }

        await patientRepository.DeleteAsync(request.Id, cancellationToken);
    }
}