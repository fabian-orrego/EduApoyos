using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduApoyos.Application.Common.Identity;
using EduApoyos.Domain.Enums;
using EduApoyos.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EduApoyos.Tests.Infrastructure.Authentication;

public sealed class JwtTokenGeneratorTests
{
    private const string SecretKey = "K3v9lNv1yEo2sRaZ6t1uWyJhFqCzX0LmBpDgTnQeVsAkYbHfCiRoUxMjPn7wZ4Sq";
    private const string Issuer = "EduApoyos.Api";
    private const string Audience = "EduApoyos.Client";
    private const int ExpirationMinutes = 45;

    private static JwtTokenGenerator CreateSut(JwtSettings? settings = null)
    {
        var effective = settings ?? new JwtSettings
        {
            Issuer = Issuer,
            Audience = Audience,
            SecretKey = SecretKey,
            AccessTokenExpirationMinutes = ExpirationMinutes,
        };

        return new JwtTokenGenerator(Options.Create(effective));
    }

    private static UserSummary BuildUser(
        UserRole role = UserRole.Advisor,
        string fullName = "María Gómez",
        string email = "maria.gomez@example.com") =>
            new(Guid.NewGuid(), email, fullName, role, DateTime.UtcNow);

    [Fact]
    public void Generate_Should_Produce_Token_With_Expected_Expiration()
    {
        var sut = CreateSut();
        var user = BuildUser();
        var before = DateTime.UtcNow;

        var result = sut.Generate(user);

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAtUtc.Should().BeAfter(before.AddMinutes(ExpirationMinutes - 1));
        result.ExpiresAtUtc.Should().BeBefore(before.AddMinutes(ExpirationMinutes + 1));
    }

    [Fact]
    public void Generate_Should_Embed_UserId_Email_And_Role_Claims()
    {
        var sut = CreateSut();
        var user = BuildUser(role: UserRole.Advisor);

        var result = sut.Generate(user);
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(result.Token).Claims.ToList();

        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Advisor");
        claims.Should().Contain(c => c.Type == "roleId" && c.Value == "1");
    }

    [Fact]
    public void Generate_Should_Emit_Different_Jti_For_Each_Token()
    {
        var sut = CreateSut();
        var user = BuildUser();

        var first = sut.Generate(user);
        var second = sut.Generate(user);

        first.Token.Should().NotBe(second.Token);
    }

    [Fact]
    public void Generate_Should_Produce_Token_That_Passes_The_Same_Signature_Validation()
    {
        var sut = CreateSut();
        var user = BuildUser();

        var result = sut.Generate(user);
        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Issuer,
            ValidAudience = Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
            ClockSkew = TimeSpan.Zero,
        };

        var principal = handler.ValidateToken(result.Token, parameters, out var validated);

        principal.Should().NotBeNull();
        validated.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Secret_Key_Is_Missing()
    {
        var settings = new JwtSettings
        {
            Issuer = Issuer,
            Audience = Audience,
            SecretKey = "",
            AccessTokenExpirationMinutes = ExpirationMinutes,
        };

        var act = () => new JwtTokenGenerator(Options.Create(settings));

        act.Should().Throw<InvalidOperationException>();
    }
}
