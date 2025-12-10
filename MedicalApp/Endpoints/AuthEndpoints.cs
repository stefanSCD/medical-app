using MediatR;
using MedicalApp.Application.Features.Authentication.Commands.LoginUser;
using MedicalApp.Application.Features.Authentication.Commands.RegisterUser;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApp.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
    }

    private static async Task<IResult> Register([FromBody] RegisterUserCommand command, ISender sender)
    {
        await sender.Send(command);
        return Results.Ok(new { Message = "Registration successful" });
    }

    private static async Task<IResult> Login([FromBody] LoginUserCommand command, ISender sender)
    {
        var token = await sender.Send(command);
        return Results.Ok(new { Token = token });
    }
}