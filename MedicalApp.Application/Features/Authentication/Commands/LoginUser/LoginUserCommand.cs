using MediatR;

namespace MedicalApp.Application.Features.Authentication.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<string>;