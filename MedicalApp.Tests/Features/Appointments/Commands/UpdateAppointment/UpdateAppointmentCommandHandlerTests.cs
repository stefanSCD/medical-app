using FluentAssertions;
using MedicalApp.Application.Features.Appointments.Commands.UpdateAppointment;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Domain.Enums;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _repoMock = new();
    private readonly Mock<IAppointmentSecurityService> _securityMock = new();
    private readonly UpdateAppointmentCommandHandler _handler;

    public UpdateAppointmentCommandHandlerTests()
    {
        _handler = new UpdateAppointmentCommandHandler(_repoMock.Object, _securityMock.Object);
    }
    
    [Fact]
    public async Task Handle_Should_Update_When_ValidationPasses_And_StatusIsValid()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var newDoctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var startDate = DateTime.Now.AddDays(2);
        var endDate = startDate.AddMinutes(30);
        var newStatusString = "Completed";

        var command = new UpdateAppointmentCommand(
            appointmentId, 
            newDoctorId, 
            patientId, 
            startDate, 
            endDate, 
            newStatusString
        );

        var existingAppointment = new Appointment
        {
            Id = appointmentId,
            DoctorId = Guid.NewGuid(),
            PatientId = patientId,
            StartDate = DateTime.Now,
            Status = AppointmentStatus.Scheduled
        };

        _securityMock.Setup(x => x.CanModifyAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repoMock.Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAppointment);

        _repoMock.Setup(x => x.IsOverlappingAsync(newDoctorId, startDate, endDate, appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingAppointment.DoctorId.Should().Be(newDoctorId);
        existingAppointment.Status.Should().Be(AppointmentStatus.Completed);
        existingAppointment.StartDate.Should().Be(startDate);
        
        _repoMock.Verify(x => x.UpdateAsync(existingAppointment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Unauthorized_When_SecurityService_Denies()
    {
        // Arrange
        var command = new UpdateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, DateTime.Now, "Scheduled");

        _securityMock.Setup(x => x.CanModifyAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Not authorized to modify this appointment.");

        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_KeyNotFound_When_AppointmentDoesNotExist()
    {
        // Arrange
        var command = new UpdateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, DateTime.Now, "Scheduled");

        _securityMock.Setup(x => x.CanModifyAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        _repoMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Appointment with id {command.Id} not found.");
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperation_When_OverlapDetected()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var start = DateTime.Now;
        var end = start.AddMinutes(30);

        var command = new UpdateAppointmentCommand(appointmentId, doctorId, Guid.NewGuid(), start, end, "Scheduled");
        var existingAppointment = new Appointment { Id = appointmentId };

        _securityMock.Setup(x => x.CanModifyAsync(appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repoMock.Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(existingAppointment);

        _repoMock.Setup(x => x.IsOverlappingAsync(doctorId, start, end, appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Doctor is already booked.");
    }

    [Fact]
    public async Task Handle_Should_Throw_ArgumentException_When_StatusEnumIsInvalid()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var command = new UpdateAppointmentCommand(
            appointmentId, 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            DateTime.Now, 
            DateTime.Now.AddMinutes(30), 
            "StatusJaponez"
        );

        var existingAppointment = new Appointment { Id = appointmentId };

        _securityMock.Setup(x => x.CanModifyAsync(appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repoMock.Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>())).ReturnsAsync(existingAppointment);
        _repoMock.Setup(x => x.IsOverlappingAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Status 'StatusJaponez' is invalid*");
    }
}