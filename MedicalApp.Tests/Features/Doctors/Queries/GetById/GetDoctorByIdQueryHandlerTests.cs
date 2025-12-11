using FluentAssertions;
using MedicalApp.Application.Features.Doctors.Queries.GetById;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Doctors.Queries.GetById;

public class GetDoctorByIdQueryHandlerTests
{
    private readonly Mock<IDoctorRepository> _repoMock = new();
    private readonly GetDoctorByIdQueryHandler _handler;

    public GetDoctorByIdQueryHandlerTests()
    {
        _handler = new GetDoctorByIdQueryHandler(_repoMock.Object);
    }
    
    [Fact]
    public async Task Handle_Should_Return_DoctorDto_When_DoctorExists()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var query = new GetDoctorByIdQuery(doctorId);

        var doctor = new Doctor
        {
            Id = doctorId,
            FirstName = "Gregory",
            LastName = "House",
            Specialization = "Diagnostic",
            UserId = "user-guid",
        };

        _repoMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        result.FullName.Should().Be("Gregory House");
        result.Specialization.Should().Be("Diagnostic");
        result.Id.Should().Be(doctorId);
    }

    [Fact]
    public async Task Handle_Should_Throw_ArgumentException_When_DoctorDoesNotExist()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var query = new GetDoctorByIdQuery(doctorId);

        _repoMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Doctor with id {doctorId} does not exists");
    }
}