using FluentValidation.TestHelper;
using MedicalApp.Application.Features.Appointments.Commands.CreateAppointment;
using MedicalApp.Application.Interfaces;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidatorTests
{
    private readonly Mock<IAppointmentRepository> _repoMock = new();
    private readonly CreateAppointmentCommandValidator _validator;

    public CreateAppointmentCommandValidatorTests()
    {
        _validator = new CreateAppointmentCommandValidator(_repoMock.Object);
    }

    [Fact]
    public void Should_Have_Error_When_DoctorId_IsEmpty()
    {
        // Arrange
        var command = new CreateAppointmentCommand(
            DoctorId: Guid.Empty, 
            PatientId: Guid.NewGuid(), 
            StartDate: DateTime.Now.AddDays(1), 
            EndDate: DateTime.Now.AddDays(1).AddMinutes(30));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DoctorId)
              .WithErrorMessage("DoctorId is required");
    }

    [Fact]
    public void Should_Have_Error_When_StartDate_Is_In_The_Past()
    {
        var pastDate = DateTime.Now.AddDays(-1);
        var command = new CreateAppointmentCommand(
            DoctorId: Guid.NewGuid(),
            PatientId: Guid.NewGuid(),
            StartDate: pastDate,
            EndDate: pastDate.AddMinutes(30));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
              .WithErrorMessage("Appointment cannot be in the past");
    }

    [Fact]
    public void Should_Have_Error_When_EndDate_Is_Before_StartDate()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddMinutes(-30);

        var command = new CreateAppointmentCommand(
            DoctorId: Guid.NewGuid(),
            PatientId: Guid.NewGuid(),
            StartDate: startDate,
            EndDate: endDate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("EndDate must be after StartDate");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Data_Is_Valid()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddMinutes(30);

        var command = new CreateAppointmentCommand(
            DoctorId: Guid.NewGuid(),
            PatientId: Guid.NewGuid(),
            StartDate: startDate,
            EndDate: endDate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}