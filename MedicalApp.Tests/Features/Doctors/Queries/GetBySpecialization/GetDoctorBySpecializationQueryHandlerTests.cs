using FluentAssertions;
using MedicalApp.Application.Features.Doctors.Queries.GetBySpecialization;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Moq;
using Xunit;
using System.Threading.Tasks;
namespace MedicalApp.Tests.Features.Doctors.Queries.GetBySpecialization;

public class GetDoctorBySpecializationQueryHandlerTests
{
    private readonly Mock<IDoctorRepository> _repoMock = new();
    private readonly GetDoctorBySpecializationQueryHandler _handler;

    public GetDoctorBySpecializationQueryHandlerTests()
    {
        _handler = new GetDoctorBySpecializationQueryHandler(_repoMock.Object);
    }
    
    [Fact]
    public async Task Handle_Should_Return_Doctors_When_SpecializationMatches()
    {
        // Arrange
        string specialization = "Cardiology";
        var query = new GetDoctorBySpecializationQuery(specialization);

        var doctors = new List<Doctor>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "Gregory", 
                LastName = "House", 
                Specialization = specialization,
                UserId = "user-1" 
            },
            new() 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "Lisa", 
                LastName = "Cuddy", 
                Specialization = specialization,
                UserId = "user-2"
            }
        };

        _repoMock.Setup(x => x.GetBySpecializationAsync(specialization, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctors);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        result[0].FullName.Should().Be("Gregory House");
        result[0].Specialization.Should().Be(specialization);
        result[1].FullName.Should().Be("Lisa Cuddy");
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoDoctorsFound()
    {
        // Arrange
        string specialization = "Dermatology";
        var query = new GetDoctorBySpecializationQuery(specialization);

        _repoMock.Setup(x => x.GetBySpecializationAsync(specialization, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Doctor>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        result.Should().NotBeNull();
    }
}