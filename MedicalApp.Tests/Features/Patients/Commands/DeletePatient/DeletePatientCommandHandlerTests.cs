using FluentAssertions;
using MedicalApp.Application.Features.Patients.Commands.DeletePatient;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Tests.Mocks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace MedicalApp.Tests.Features.Patients.Commands.DeletePatient;

public class DeletePatientCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ICurrentUserService> _userServiceMock = new();

    private readonly DeletePatientCommandHandler _handler;

    public DeletePatientCommandHandlerTests()
    {
        _userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();

        _handler = new(
            _patientRepoMock.Object,
            _userManagerMock.Object,
            _userServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_Delete_When_UserIsAdmin()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var patientUserId = "patient-user-123";
        var command = new DeletePatientCommand(patientId);

        var patient = new Patient
        {
            Id = patientId,
            UserId = patientUserId,
            FirstName = "John",
            LastName = "Doe",
            PersonalNumericCode = "1900101123456"
        };

        var appUser = new ApplicationUser { Id = patientUserId, UserName = "john", Email = "john@test.com" };

        _userServiceMock.Setup(x => x.UserId).Returns("admin-id");
        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        _patientRepoMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _userManagerMock.Setup(x => x.FindByIdAsync(patientUserId)).ReturnsAsync(appUser);
        _userManagerMock.Setup(x => x.DeleteAsync(appUser)).ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userManagerMock.Verify(x => x.DeleteAsync(appUser), Times.Once);
        _patientRepoMock.Verify(x => x.DeleteAsync(patientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Delete_When_UserIsOwner()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var myUserId = "my-own-id";
        var command = new DeletePatientCommand(patientId);

        var patient = new Patient
        {
            Id = patientId,
            UserId = myUserId,
            FirstName = "Me",
            LastName = "Myself",
            PersonalNumericCode = "1900101123456"
        };

        var appUser = new ApplicationUser { Id = myUserId, UserName = "me", Email = "me@test.com" };

        _userServiceMock.Setup(x => x.UserId).Returns(myUserId);
        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        _patientRepoMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
        _userManagerMock.Setup(x => x.FindByIdAsync(myUserId)).ReturnsAsync(appUser);
        _userManagerMock.Setup(x => x.DeleteAsync(appUser)).ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _patientRepoMock.Verify(x => x.DeleteAsync(patientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Unauthorized_When_UserIsNotAdmin_And_NotOwner()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var command = new DeletePatientCommand(patientId);

        var patient = new Patient
        {
            Id = patientId,
            UserId = "target-user-id",
            FirstName = "Target",
            LastName = "User",
            PersonalNumericCode = "1900101123456"
        };

        _userServiceMock.Setup(x => x.UserId).Returns("hacker-id");
        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        _patientRepoMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>())).ReturnsAsync(patient);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not authorized*");

        _userManagerMock.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _patientRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_KeyNotFound_When_PatientDoesNotExist()
    {
        // Arrange
        var command = new DeletePatientCommand(Guid.NewGuid());
        _patientRepoMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Patient with ID {command.Id} not found.");
    }

    [Fact]
    public async Task Handle_Should_Delete_OnlyRepo_When_PatientHasNoUser()
    {
        var patientId = Guid.NewGuid();
        var command = new DeletePatientCommand(patientId);

        var patient = new Patient
        {
            Id = patientId,
            UserId = null,
            FirstName = "Manual",
            LastName = "Create",
            PersonalNumericCode = "1800101123456"
        };

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);
        _patientRepoMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>())).ReturnsAsync(patient);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);

        _patientRepoMock.Verify(x => x.DeleteAsync(patientId, It.IsAny<CancellationToken>()), Times.Once);
    }
}