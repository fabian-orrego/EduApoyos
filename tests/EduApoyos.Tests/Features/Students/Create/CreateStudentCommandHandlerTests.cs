using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Application.Features.Students.Create;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.Students.Create;

public sealed class CreateStudentCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityService = new(MockBehavior.Strict);
    private readonly Mock<IStudentRepository> _studentRepository = new(MockBehavior.Strict);

    private CreateStudentCommandHandler CreateSut() =>
        new(_identityService.Object, _studentRepository.Object);

    private static CreateStudentCommand BuildCommand(
        string email = "juan.perez@example.com",
        string documentNumber = "1234567890",
        DocumentType documentType = DocumentType.NationalId,
        string academicProgram = "Ingeniería de Software",
        int semester = 4) =>
            new(email, documentNumber, documentType, academicProgram, semester);

    private static UserSummary BuildUser(
        Guid? id = null,
        UserRole role = UserRole.Student,
        string email = "juan.perez@example.com",
        string fullName = "Juan Pérez") =>
            new(id ?? Guid.NewGuid(), email, fullName, role, DateTime.UtcNow);

    [Fact]
    public async Task Handle_Should_Create_Student_When_All_Rules_Are_Satisfied()
    {
        var command = BuildCommand();
        var user = BuildUser();

        _identityService
            .Setup(s => s.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _studentRepository
            .Setup(r => r.ExistsByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.ExistsByDocumentAsync(
                command.DocumentType,
                command.DocumentNumber,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Student? persisted = null;
        _studentRepository
            .Setup(r => r.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .Callback<Student, CancellationToken>((s, _) => persisted = s)
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        persisted.Should().NotBeNull();
        persisted!.UserId.Should().Be(user.Id);
        persisted.DocumentNumber.Should().Be(command.DocumentNumber);
        persisted.DocumentType.Should().Be(command.DocumentType);
        persisted.AcademicProgram.Should().Be(command.AcademicProgram);
        persisted.Semester.Should().Be(command.Semester);

        result.Value.Should().BeEquivalentTo(new CreateStudentResponse(
            persisted.Id,
            user.Id,
            command.DocumentNumber,
            (int)command.DocumentType,
            command.AcademicProgram,
            command.Semester));

        _identityService.VerifyAll();
        _studentRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_Should_Trim_Whitespace_From_Text_Fields()
    {
        var command = BuildCommand(
            email: "  juan.perez@example.com  ",
            documentNumber: "  1234567890  ",
            academicProgram: "  Ingeniería de Software  ");

        var user = BuildUser();

        _identityService
            .Setup(s => s.FindByEmailAsync("juan.perez@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _studentRepository
            .Setup(r => r.ExistsByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.ExistsByDocumentAsync(
                command.DocumentType,
                "1234567890",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Student? persisted = null;
        _studentRepository
            .Setup(r => r.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .Callback<Student, CancellationToken>((s, _) => persisted = s)
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        persisted!.DocumentNumber.Should().Be("1234567890");
        persisted.AcademicProgram.Should().Be("Ingeniería de Software");

        _identityService.VerifyAll();
        _studentRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_Should_Return_Validation_Error_When_User_Not_Found()
    {
        var command = BuildCommand();

        _identityService
            .Setup(s => s.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSummary?)null);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("students.user.notFound");

        _studentRepository.Verify(
            r => r.ExistsByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _studentRepository.Verify(
            r => r.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Validation_Error_When_User_Is_Not_A_Student()
    {
        var command = BuildCommand();
        var user = BuildUser(role: UserRole.Advisor);

        _identityService
            .Setup(s => s.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("students.user.invalidRole");

        _studentRepository.Verify(
            r => r.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_User_Already_Linked_To_A_Student()
    {
        var command = BuildCommand();
        var user = BuildUser();

        _identityService
            .Setup(s => s.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _studentRepository
            .Setup(r => r.ExistsByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("students.user.alreadyLinked");

        _studentRepository.Verify(
            r => r.ExistsByDocumentAsync(
                It.IsAny<DocumentType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _studentRepository.Verify(
            r => r.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Document_Is_Duplicated()
    {
        var command = BuildCommand();
        var user = BuildUser();

        _identityService
            .Setup(s => s.FindByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _studentRepository
            .Setup(r => r.ExistsByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.ExistsByDocumentAsync(
                command.DocumentType,
                command.DocumentNumber,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("students.document.duplicated");

        _studentRepository.Verify(
            r => r.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Propagate_Cancellation_Token()
    {
        var command = BuildCommand();
        var user = BuildUser();
        using var cts = new CancellationTokenSource();

        _identityService
            .Setup(s => s.FindByEmailAsync(command.Email, cts.Token))
            .ReturnsAsync(user);

        _studentRepository
            .Setup(r => r.ExistsByUserIdAsync(user.Id, cts.Token))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.ExistsByDocumentAsync(
                command.DocumentType,
                command.DocumentNumber,
                cts.Token))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.CreateAsync(It.IsAny<Student>(), cts.Token))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, cts.Token);

        result.IsSuccess.Should().BeTrue();
        _identityService.VerifyAll();
        _studentRepository.VerifyAll();
    }
}
