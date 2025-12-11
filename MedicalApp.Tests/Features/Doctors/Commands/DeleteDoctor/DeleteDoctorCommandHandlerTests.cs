using FluentAssertions;
using MedicalApp.Application.Features.Doctors.Commands.DeleteDoctor;
using MedicalApp.Application.Interfaces;
using MedicalApp.Domain.Entities;
using MedicalApp.Domain.Enums;
using MedicalApp.Tests.Mocks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using System.Threading.Tasks;
namespace MedicalApp.Tests.Features.Doctors.Commands.DeleteDoctor;

public class DeleteDoctorCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepoMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ICurrentUserService> _userServiceMock = new();

    private readonly DeleteDoctorCommandHandler _handler;

    public DeleteDoctorCommandHandlerTests()
    {
        _userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();

        _handler = new(
            _doctorRepoMock.Object,
            _appointmentRepoMock.Object,
            _userManagerMock.Object,
            _userServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_DeleteDoctor_Appointments_And_User_When_Admin()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        string userId = "user-guid-123";
        
        var command = new DeleteDoctorCommand(doctorId); 

        var doctor = new Doctor 
        { 
            Id = doctorId, 
            UserId = userId, 
            FirstName = "Gregory", 
            LastName = "House",
            Specialization = "Diagnostic",
        };

        var appointments = new List<Appointment>
        {
            new() { 
                Id = Guid.NewGuid(), 
                DoctorId = doctorId, 
                PatientId = Guid.NewGuid(), 
                StartDate = DateTime.Now,
                EndDate =  DateTime.Now.AddHours(1),
                Status = AppointmentStatus.Scheduled
            },
            new() { 
                Id = Guid.NewGuid(), 
                DoctorId = doctorId, 
                PatientId = Guid.NewGuid(), 
                StartDate = DateTime.Now.AddHours(3),
                EndDate = DateTime.Now.AddHours(4),
                Status = AppointmentStatus.Scheduled
            }
        };

        var appUser = new ApplicationUser 
        { 
            Id = userId, 
            Email = "house@test.com", 
            UserName = "house@test.com" 
        };

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);

        _doctorRepoMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _appointmentRepoMock.Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(appUser);
        
        _userManagerMock.Setup(x => x.DeleteAsync(appUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        
        _appointmentRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

        _userManagerMock.Verify(x => x.DeleteAsync(appUser), Times.Once);

        _doctorRepoMock.Verify(x => x.DeleteAsync(doctorId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_Should_Throw_Unauthorized_When_UserIsNotAdmin()
    {
        // Arrange
        var command = new DeleteDoctorCommand(Guid.NewGuid());
        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(false);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*authorized*");

        _doctorRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_Should_Throw_KeyNotFound_When_DoctorDoesNotExist()
    {
        // Arrange
        var command = new DeleteDoctorCommand(Guid.NewGuid());
        
        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);
        
        _doctorRepoMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Doctor with ID {command.Id} not found.");
    }
    
    [Fact]
    public async Task Handle_Should_Throw_InvalidOperation_When_IdentityDeleteFails()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        string userId = "user-fail";
        var command = new DeleteDoctorCommand(doctorId);

        var doctor = new Doctor 
        { 
            Id = doctorId, 
            UserId = userId,
            FirstName = "Gregory",
            LastName = "House",
            Specialization = "Diagnostic"
        };
        
        var appUser = new ApplicationUser { Id = userId };

        _userServiceMock.Setup(x => x.IsInRole("Admin")).Returns(true);
        _doctorRepoMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
        _appointmentRepoMock.Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Appointment>());
        
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(appUser);
        
        var identityError = IdentityResult.Failed(new IdentityError { Description = "Database locked" });
        _userManagerMock.Setup(x => x.DeleteAsync(appUser)).ReturnsAsync(identityError);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to delete user account*");

        _doctorRepoMock.Verify(x => x.DeleteAsync(doctorId, It.IsAny<CancellationToken>()), Times.Never);
    }
}