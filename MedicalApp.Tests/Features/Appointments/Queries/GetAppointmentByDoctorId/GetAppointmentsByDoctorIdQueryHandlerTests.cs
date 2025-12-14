using FluentAssertions;
using MedicalApp.Application.Features.Appointments.Queries.GetAppointmentsByDoctorId;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Domain.Enums;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Appointments.Queries.GetAppointmentByDoctorId;

public class GetAppointmentsByDoctorIdQueryHandlerTests
{
    private readonly Mock<IAppointmentRepository> _apptRepoMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepoMock = new();
    private readonly Mock<ICurrentUserService> _userMock = new();

    private readonly GetAppointmentsByDoctorIdQueryHandler _handler;

    public GetAppointmentsByDoctorIdQueryHandlerTests()
    {
        _handler = new GetAppointmentsByDoctorIdQueryHandler(
            _apptRepoMock.Object,
            _doctorRepoMock.Object,
            _userMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_Return_List_When_UserIsDoctor_And_IdsMatch()
    {
        // Arrange
        var userId = "doctor-user-123";
        var doctorId = Guid.NewGuid();
        var query = new GetAppointmentsByDoctorIdQuery(doctorId);

        var doctor = new Doctor
            { Id = doctorId, FirstName = "Gregory", LastName = "House", Specialization = "ASDAS", UserId = userId };
        var patient = new Patient
            { Id = Guid.NewGuid(), FirstName = "James", LastName = "Wilson", PersonalNumericCode = "5024126127435" };

        var appointments = new List<Appointment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                Doctor = doctor,
                PatientId = patient.Id,
                Patient = patient,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMinutes(30),
                Status = AppointmentStatus.Scheduled
            }
        };

        _userMock.Setup(x => x.UserId).Returns(userId);

        _doctorRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _apptRepoMock.Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].DoctorName.Should().Be("Gregory House");
        result[0].PatientName.Should().Be("James Wilson");
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperation_When_UserIsNotADoctor()
    {
        // Arrange
        var userId = "patient-user";
        var query = new GetAppointmentsByDoctorIdQuery(Guid.NewGuid());

        _userMock.Setup(x => x.UserId).Returns(userId);

        _doctorRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Current user is not a valid doctor.");
    }

    [Fact]
    public async Task Handle_Should_Throw_Unauthorized_When_RequestingOtherDoctorData()
    {
        // Arrange
        var userId = "my-user-id";
        var myDoctorId = Guid.NewGuid();
        var otherDoctorId = Guid.NewGuid();

        var query = new GetAppointmentsByDoctorIdQuery(otherDoctorId);

        var myDoctorProfile = new Doctor
            { Id = myDoctorId, FirstName = "Gregory", LastName = "House", Specialization = "ASDAS", UserId = userId };

        _userMock.Setup(x => x.UserId).Returns(userId);

        _doctorRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(myDoctorProfile);


        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only view your own appointments.");
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoAppointmentsFound()
    {
        // Arrange
        var userId = "user-doc";
        var doctorId = Guid.NewGuid();
        var query = new GetAppointmentsByDoctorIdQuery(doctorId);

        var doctor = new Doctor
            { Id = doctorId, FirstName = "Gregory", LastName = "House", Specialization = "ASDAS", UserId = userId };


        _userMock.Setup(x => x.UserId).Returns(userId);
        _doctorRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);

        _apptRepoMock.Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}