using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Results;
using EduApoyos.Application.Features.Auth.Register;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.Auth.Register;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityService = new(MockBehavior.Strict);

    private RegisterUserCommandHandler CreateSut() => new(_identityService.Object);

    private static RegisterUserCommand BuildCommand(
        string fullName = "Juan Pérez",
        string email = "juan.perez@example.com",
        string password = "Password123",
        UserRole role = UserRole.Student) =>
            new(fullName, email, password, password, role);

    [Fact]
    public async Task Handle_Should_Return_Response_When_Identity_Succeeds()
    {
        var command = BuildCommand();
        var registeredAt = DateTime.UtcNow;
        var userId = Guid.NewGuid();

        _identityService
            .Setup(s => s.CreateUserAsync(
                command.FullName,
                command.Email,
                command.Password,
                command.Role,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserSummary(
                userId,
                command.Email,
                command.FullName,
                command.Role,
                registeredAt)));

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new RegisterUserResponse(
            userId,
            command.Email,
            command.FullName,
            (int)command.Role,
            registeredAt));

        _identityService.VerifyAll();
    }

    [Fact]
    public async Task Handle_Should_Trim_FullName_And_Email_Before_Delegating()
    {
        var command = BuildCommand(fullName: "  Juan Pérez  ", email: "  juan.perez@example.com  ");

        _identityService
            .Setup(s => s.CreateUserAsync(
                "Juan Pérez",
                "juan.perez@example.com",
                command.Password,
                command.Role,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UserSummary(
                Guid.NewGuid(),
                "juan.perez@example.com",
                "Juan Pérez",
                command.Role,
                DateTime.UtcNow)));

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _identityService.VerifyAll();
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Identity_Reports_Duplicated_Email()
    {
        var command = BuildCommand();
        var conflict = Error.Conflict(
            "auth.email.duplicated",
            "El correo electrónico ya se encuentra registrado.");

        _identityService
            .Setup(s => s.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<UserRole>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserSummary>(conflict));

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(conflict);
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_Should_Propagate_Cancellation_Token()
    {
        var command = BuildCommand();
        using var cts = new CancellationTokenSource();

        _identityService
            .Setup(s => s.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<UserRole>(),
                cts.Token))
            .ReturnsAsync(Result.Success(new UserSummary(
                Guid.NewGuid(),
                command.Email,
                command.FullName,
                command.Role,
                DateTime.UtcNow)));

        var result = await CreateSut().Handle(command, cts.Token);

        result.IsSuccess.Should().BeTrue();
        _identityService.VerifyAll();
    }
}
