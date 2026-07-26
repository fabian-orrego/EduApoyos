using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Results;
using EduApoyos.Application.Features.Auth.Login;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.Auth.Login;

public sealed class LoginCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityService = new(MockBehavior.Strict);
    private readonly Mock<IJwtTokenGenerator> _tokenGenerator = new(MockBehavior.Strict);

    private LoginCommandHandler CreateSut() =>
        new(_identityService.Object, _tokenGenerator.Object);

    private static UserSummary BuildUser(
        UserRole role = UserRole.Student,
        string fullName = "Juan Pérez",
        string email = "juan.perez@example.com") =>
            new(Guid.NewGuid(), email, fullName, role, DateTime.UtcNow);

    [Fact]
    public async Task Handle_Should_Return_Token_When_Credentials_Are_Valid()
    {
        var command = new LoginCommand("juan.perez@example.com", "Password123");
        var user = BuildUser();
        var accessToken = new AccessToken("signed.jwt.token", DateTime.UtcNow.AddMinutes(60));

        _identityService
            .Setup(s => s.ValidateCredentialsAsync(
                command.Email,
                command.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _tokenGenerator
            .Setup(g => g.Generate(user))
            .Returns(accessToken);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new LoginResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            user.FullName,
            (int)user.Role));

        _identityService.VerifyAll();
        _tokenGenerator.VerifyAll();
    }

    [Fact]
    public async Task Handle_Should_Trim_Email_Before_Delegating_To_Identity_Service()
    {
        var command = new LoginCommand("  juan.perez@example.com  ", "Password123");
        var user = BuildUser();
        var accessToken = new AccessToken("t", DateTime.UtcNow.AddMinutes(60));

        _identityService
            .Setup(s => s.ValidateCredentialsAsync(
                "juan.perez@example.com",
                command.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _tokenGenerator
            .Setup(g => g.Generate(user))
            .Returns(accessToken);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _identityService.VerifyAll();
        _tokenGenerator.VerifyAll();
    }

    [Fact]
    public async Task Handle_Should_Return_Unauthorized_When_Credentials_Are_Invalid()
    {
        var command = new LoginCommand("juan.perez@example.com", "wrong");
        var unauthorized = Error.Unauthorized(
            "auth.credentials.invalid",
            "Credenciales inválidas.");

        _identityService
            .Setup(s => s.ValidateCredentialsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserSummary>(unauthorized));

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(unauthorized);
        result.Error.Type.Should().Be(ErrorType.Unauthorized);

        // The token generator must never be reached when validation fails (RN-004).
        _tokenGenerator.Verify(g => g.Generate(It.IsAny<UserSummary>()), Times.Never);
    }

    [Theory]
    [InlineData(UserRole.Advisor)]
    [InlineData(UserRole.Student)]
    public async Task Handle_Should_Surface_RoleId_In_The_Response(UserRole role)
    {
        var command = new LoginCommand("user@example.com", "Password123");
        var user = BuildUser(role: role);
        var accessToken = new AccessToken("t", DateTime.UtcNow.AddMinutes(60));

        _identityService
            .Setup(s => s.ValidateCredentialsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        _tokenGenerator
            .Setup(g => g.Generate(user))
            .Returns(accessToken);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RoleId.Should().Be((int)role);
    }

    [Fact]
    public async Task Handle_Should_Propagate_Cancellation_Token()
    {
        var command = new LoginCommand("user@example.com", "Password123");
        using var cts = new CancellationTokenSource();
        var user = BuildUser();
        var accessToken = new AccessToken("t", DateTime.UtcNow.AddMinutes(60));

        _identityService
            .Setup(s => s.ValidateCredentialsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                cts.Token))
            .ReturnsAsync(Result.Success(user));

        _tokenGenerator
            .Setup(g => g.Generate(user))
            .Returns(accessToken);

        var result = await CreateSut().Handle(command, cts.Token);

        result.IsSuccess.Should().BeTrue();
        _identityService.VerifyAll();
    }
}
