using FluentAssertions;
using MedicalApp.Application.Features.Patients.Queries.GetById;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Patients.Queries.GetById;

public class GetPatientByIdQueryHandlerTests
{
    private readonly Mock<IPatientRepository> _repoMock = new();
    private readonly GetPatientByIdQueryHandler _handler;

    public GetPatientByIdQueryHandlerTests()
    {
        _handler = new GetPatientByIdQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_PatientDto_When_PatientExists()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var query = new GetPatientByIdQuery(patientId);

        var patient = new Patient
        {
            Id = patientId,
            FirstName = "Ion",
            LastName = "Popescu",
            PersonalNumericCode = "1900101123456",
            UserId = "user-guid-1"
        };

        _repoMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.FullName.Should().Be("Ion Popescu");
        result.PersonalNumericCode.Should().Be("1900101123456");
        result.Id.Should().Be(patientId);
    }

    [Fact]
    public async Task Handle_Should_Throw_ArgumentException_When_PatientDoesNotExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var query = new GetPatientByIdQuery(patientId);

        _repoMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Patient with id {patientId} was not found.");
    }
}