using FluentAssertions;
using MedicalApp.Application.Features.Patients.Queries.GetAllPatients;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace MedicalApp.Tests.Features.Patients.Queries.GetAllPatients;

public class GetAllPatientsQueryHandlerTests
{
    private readonly Mock<IPatientRepository> _repoMock = new();
    private readonly GetAllPatientsQueryHandler _handler;

    public GetAllPatientsQueryHandlerTests()
    {
        _handler = new GetAllPatientsQueryHandler(_repoMock.Object);
    }
    
    [Fact]
    public async Task Handle_Should_Return_AllPatients_MappedToDtos()
    {
        // Arrange
        var query = new GetAllPatientsQuery();

        var patients = new List<Patient>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "Ion", 
                LastName = "Popescu", 
                PersonalNumericCode = "1900101123456",
                UserId = "user-1"
            },
            new() 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "Maria", 
                LastName = "Ionescu", 
                PersonalNumericCode = "2900101123456",
                UserId = "user-2"
            }
        };

        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);


        result[0].FullName.Should().Be("Ion Popescu");
        result[0].PersonalNumericCode.Should().Be("1900101123456");

        result[1].FullName.Should().Be("Maria Ionescu");
    }

    [Fact]
    public async Task Handle_Should_Return_EmptyList_When_NoPatientsExist()
    {
        // Arrange
        var query = new GetAllPatientsQuery();

        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Patient>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        result.Should().NotBeNull();
    }
}