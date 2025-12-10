using MediatR;
using MedicalApp.Application.Features.Patients.Commands.CreatePatient;
using MedicalApp.Application.Features.Patients.Commands.DeletePatient;
using MedicalApp.Application.Features.Patients.Commands.UpdatePatient;
using MedicalApp.Application.Features.Patients.Queries.GetAllPatients;
using MedicalApp.Application.Features.Patients.Queries.GetByCnp;
using MedicalApp.Application.Features.Patients.Queries.GetById;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApp.Endpoints;

public static class PatientEndpoints
{
    public static void MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients").WithTags("Patients");
        group.MapPost("/", CreatePatient);
        group.MapPut("/{id:guid}", UpdatePatient);
        group.MapDelete("/{id:guid}", DeletePatient);
        group.MapGet("/", GetAllPatients);
        group.MapGet("/{id:guid}", GetPatientById);
        group.MapGet("/cnp/{cnp}", GetPatientByCnp);
    }

    private static async Task<IResult> CreatePatient([FromBody] CreatePatientCommand command, ISender sender)
    {
        var createdId = await sender.Send(command);
        return Results.Created($"/api/patients/{createdId}", new { Id = createdId });
    }

    private static async Task<IResult> UpdatePatient(Guid id, [FromBody] UpdatePatientCommand command, ISender sender)
    {
        var newCommand = command with { Id = id };
        await sender.Send(newCommand);
        return Results.NoContent();
    }

    private static async Task<IResult> DeletePatient(Guid id, ISender sender)
    {
        var command = new DeletePatientCommand(id);
        await sender.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAllPatients(ISender sender)
    {
        var query = new GetAllPatientsQuery();
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPatientById(Guid id, ISender sender)
    {
        var query = new GetPatientByIdQuery(id);
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPatientByCnp(string cnp, ISender sender)
    {
        var query = new GetPatientByCnpQuery(cnp);
        var result = await sender.Send(query);
        return Results.Ok(result);
    }
}