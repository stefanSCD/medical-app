using MediatR;
using MedicalApp.Application.Features.Appointments.Commands.CreateAppointment;
using MedicalApp.Application.Features.Appointments.Commands.DeleteAppointment;
using MedicalApp.Application.Features.Appointments.Commands.UpdateAppointment;
using MedicalApp.Application.Features.Appointments.Queries.GetAppointmentById;
using MedicalApp.Application.Features.Appointments.Queries.GetAppointmentByPatientId;
using MedicalApp.Application.Features.Appointments.Queries.GetAppointmentsByDoctorId;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApp.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .WithTags("Appointments");

        group.MapPost("/", CreateAppointment);
        group.MapGet("/doctor/{id:guid}", GetByDoctorId);
        group.MapGet("/patient/{id:guid}", GetByPatientId);
        group.MapDelete("/{id:guid}", DeleteAppointment);
        group.MapPut("/{id:guid}", UpdateAppointment);
        group.MapGet("/{id:guid}", GetById);
    }

    private static async Task<IResult> CreateAppointment([FromBody] CreateAppointmentCommand command, ISender sender)
    {
        var createdId = await sender.Send(command);
        return Results.Created($"/api/appointments/{createdId}", new { Id = createdId });
    }

    private static async Task<IResult> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentCommand command,
        ISender sender)
    {
        var newCommand = command with { Id = id };
        await sender.Send(newCommand);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAppointment(Guid id, ISender sender)
    {
        var command = new DeleteAppointmentCommand(id);
        await sender.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> GetById(Guid id, ISender sender)
    {
        var command = new GetAppointmentByIdQuery(id);
        var result = await sender.Send(command);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByDoctorId(Guid id, ISender sender)
    {
        var command = new GetAppointmentsByDoctorIdQuery(id);
        var result = await sender.Send(command);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByPatientId(Guid id, ISender sender)
    {
        var command = new GetAppointmentsByPatientIdQuery(id);
        var result = await sender.Send(command);
        return Results.Ok(result);
    }
}