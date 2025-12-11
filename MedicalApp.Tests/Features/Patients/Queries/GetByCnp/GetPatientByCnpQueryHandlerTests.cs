using FluentAssertions;
using MedicalApp.Application.Features.Patients.Queries.GetByCnp;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace MedicalApp.Tests.Features.Patients.Queries.GetByCnp;

public class GetPatientByCnpQueryHandlerTests
{
    private readonly Mock<IPatientRepository> _repoMock = new();
    private readonly GetPatientByCnpQueryHandler _handler;

    public GetPatientByCnpQueryHandlerTests()
    {
        _handler = new GetPatientByCnpQueryHandler(_repoMock.Object);
    }
    
    [Fact]
    public async Task Handle_Should_Return_PatientDto_When_PatientExists()
    {
        // Arrange
        var cnp = "1900101123456";
        var query = new GetPatientByCnpQuery(cnp);

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "Ion",
            LastName = "Popescu",
            PersonalNumericCode = cnp,
            UserId = "user-guid-123"
        };

        _repoMock.Setup(x => x.GetByCnpAsync(cnp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        result.FullName.Should().Be("Ion Popescu");
        result.PersonalNumericCode.Should().Be(cnp);
        result.Id.Should().Be(patient.Id);
    }

    [Fact]
    public async Task Handle_Should_Throw_ArgumentException_When_PatientDoesNotExist()
    {
        // Arrange
        var cnp = "9999999999999";
        var query = new GetPatientByCnpQuery(cnp);

        _repoMock.Setup(x => x.GetByCnpAsync(cnp, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Patient with CNP {cnp} was not found.");
    }
}