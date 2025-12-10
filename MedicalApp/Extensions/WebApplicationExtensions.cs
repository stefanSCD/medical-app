using MedicalApp.Endpoints;

namespace MedicalApp.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapAllEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World!");

        app.MapDoctorEndpoints();
        app.MapPatientEndpoints();
        app.MapAppointmentEndpoints();
        app.MapAuthEndpoints();

        return app;
    }
}