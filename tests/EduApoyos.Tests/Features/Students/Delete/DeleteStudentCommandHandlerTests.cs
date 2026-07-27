using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Application.Features.Students.Delete;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduApoyos.Tests.Features.Students.Delete;

public sealed class DeleteStudentCommandHandlerTests
{
    private readonly Mock<IStudentRepository> _studentRepository = new(MockBehavior.Strict);

    private DeleteStudentCommandHandler CreateSut() => new(_studentRepository.Object);

    private static Student BuildStudent() =>
        new(
            Guid.NewGuid(),
            "1234567890",
            DocumentType.NationalId,
            "Ingeniería de Software",
            4);

    [Fact]
    public async Task Handle_Should_Delete_Student_When_All_Rules_Are_Satisfied()
    {
        var student = BuildStudent();
        var command = new DeleteStudentCommand(student.Id);

        _studentRepository
            .Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _studentRepository
            .Setup(r => r.HasSupportRequestsAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.DeleteAsync(student, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _studentRepository.VerifyAll();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Student_Does_Not_Exist()
    {
        var command = new DeleteStudentCommand(Guid.NewGuid());

        _studentRepository
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("students.notFound");

        _studentRepository.Verify(
            r => r.HasSupportRequestsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _studentRepository.Verify(
            r => r.DeleteAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Student_Has_Support_Requests()
    {
        var student = BuildStudent();
        var command = new DeleteStudentCommand(student.Id);

        _studentRepository
            .Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _studentRepository
            .Setup(r => r.HasSupportRequestsAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("students.delete.hasSupportRequests");

        _studentRepository.Verify(
            r => r.DeleteAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Propagate_Cancellation_Token()
    {
        var student = BuildStudent();
        var command = new DeleteStudentCommand(student.Id);
        using var cts = new CancellationTokenSource();

        _studentRepository
            .Setup(r => r.GetByIdAsync(student.Id, cts.Token))
            .ReturnsAsync(student);

        _studentRepository
            .Setup(r => r.HasSupportRequestsAsync(student.Id, cts.Token))
            .ReturnsAsync(false);

        _studentRepository
            .Setup(r => r.DeleteAsync(student, cts.Token))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(command, cts.Token);

        result.IsSuccess.Should().BeTrue();
        _studentRepository.VerifyAll();
    }
}
