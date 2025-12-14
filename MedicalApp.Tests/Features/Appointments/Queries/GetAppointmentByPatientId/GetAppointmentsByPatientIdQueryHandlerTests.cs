using FluentAssertions;
using MedicalApp.Application.Features.Appointments.Queries.GetAppointmentByPatientId;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Domain.Enums;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Appointments.Queries.GetAppointmentByPatientId;

public class GetAppointmentsByPatientIdQueryHandlerTests
{
    private readonly Mock<IAppointmentRepository> _apptRepoMock = new();
    private readonly Mock<IPatientRepository> _patientRepoMock = new();
    private readonly Mock<ICurrentUserService> _userMock = new();

    private readonly GetAppointmentsByPatientIdQueryHandler _handler;

    public GetAppointmentsByPatientIdQueryHandlerTests()
    {
        _handler = new GetAppointmentsByPatientIdQueryHandler(
            _apptRepoMock.Object,
            _patientRepoMock.Object,
            _userMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_Return_List_When_UserIsPatient_And_IdsMatch()
    {
        // Arrange
        var userId = "user-123";
        var patientId = Guid.NewGuid();
        var query = new GetAppointmentsByPatientIdQuery(patientId);

        var doctor = new Doctor { Id = Guid.NewGuid(), FirstName = "Gregory", LastName = "House", Specialization = "ASDAS"};
        var patient = new Patient { Id = patientId, UserId = userId, FirstName = "James", LastName = "Wilson", PersonalNumericCode = "5024126127435" };

        var appointments = new List<Appointment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                Doctor = doctor,
                PatientId = patientId,
                Patient = patient,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMinutes(30),
                Status = AppointmentStatus.Scheduled
            }
        };

        _userMock.Setup(x => x.UserId).Returns(userId);

        _patientRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _apptRepoMock.Setup(x => x.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].DoctorName.Should().Be("Gregory House");
        result[0].PatientName.Should().Be("James Wilson");
        result[0].Status.Should().Be("Scheduled");
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperation_When_UserIsNotAPatient()
    {
        // Arrange
        var userId = "admin-user";
        var query = new GetAppointmentsByPatientIdQuery(Guid.NewGuid());

        _userMock.Setup(x => x.UserId).Returns(userId);

        _patientRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Current user is not a valid patient.");
    }

    [Fact]
    public async Task Handle_Should_Throw_Unauthorized_When_RequestingOtherPatientData()
    {
        // Arrange
        var userId = "user-me";
        var myPatientId = Guid.NewGuid();
        var otherPatientId = Guid.NewGuid();

        var query = new GetAppointmentsByPatientIdQuery(otherPatientId);

        var myPatientProfile = new Patient { Id = myPatientId, UserId = userId,
            PersonalNumericCode = "5024126127435", FirstName = "James", LastName = "Wilson" };

        _userMock.Setup(x => x.UserId).Returns(userId);

        _patientRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(myPatientProfile);


        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only view your own appointments.");
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoAppointmentsFound()
    {
        // Arrange
        var userId = "user-123";
        var patientId = Guid.NewGuid();
        var query = new GetAppointmentsByPatientIdQuery(patientId);

        var patient = new Patient
        {
            Id = patientId, UserId = userId, PersonalNumericCode = "5024126127435", FirstName = "James",
            LastName = "Wilson"
        };

        _userMock.Setup(x => x.UserId).Returns(userId);
        _patientRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(patient);

        _apptRepoMock.Setup(x => x.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}