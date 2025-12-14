using FluentAssertions;
using MedicalApp.Application.Features.Patients.Commands.CreatePatient;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace MedicalApp.Tests.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _repoMock = new();
    private readonly Mock<ICurrentUserService> _userMock = new();

    private readonly CreatePatientCommandHandler _handler;

    public CreatePatientCommandHandlerTests()
    {
        _handler = new(_repoMock.Object, _userMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CreatePatient_When_UserIsAdmin_And_CnpIsUnique()
    {
        // Arange
        var command = new CreatePatientCommand("Ion", "Popescu", "1900108123456");
        _userMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        _repoMock.Setup(x => x.GetByCnpAsync(command.PersonalNumericCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _repoMock.Verify(x => x.AddAsync(
            It.Is<Patient>(p =>
                p.FirstName == command.FirstName &&
                p.PersonalNumericCode == command.PersonalNumericCode &&
                p.UserId == null),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Unauthorized_When_UserIsNotAdmin()
    {
        // Arrange
        var command = new CreatePatientCommand("Hacker", "Test", "123");

        _userMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*authorized*");

        _repoMock.Verify(x => x.GetByCnpAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(x => x.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperation_When_CnpAlreadyExists()
    {
        // Arrange
        var cnp = "1900101123456";
        var command = new CreatePatientCommand("Ion", "Popescu", cnp);

        _userMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        var existingPatient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "Existing",
            LastName = "Patient",
            PersonalNumericCode = cnp,
            UserId = null
        };

        _repoMock.Setup(x => x.GetByCnpAsync(cnp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPatient);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"A patient with CNP {cnp} already exists.");

        _repoMock.Verify(x => x.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}