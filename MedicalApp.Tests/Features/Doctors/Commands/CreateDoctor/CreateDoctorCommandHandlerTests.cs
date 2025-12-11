using FluentAssertions;
using MedicalApp.Application.Features.Doctors.Commands.CreateDoctor;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Tests.Mocks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Doctors.Commands.CreateDoctor;

public class CreateDoctorCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _repoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ICurrentUserService> _userServiceMock = new();

    private readonly CreateDoctorCommandHandler _handler;

    public CreateDoctorCommandHandlerTests()
    {
        _userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();

        _handler = new(
            _repoMock.Object,
            _userManagerMock.Object,
            _userServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_CreateDoctor_When_UserIsAdmin_And_DataIsValid()
    {
        // Arrange
        CreateDoctorCommand command =
            new CreateDoctorCommand(
                "Gregory",
                "House",
                "Diagnostic",
                "35435f",
                "StrongPassword123!"
            );

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _userManagerMock.Verify(x => x.CreateAsync(
            It.Is<ApplicationUser>(u => u.Email == command.Email),
            command.Password), Times.Once);

        _userManagerMock.Verify(x => x.AddToRoleAsync(
            It.IsAny<ApplicationUser>(), "Doctor"), Times.Once);

        _repoMock.Verify(x => x.AddAsync(
            It.Is<Doctor>(d => d.FirstName == "Gregory" && d.UserId != null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Unauthorized_When_UserIsNotAdmin()
    {
        // Arrange
        CreateDoctorCommand command =
            new CreateDoctorCommand(
                "Gregory",
                "House",
                "Diagnostic",
                "35435f",
                "StrongPassword123!"
            );

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*authorized*");

        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _repoMock.Verify(x => x.AddAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_ArgumentException_When_EmailAlreadyExists()
    {
        // Arrange
        CreateDoctorCommand command =
            new CreateDoctorCommand(
                "Gregory",
                "House",
                "Diagnostic",
                "35435f",
                "StrongPassword123!"
            );

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(new ApplicationUser());

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"User with email {command.Email} already exists.");

        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_ArgumentException_When_IdentityCreationFails()
    {
        CreateDoctorCommand command =
            new CreateDoctorCommand(
                "Gregory",
                "House",
                "Diagnostic",
                "35435f",
                "StrongPassword123!"
            );

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email)).ReturnsAsync((ApplicationUser?)null);

        var identityError = IdentityResult.Failed(new IdentityError { Description = "Password too short" });
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(identityError);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Password too short*");
    }
}