using FluentAssertions;
using MedicalApp.Application.Features.Appointments.Queries.GetAppointmentById;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Domain.Enums;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryHandlerTests
{
    private readonly Mock<IAppointmentRepository> _repoMock = new();
    private readonly Mock<IAppointmentSecurityService> _securityMock = new();
    private readonly GetAppointmentByIdQueryHandler _handler;

    public GetAppointmentByIdQueryHandlerTests()
    {
        _handler = new GetAppointmentByIdQueryHandler(_repoMock.Object, _securityMock.Object);
    }
    
    [Fact]
    public async Task Handle_Should_Return_Dto_When_Authorized_And_Exists()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var query = new GetAppointmentByIdQuery(appointmentId);

        var doctor = new Doctor { Id = Guid.NewGuid(), FirstName = "Dr", LastName = "House", Specialization = "Doctor"};
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", PersonalNumericCode = "5050105081612"};

        var appointment = new Appointment
        {
            Id = appointmentId,
            DoctorId = doctor.Id,
            Doctor = doctor,
            PatientId = patient.Id,
            Patient = patient,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMinutes(30),
            Status = AppointmentStatus.Scheduled
        };

        _repoMock.Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _securityMock.Setup(x => x.CanViewAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(appointmentId);
        
        result.DoctorName.Should().Be("Dr House");
        result.PatientName.Should().Be("John Doe");
        
        result.Status.Should().Be("Scheduled");
    }

    [Fact]
    public async Task Handle_Should_Throw_ArgumentException_When_AppointmentNotFound()
    {
        // Arrange
        var query = new GetAppointmentByIdQuery(Guid.NewGuid());

        _repoMock.Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Appointment with id {query.Id} not found");
            
        _securityMock.Verify(x => x.CanViewAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperation_When_NotAuthorized()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var query = new GetAppointmentByIdQuery(appointmentId);
        
        var appointment = new Appointment { Id = appointmentId };

        _repoMock.Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _securityMock.Setup(x => x.CanViewAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You do not have permission to view this appointment.");
    }
}