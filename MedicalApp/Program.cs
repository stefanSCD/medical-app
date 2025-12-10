using FluentValidation;
using MedicalApp.Application.Behaviors;
using MedicalApp.Application.Features.Doctors.Commands.CreateDoctor;
using MedicalApp.Application.Interfaces;
using MedicalApp.Endpoints;
using MedicalApp.Infrastructure.Persistence;
using MedicalApp.Infrastructure.Repositories;
using MedicalApp.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

builder.Services.AddValidatorsFromAssembly(typeof(CreateDoctorCommand).Assembly);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateDoctorCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/", () => "Hello World!");

app.MapDoctorEndpoints();
app.MapPatientEndpoints();
app.MapAppointmentEndpoints();

app.Run();