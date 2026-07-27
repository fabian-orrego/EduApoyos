namespace EduApoyos.Application.Features.SupportRequests.Certificate;

/// <summary>
/// Envelope returned by <see cref="GenerateSupportRequestCertificateQuery"/> (US-018). Carries
/// the freshly rendered PDF bytes together with a suggested file name so the API layer only
/// has to forward the payload.
/// </summary>
public sealed record SupportRequestCertificate(
    byte[] Content,
    string FileName,
    string ContentType);
