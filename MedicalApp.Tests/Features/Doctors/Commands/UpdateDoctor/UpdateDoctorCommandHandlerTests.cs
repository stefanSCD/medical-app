using FluentAssertions;
using MedicalApp.Application.Features.Doctors.Commands.UpdateDoctor;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Doctors.Commands.UpdateDoctor;

public class UpdateDoctorCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _repoMock = new();
    private readonly Mock<ICurrentUserService> _userServiceMock = new();

    private readonly UpdateDoctorCommandHandler _handler;

    public UpdateDoctorCommandHandlerTests()
    {
        _handler = new(_repoMock.Object, _userServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_UpdateDoctor_When_UserIsAdmin_And_DoctorExists()
    {
        // Arrange
        var doctorId = Guid.NewGuid();

        var command = new UpdateDoctorCommand(
            doctorId,
            "NewFirstName",
            "NewLastName",
            "NewSpecialization"
        );

        var existingDoctor = new Doctor
        {
            Id = doctorId,
            FirstName = "OldName",
            LastName = "OldLast",
            Specialization = "OldSpec",
            UserId = "user-guid-1",
        };

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        _repoMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDoctor);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingDoctor.FirstName.Should().Be("NewFirstName");
        existingDoctor.LastName.Should().Be("NewLastName");
        existingDoctor.Specialization.Should().Be("NewSpecialization");

        _repoMock.Verify(x => x.UpdateAsync(
            It.Is<Doctor>(d => d.Id == doctorId && d.FirstName == "NewFirstName"),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Unauthorized_When_UserIsNotAdmin()
    {
        // Arrange
        var command = new UpdateDoctorCommand(Guid.NewGuid(), "A", "B", "C");

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*authorized*");

        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_KeyNotFound_When_DoctorDoesNotExist()
    {
        // Arrange
        var command = new UpdateDoctorCommand(Guid.NewGuid(), "A", "B", "C");

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        _repoMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Doctor with ID {command.Id} not found.");

        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}