using FluentValidation.TestHelper;
using MedicalApp.Application.Features.Doctors.Commands.CreateDoctor;
using MedicalApp.Domain.Entities;
using MedicalApp.Tests.Mocks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Doctors.Commands.CreateDoctor;

public class CreateDoctorCommandValidatorTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly CreateDoctorCommandValidator _validator;
    
    public CreateDoctorCommandValidatorTests()
    {
        _userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
        
        _validator = new CreateDoctorCommandValidator(_userManagerMock.Object);
    }
    
    [Fact]
    public async Task Should_Have_Error_When_FirstName_Is_Empty()
    {
        // Arrange
        CreateDoctorCommand command =
            new CreateDoctorCommand(
                "",
                "House",
                "Diagnostic",
                "35435f",
                "StrongPassword123!"
            );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("FirstName is required");
    }

    [Fact]
    public async Task Should_Have_Error_When_Email_Is_Invalid_Format()
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

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public async Task Should_Have_Error_When_Email_Already_Exists()
    {
        // Arrange
        var email = "existing@doctor.com";
        CreateDoctorCommand command =
            new CreateDoctorCommand(
                "Gregory",
                "House",
                "Diagnostic",
                email,
                "Diagnostic31431s"
            );

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(new ApplicationUser());

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("A user with this email address already exists.");
    }

    [Fact]
    public async Task Should_Not_Have_Error_When_Data_Is_Valid()
    {
        // Arrange
        CreateDoctorCommand command =
            new CreateDoctorCommand(
                "Gregory",
                "House",
                "Diagnostic",
                "house@md.com",
                "StrongPasswrod!"
            );

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveAnyValidationErrors();
    }
}