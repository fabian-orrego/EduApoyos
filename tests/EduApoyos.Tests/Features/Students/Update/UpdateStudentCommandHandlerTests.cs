using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Application.Features.Students.Update;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.Students.Update;

public sealed class UpdateStudentCommandHandlerTests
{
    private readonly Mock<IStudentRepository> _studentRepository = new(MockBehavior.Strict);

    private UpdateStudentCommandHandler CreateSut() => new(_studentRepository.Object);

    private static Student BuildStudent(
        Guid? userId = null,
        string documentNumber = "1234567890",
        DocumentType documentType = DocumentType.NationalId,
        string academicProgram = "Ingeniería Industrial",
        int semester = 3) =>
            new(
                userId ?? Guid.NewGuid(),
                documentNumber,
                documentType,
                academicProgram,
                semester);

    private static UpdateStudentCommand BuildCommand(
        Guid id,
        string documentNumber = "9876543210",
        DocumentType documentType = DocumentType.Passport,
        string academicProgram = "Contaduría Pública",
        int semester = 5) =>
            new(id, documentNumber, documentType, academicProgram, semester);

    [Fact]
    public async Task Handle_Should_Update_Student_And_Return_Response_On_Success()
    {
        var userId = Guid.NewGuid();
        var student = BuildStudent(userId: userId);
        var command = BuildCommand(student.Id);

        _studentRepository
            .Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _studentRepository
            .Setup(r => r.ExistsByDocumentAsync(
                command.DocumentType,
                command.DocumentNumber,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.UpdateAsync(student, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(student.Id);
        result.Value.UserId.Should().Be(userId);
        result.Value.DocumentNumber.Should().Be(command.DocumentNumber);
        result.Value.DocumentType.Should().Be((int)command.DocumentType);
        result.Value.AcademicProgram.Should().Be(command.AcademicProgram);
        result.Value.Semester.Should().Be(command.Semester);

        student.DocumentNumber.Should().Be(command.DocumentNumber);
        student.DocumentType.Should().Be(command.DocumentType);
        student.AcademicProgram.Should().Be(command.AcademicProgram);
        student.Semester.Should().Be(command.Semester);
        student.UserId.Should().Be(userId);

        _studentRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_Should_Trim_Whitespace_From_Text_Fields()
    {
        var student = BuildStudent();
        var command = BuildCommand(
            student.Id,
            documentNumber: "  9876543210  ",
            academicProgram: "  Contaduría Pública  ");

        _studentRepository
            .Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _studentRepository
            .Setup(r => r.ExistsByDocumentAsync(
                command.DocumentType,
                "9876543210",
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.UpdateAsync(student, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        student.DocumentNumber.Should().Be("9876543210");
        student.AcademicProgram.Should().Be("Contaduría Pública");

        _studentRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Student_Does_Not_Exist()
    {
        var studentId = Guid.NewGuid();
        var command = BuildCommand(studentId);

        _studentRepository
            .Setup(r => r.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("students.notFound");

        _studentRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Document_Is_Already_Registered()
    {
        var student = BuildStudent();
        var command = BuildCommand(student.Id);

        _studentRepository
            .Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _studentRepository
            .Setup(r => r.ExistsByDocumentAsync(
                command.DocumentType,
                command.DocumentNumber,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("students.document.duplicated");

        _studentRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Skip_Document_Uniqueness_Check_When_Document_Did_Not_Change()
    {
        var student = BuildStudent(
            documentNumber: "1234567890",
            documentType: DocumentType.NationalId);
        var command = new UpdateStudentCommand(
            student.Id,
            "1234567890",
            DocumentType.NationalId,
            "Otro programa",
            7);

        _studentRepository
            .Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _studentRepository
            .Setup(r => r.UpdateAsync(student, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _studentRepository.Verify(
            r => r.ExistsByDocumentAsync(
                It.IsAny<DocumentType>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Not_Modify_UserId()
    {
        var originalUserId = Guid.NewGuid();
        var student = BuildStudent(userId: originalUserId);
        var command = BuildCommand(student.Id);

        _studentRepository
            .Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _studentRepository
            .Setup(r => r.ExistsByDocumentAsync(
                command.DocumentType,
                command.DocumentNumber,
                student.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.UpdateAsync(student, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        student.UserId.Should().Be(originalUserId);
        result.Value.UserId.Should().Be(originalUserId);
    }

    [Fact]
    public async Task Handle_Should_Propagate_Cancellation_Token()
    {
        var student = BuildStudent();
        var command = BuildCommand(student.Id);
        using var cts = new CancellationTokenSource();

        _studentRepository
            .Setup(r => r.GetByIdAsync(student.Id, cts.Token))
            .ReturnsAsync(student);

        _studentRepository
            .Setup(r => r.ExistsByDocumentAsync(
                command.DocumentType,
                command.DocumentNumber,
                student.Id,
                cts.Token))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.UpdateAsync(student, cts.Token))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, cts.Token);

        result.IsSuccess.Should().BeTrue();
        _studentRepository.VerifyAll();
    }
}
