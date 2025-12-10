using FluentValidation;
using MedicalApp.Application.Behaviors;
using MedicalApp.Application.Features.Doctors.Commands.CreateDoctor;
using MedicalApp.Application.Interfaces;
using MedicalApp.Application.Services;

namespace MedicalApp.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAppointmentSecurityService, AppointmentSecurityService>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateDoctorCommand).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(CreateDoctorCommand).Assembly);

        return services;
    }
}