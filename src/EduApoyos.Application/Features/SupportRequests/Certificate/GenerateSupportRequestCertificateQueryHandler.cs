using EduApoyos.Application.Common.Documents;
using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.Certificate;

/// <summary>
/// Handles <see cref="GenerateSupportRequestCertificateQuery"/> (US-018). The handler enforces
/// ownership (only the student the request belongs to may download it) and delegates the
/// actual PDF rendering to <see cref="ISupportRequestPdfGenerator"/>.
/// </summary>
public sealed class GenerateSupportRequestCertificateQueryHandler
    : IRequestHandler<GenerateSupportRequestCertificateQuery, Result<SupportRequestCertificate>>
{
    private readonly ISupportRequestRepository _supportRequestRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ISupportRequestPdfGenerator _pdfGenerator;

    public GenerateSupportRequestCertificateQueryHandler(
        ISupportRequestRepository supportRequestRepository,
        IStudentRepository studentRepository,
        ICurrentUserService currentUser,
        ISupportRequestPdfGenerator pdfGenerator)
    {
        _supportRequestRepository = supportRequestRepository;
        _studentRepository = studentRepository;
        _currentUser = currentUser;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<Result<SupportRequestCertificate>> Handle(
        GenerateSupportRequestCertificateQuery request,
        CancellationToken cancellationToken)
    {
        var detail = await _supportRequestRepository
            .GetDetailAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (detail is null)
        {
            return Result.Failure<SupportRequestCertificate>(
                Error.NotFound(
                    "supportRequests.notFound",
                    "La solicitud no existe."));
        }

        // Students may only download their own certificates (RN-1). Advisors are not the
        // intended audience for this document (the story is scoped to Student) so they are
        // rejected with the same 403 as any non-owner.
        if (_currentUser.Role != UserRole.Student)
        {
            return Result.Failure<SupportRequestCertificate>(
                Error.Forbidden(
                    "supportRequests.certificate.forbidden",
                    "Solo el estudiante propietario puede descargar la constancia."));
        }

        var callerStudentId = _currentUser.UserId is Guid userId
            ? await _studentRepository
                .GetIdByUserIdAsync(userId, cancellationToken)
                .ConfigureAwait(false)
            : null;

        if (callerStudentId is null || callerStudentId.Value != detail.StudentId)
        {
            return Result.Failure<SupportRequestCertificate>(
                Error.Forbidden(
                    "supportRequests.certificate.forbidden",
                    "Solo el estudiante propietario puede descargar la constancia."));
        }

        var issuedAt = DateTime.UtcNow;
        var bytes = _pdfGenerator.Generate(detail, issuedAt);

        // A short unique stamp keeps every download filename distinct so browsers/OS never
        // reuse a previously cached PDF for the same request id.
        var stamp = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"constancia-solicitud-{detail.Id:N}-{stamp}.pdf";

        return Result.Success(
            new SupportRequestCertificate(bytes, fileName, "application/pdf"));
    }
}
