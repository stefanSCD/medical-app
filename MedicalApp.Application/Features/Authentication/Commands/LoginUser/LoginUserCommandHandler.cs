using MediatR;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MedicalApp.Application.Features.Authentication.Commands.LoginUser;

public class LoginUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator) : IRequestHandler<LoginUserCommand, string>
{
    public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new Exception("User not found");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new Exception("Invalid password");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = tokenGenerator.GenerateToken(user, roles);
        return token;
    }
}