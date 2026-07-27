using EduApoyos.Application.Features.SupportRequests.Detail;

namespace EduApoyos.Application.Common.Documents;

/// <summary>
/// Renders a PDF constancia (proof-of-application) for a support request (US-018). The concrete
/// implementation lives in <c>EduApoyos.Infrastructure</c> so the Application layer stays
/// unaware of the PDF library.
/// </summary>
public interface ISupportRequestPdfGenerator
{
    /// <summary>
    /// Builds the PDF in memory and returns its raw bytes. The document is generated on demand
    /// and is not persisted anywhere (US-018 RN-2).
    /// </summary>
    byte[] Generate(SupportRequestDetail detail, DateTime issuedAt);
}
