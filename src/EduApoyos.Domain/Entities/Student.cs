using EduApoyos.Domain.Common;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Domain.Entities;

public class Student : Entity
{
    private Student()
    {
        DocumentNumber = string.Empty;
        AcademicProgram = string.Empty;
    }

    public Student(
        Guid userId,
        string documentNumber,
        DocumentType documentType,
        string academicProgram,
        int semester)
    {
        UserId = userId;
        DocumentNumber = documentNumber;
        DocumentType = documentType;
        AcademicProgram = academicProgram;
        Semester = semester;
    }

    public Guid UserId { get; private set; }

    public string DocumentNumber { get; private set; }

    public DocumentType DocumentType { get; private set; }

    public string AcademicProgram { get; private set; }

    public int Semester { get; private set; }

    /// <summary>
    /// Updates the mutable academic information of the student (US-009).
    /// The <see cref="UserId"/> is intentionally not exposed as a parameter because RN-003
    /// forbids reassigning the underlying Identity user.
    /// </summary>
    public void UpdateAcademicInfo(
        string documentNumber,
        DocumentType documentType,
        string academicProgram,
        int semester)
    {
        DocumentNumber = documentNumber;
        DocumentType = documentType;
        AcademicProgram = academicProgram;
        Semester = semester;
    }
}
