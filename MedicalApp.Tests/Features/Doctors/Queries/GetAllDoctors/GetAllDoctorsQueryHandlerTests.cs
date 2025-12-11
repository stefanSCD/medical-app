using FluentAssertions;
using MedicalApp.Application.Features.Doctors.Queries.GetAllDoctors;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Doctors.Queries.GetAllDoctors;

public class GetAllDoctorsQueryHandlerTests
{
    private readonly Mock<IDoctorRepository> _repoMock = new();
    private readonly GetAllDoctorsQueryHandler _handler;

    public GetAllDoctorsQueryHandlerTests()
    {
        _handler = new GetAllDoctorsQueryHandler(_repoMock.Object);
    }
    
    [Fact]
    public async Task Handle_Should_Return_AllDoctors_MappedToDtos()
    {
        // Arrange
        var query = new GetAllDoctorsQuery();

        var doctors = new List<Doctor>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "Gregory", 
                LastName = "House", 
                Specialization = "Diagnostic",
                UserId = "user-1",
            },
            new() 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "James", 
                LastName = "Wilson", 
                Specialization = "Oncology",
                UserId = "user-2",
            }
        };

        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctors);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        
        result[0].FullName.Should().Be("Gregory House");
        result[0].Specialization.Should().Be("Diagnostic");

        result[1].FullName.Should().Be("James Wilson");
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoDoctorsExist()
    {
        // Arrange
        var query = new GetAllDoctorsQuery();

        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Doctor>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        result.Should().NotBeNull();
    }
}