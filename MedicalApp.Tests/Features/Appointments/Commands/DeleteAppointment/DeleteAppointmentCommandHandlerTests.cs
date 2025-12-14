using FluentAssertions;
using MedicalApp.Application.Features.Appointments.Commands.DeleteAppointment;
using MedicalApp.Application.Interfaces;
using Moq;
using Xunit;

namespace MedicalApp.Tests.Features.Appointments.Commands.DeleteAppointment;

public class DeleteAppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _repoMock = new();
    private readonly Mock<IAppointmentSecurityService> _securityMock = new();
    private readonly DeleteAppointmentCommandHandler _handler;

    public DeleteAppointmentCommandHandlerTests()
    {
        _handler = new DeleteAppointmentCommandHandler(_repoMock.Object, _securityMock.Object);
    }
    
    [Fact]
    public async Task Handle_Should_Delete_When_SecurityService_Allows()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var command = new DeleteAppointmentCommand(appointmentId);

        _securityMock.Setup(x => x.CanModifyAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repoMock.Verify(x => x.DeleteAsync(appointmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Unauthorized_When_SecurityService_Denies()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var command = new DeleteAppointmentCommand(appointmentId);

        _securityMock.Setup(x => x.CanModifyAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Not authorized to delete this appointment.");

        _repoMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}