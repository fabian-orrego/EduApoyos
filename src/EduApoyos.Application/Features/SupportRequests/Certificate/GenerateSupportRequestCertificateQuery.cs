using EduApoyos.Application.Common.Results;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.Certificate;

/// <summary>
/// Generates the PDF constancia (proof of application) for a support request (US-018). The
/// document is produced on demand and only the owning student may download it (RN-1).
/// </summary>
public sealed record GenerateSupportRequestCertificateQuery(Guid Id)
    : IRequest<Result<SupportRequestCertificate>>;
