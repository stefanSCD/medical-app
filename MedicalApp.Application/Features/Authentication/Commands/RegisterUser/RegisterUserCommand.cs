using MediatR;

namespace MedicalApp.Application.Features.Authentication.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName, String PersonalNumericCode) : IRequest;