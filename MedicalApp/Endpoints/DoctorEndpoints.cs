using MediatR;
using MedicalApp.Application.Features.Doctors.Commands.CreateDoctor;
using MedicalApp.Application.Features.Doctors.Commands.DeleteDoctor;
using MedicalApp.Application.Features.Doctors.Commands.UpdateDoctor;
using MedicalApp.Application.Features.Doctors.Queries.GetAllDoctors;
using MedicalApp.Application.Features.Doctors.Queries.GetById;
using MedicalApp.Application.Features.Doctors.Queries.GetBySpecialization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApp.Endpoints;

public static class DoctorEndpoints
{
    public static void MapDoctorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/doctors")
            .WithTags("Doctors");

        group.MapPost("/", CreateDoctor);
        group.MapGet("/", GetAllDoctors);
        group.MapDelete("/{id:guid}", DeleteDoctor); 
        group.MapPut("/{id:guid}", UpdateDoctor);
        group.MapGet("/{id:guid}", GetById);
        group.MapGet("/specialization/{specialization}", GetBySpecialization);
    }

    private static async Task<IResult> CreateDoctor([FromBody] CreateDoctorCommand command, ISender sender)
    {
        var createdId = await sender.Send(command);
        return Results.Created($"/api/doctors/{createdId}", new { Id = createdId });
    }

    private static async Task<IResult> GetAllDoctors(ISender sender)
    {
        var query = new GetAllDoctorsQuery();
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteDoctor(Guid id, ISender sender)
    {
        var command = new DeleteDoctorCommand(id);
        await sender.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateDoctor(Guid id, [FromBody] UpdateDoctorCommand command, ISender sender)
    {
        var newCommand = command with { Id = id };
        await sender.Send(newCommand);
        return Results.NoContent();
    }

    private static async Task<IResult> GetById(Guid id, ISender sender)
    {
        var query = new GetDoctorByIdQuery(id);
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetBySpecialization(string specialization, ISender sender)
    {
        var query = new GetDoctorBySpecializationQuery(specialization);
        var result = await sender.Send(query);
        return Results.Ok(result);
    }
}