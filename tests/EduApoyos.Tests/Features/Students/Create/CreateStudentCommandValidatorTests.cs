using EduApoyos.Application.Features.Students.Create;
using EduApoyos.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EduApoyos.Tests.Features.Students.Create;

public sealed class CreateStudentCommandValidatorTests
{
    private readonly CreateStudentCommandValidator _validator = new();

    private static CreateStudentCommand ValidCommand(
        Action<CreateStudentCommandBuilder>? mutate = null)
    {
        var builder = new CreateStudentCommandBuilder();
        mutate?.Invoke(builder);
        return builder.Build();
    }

    [Fact]
    public void Should_Pass_When_Command_Is_Valid()
    {
        var command = ValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Fail_When_Email_Is_Empty(string? email)
    {
        var command = ValidCommand(b => b.Email = email!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@missing-user.com")]
    public void Should_Fail_When_Email_Has_Invalid_Format(string email)
    {
        var command = ValidCommand(b => b.Email = email);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Fail_When_DocumentNumber_Is_Empty(string? documentNumber)
    {
        var command = ValidCommand(b => b.DocumentNumber = documentNumber!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.DocumentNumber);
    }

    [Fact]
    public void Should_Fail_When_DocumentNumber_Exceeds_20_Characters()
    {
        var command = ValidCommand(b => b.DocumentNumber = new string('1', 21));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.DocumentNumber);
    }

    [Fact]
    public void Should_Fail_When_DocumentType_Is_Not_A_Valid_Enum_Value()
    {
        var command = ValidCommand(b => b.DocumentType = (DocumentType)999);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.DocumentType);
    }

    [Theory]
    [InlineData(DocumentType.NationalId)]
    [InlineData(DocumentType.ForeignerId)]
    [InlineData(DocumentType.Passport)]
    public void Should_Pass_When_DocumentType_Is_A_Valid_Enum_Value(DocumentType documentType)
    {
        var command = ValidCommand(b => b.DocumentType = documentType);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.DocumentType);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Fail_When_AcademicProgram_Is_Empty(string? academicProgram)
    {
        var command = ValidCommand(b => b.AcademicProgram = academicProgram!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.AcademicProgram);
    }

    [Fact]
    public void Should_Fail_When_AcademicProgram_Exceeds_150_Characters()
    {
        var command = ValidCommand(b => b.AcademicProgram = new string('a', 151));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.AcademicProgram);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(13)]
    [InlineData(20)]
    public void Should_Fail_When_Semester_Is_Out_Of_Range(int semester)
    {
        var command = ValidCommand(b => b.Semester = semester);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Semester);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Should_Pass_When_Semester_Is_Within_Range(int semester)
    {
        var command = ValidCommand(b => b.Semester = semester);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Semester);
        result.IsValid.Should().BeTrue();
    }

    private sealed class CreateStudentCommandBuilder
    {
        public string Email { get; set; } = "juan.perez@example.com";
        public string DocumentNumber { get; set; } = "1234567890";
        public DocumentType DocumentType { get; set; } = DocumentType.NationalId;
        public string AcademicProgram { get; set; } = "Ingeniería de Software";
        public int Semester { get; set; } = 4;

        public CreateStudentCommand Build() =>
            new(Email, DocumentNumber, DocumentType, AcademicProgram, Semester);
    }
}
