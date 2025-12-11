using FluentAssertions;
using MedicalApp.Application.Features.Patients.Commands.UpdatePatient;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _repoMock = new();
    private readonly Mock<ICurrentUserService> _userServiceMock = new();
    
    private readonly UpdatePatientCommandHandler _handler;

    public UpdatePatientCommandHandlerTests()
    {
        _handler = new(_repoMock.Object, _userServiceMock.Object);
    }
    
    [Fact]
    public async Task Handle_Should_Update_When_UserIsAdmin_And_CnpIsUnique()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var newCnp = "1990000123456";
        
        var command = new UpdatePatientCommand(patientId, "NewFirst", "NewLast", newCnp);

        var existingPatient = new Patient
        {
            Id = patientId,
            FirstName = "OldFirst",
            LastName = "OldLast",
            PersonalNumericCode = "1880000123456",
            UserId = "user-1"
        };

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        _repoMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPatient);

        _repoMock.Setup(x => x.GetByCnpAsync(newCnp, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingPatient.FirstName.Should().Be("NewFirst");
        existingPatient.PersonalNumericCode.Should().Be(newCnp);

        _repoMock.Verify(x => x.UpdateAsync(existingPatient, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Update_When_UserIsOwner()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var myUserId = "my-user-id";
        
        var command = new UpdatePatientCommand(patientId, "NewName", "NewLast", "1900101123456");

        var existingPatient = new Patient
        {
            Id = patientId,
            UserId = myUserId,
            FirstName = "Old",
            LastName = "Name",
            PersonalNumericCode = "1900101123456"
        };
        
        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);
        _userServiceMock.Setup(x => x.UserId).Returns(myUserId);

        _repoMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPatient);

        _repoMock.Setup(x => x.GetByCnpAsync(command.PersonalNumericCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPatient);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repoMock.Verify(x => x.UpdateAsync(existingPatient, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Unauthorized_When_UserIsNotAdmin_And_NotOwner()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var command = new UpdatePatientCommand(patientId, "A", "B", "123");

        var existingPatient = new Patient
        {
            Id = patientId,
            UserId = "other-user-id", 
            FirstName = "Target",
            LastName = "Patient",
            PersonalNumericCode = "111"
        };

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);
        _userServiceMock.Setup(x => x.UserId).Returns("hacker-id");

        _repoMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPatient);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not authorized*");

        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperation_When_CnpBelongsToOtherPatient()
    {
        var patientId = Guid.NewGuid();
        var conflictingCnp = "1999999999999";
        var command = new UpdatePatientCommand(patientId, "A", "B", conflictingCnp);

        var existingPatient = new Patient { Id = patientId, UserId = "u1", PersonalNumericCode = "111", FirstName = "F", LastName = "L" };
        
        var otherPatient = new Patient { Id = Guid.NewGuid(), PersonalNumericCode = conflictingCnp, FirstName = "Other", LastName = "One" };

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);
        _repoMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>())).ReturnsAsync(existingPatient);
        
        _repoMock.Setup(x => x.GetByCnpAsync(conflictingCnp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherPatient);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"A patient with CNP {conflictingCnp} already exists.");
            
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_KeyNotFound_When_PatientDoesNotExist()
    {
        var command = new UpdatePatientCommand(Guid.NewGuid(), "A", "B", "123");
        _repoMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }
}