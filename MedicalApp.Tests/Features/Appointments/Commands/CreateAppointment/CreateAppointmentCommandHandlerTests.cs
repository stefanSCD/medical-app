using FluentAssertions;
using MedicalApp.Application.Features.Appointments.Commands.CreateAppointment;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Domain.Enums;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock = new();
    private readonly Mock<IPatientRepository> _patientRepoMock = new();
    private readonly Mock<ICurrentUserService> _userServiceMock = new();

    private readonly CreateAppointmentCommandHandler _handler;

    public CreateAppointmentCommandHandlerTests()
    {
        _handler = new CreateAppointmentCommandHandler(
            _appointmentRepoMock.Object,
            _patientRepoMock.Object,
            _userServiceMock.Object
        );
    }
    
    [Fact]
    public async Task Handle_Should_CreateAppointment_When_PatientExists_And_DoctorIsFree()
    {
        // Arrange
        var userId = "user-guid-123";
        var doctorId = Guid.NewGuid();
        var realPatientId = Guid.NewGuid();
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddMinutes(30);
        
        var command = new CreateAppointmentCommand(doctorId, Guid.NewGuid(), startDate, endDate);

        var patient = new Patient
        {
            Id = realPatientId,
            UserId = userId,
            FirstName = "Test",
            LastName = "Patient",
            PersonalNumericCode = "1900101123456"
        };

        _userServiceMock.Setup(x => x.UserId).Returns(userId);

        _patientRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _appointmentRepoMock.Setup(x => x.IsOverlappingAsync(doctorId, startDate, endDate, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _appointmentRepoMock.Verify(x => x.AddAsync(
            It.Is<Appointment>(a =>
                a.DoctorId == doctorId &&
                a.PatientId == realPatientId &&
                a.StartDate == startDate &&
                a.EndDate == endDate &&
                a.Status == AppointmentStatus.Scheduled
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperation_When_UserIsNotRegisteredAsPatient()
    {
        // Arrange
        var userId = "new-user-id";
        var command = new CreateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMinutes(30));

        _userServiceMock.Setup(x => x.UserId).Returns(userId);

        _patientRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Current user is not registered as a patient.");
            
        _appointmentRepoMock.Verify(x => x.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperation_When_DoctorIsAlreadyBooked()
    {
        // Arrange
        var userId = "user-123";
        var doctorId = Guid.NewGuid();
        var start = DateTime.Now.AddDays(1);
        var end = start.AddMinutes(30);

        var command = new CreateAppointmentCommand(doctorId, Guid.NewGuid(), start, end);
        var patient = new Patient { Id = Guid.NewGuid(), UserId = userId, FirstName = "A", LastName = "B", PersonalNumericCode = "123" };

        _userServiceMock.Setup(x => x.UserId).Returns(userId);
        
        _patientRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _appointmentRepoMock.Setup(x => x.IsOverlappingAsync(doctorId, start, end, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Doctor is already booked.");
    }
}