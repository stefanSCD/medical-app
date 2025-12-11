using FluentValidation.TestHelper;
using MedicalApp.Application.Features.Patients.Commands.CreatePatient;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandValidatorTests
{
    private readonly Mock<IPatientRepository> _repoMock = new();
    private readonly CreatePatientCommandValidator _validator;

    public CreatePatientCommandValidatorTests()
    {
        _validator = new CreatePatientCommandValidator(_repoMock.Object);
    }
    
    [Fact]
    public async Task Should_Have_Error_When_FirstName_Is_Invalid()
    {
        var command = new CreatePatientCommand("", "ValidLast", "1900101123456");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First name is required");
              
        var longName = new string('a', 51);
        var longCommand = new CreatePatientCommand(longName, "ValidLast", "1900101123456");
        
        var longResult = await _validator.TestValidateAsync(longCommand);
        longResult.ShouldHaveValidationErrorFor(x => x.FirstName)
                  .WithErrorMessage("First name cannot exceed 50 characters");
    }

    [Fact]
    public async Task Should_Have_Error_When_Cnp_Format_Is_Invalid()
    {
        var shortCnpCommand = new CreatePatientCommand("Valid", "Valid", "123456789012");
        var resultShort = await _validator.TestValidateAsync(shortCnpCommand);
        
        resultShort.ShouldHaveValidationErrorFor(x => x.PersonalNumericCode)
                   .WithErrorMessage("Personal numeric code should have 13 digits");

        var lettersCommand = new CreatePatientCommand("Valid", "Valid", "1234567890abc");
        var resultLetters = await _validator.TestValidateAsync(lettersCommand);
        
        resultLetters.ShouldHaveValidationErrorFor(x => x.PersonalNumericCode)
                     .WithErrorMessage("Personal numeric code must contain only digits");
    }

    [Fact]
    public async Task Should_Have_Error_When_Cnp_Already_Exists()
    {
        // Arrange
        var cnp = "1900101123456";
        var command = new CreatePatientCommand("Ion", "Popescu", cnp);

        _repoMock.Setup(x => x.GetByCnpAsync(cnp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient { FirstName = "Existent", LastName = "User", PersonalNumericCode = cnp });

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PersonalNumericCode)
              .WithErrorMessage("A patient with the specified personal numeric code already exists");
    }

    [Fact]
    public async Task Should_Not_Have_Error_When_Data_Is_Valid()
    {
        // Arrange
        var cnp = "2900101123456";
        var command = new CreatePatientCommand("Jane", "Doe", cnp);

        _repoMock.Setup(x => x.GetByCnpAsync(cnp, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}