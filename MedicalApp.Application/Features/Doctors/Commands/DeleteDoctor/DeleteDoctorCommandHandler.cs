using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MedicalApp.Application.Features.Doctors.Commands.DeleteDoctor;

public class DeleteDoctorCommandHandler(
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUserService
) : IRequestHandler<DeleteDoctorCommand>
{
    public async Task Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsInRole("Admin"))
        {
            throw new UnauthorizedAccessException("Only administrators are authorized to delete doctor profiles.");
        }

        var doctorToDelete = await doctorRepository.GetByIdAsync(request.Id, cancellationToken);

        if (doctorToDelete == null)
        {
            throw new KeyNotFoundException($"Doctor with ID {request.Id} not found.");
        }

        var doctorsAppointments = await appointmentRepository.GetByDoctorIdAsync(request.Id, cancellationToken);
        foreach (var appointment in doctorsAppointments)
        {
            await appointmentRepository.DeleteAsync(appointment.Id, cancellationToken);
        }

        if (doctorToDelete.UserId != null)
        {
            var user = await userManager.FindByIdAsync(doctorToDelete.UserId);
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

        await doctorRepository.DeleteAsync(request.Id, cancellationToken);
    }
}