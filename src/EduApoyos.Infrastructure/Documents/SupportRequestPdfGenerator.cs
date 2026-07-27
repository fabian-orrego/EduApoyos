using System.Globalization;
using EduApoyos.Application.Common.Documents;
using EduApoyos.Application.Features.SupportRequests.Detail;
using EduApoyos.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduApoyos.Infrastructure.Documents;

/// <summary>
/// QuestPDF backed implementation of <see cref="ISupportRequestPdfGenerator"/>. The generator
/// composes an A4 constancia with the student information, the request payload, the current
/// status and the chronological status-history comments. The document is rendered in-memory
/// on every call so no PDF is stored on disk (US-018 RN-2).
/// </summary>
internal sealed class SupportRequestPdfGenerator : ISupportRequestPdfGenerator
{
    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-CO");

    /// <summary>
    /// Calibri is typically available on Windows developer machines; Linux containers ship
    /// DejaVu (installed in the API Dockerfile) so PDF generation keeps working in Docker.
    /// </summary>
    private static string ResolveFontFamily() =>
        OperatingSystem.IsWindows() ? Fonts.Calibri : "DejaVu Sans";

    public byte[] Generate(SupportRequestDetail detail, DateTime issuedAt)
    {
        // The community license only needs to be configured once per process. Setting it here
        // keeps the constraint local to the PDF module and does not require any startup wiring.
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(ResolveFontFamily()));

                page.Header().Element(BuildHeader);
                page.Content().Element(content => BuildContent(content, detail, issuedAt));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Documento generado electrónicamente por EduApoyos.")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void BuildHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("EduApoyos")
                .FontSize(20)
                .SemiBold()
                .FontColor(Colors.Blue.Darken2);
            column.Item().Text("Constancia de Solicitud de Apoyo")
                .FontSize(14)
                .FontColor(Colors.Grey.Darken2);
            column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private static void BuildContent(
        IContainer container,
        SupportRequestDetail detail,
        DateTime issuedAt)
    {
        container.PaddingVertical(15).Column(column =>
        {
            column.Spacing(15);

            column.Item().Element(section =>
                Section(section, "Información del estudiante", inner =>
                {
                    Field(inner, "Nombre", detail.StudentFullName);
                    Field(inner, "Correo electrónico", detail.StudentEmail);
                    Field(inner, "Documento",
                        $"{DescribeDocumentType(detail.StudentDocumentType)} · {detail.StudentDocumentNumber}");
                    Field(inner, "Programa académico", detail.StudentAcademicProgram);
                    Field(inner, "Semestre", detail.StudentSemester.ToString(SpanishCulture));
                }));

            column.Item().Element(section =>
                Section(section, "Detalle de la solicitud", inner =>
                {
                    Field(inner, "Número de solicitud", detail.Id.ToString());
                    Field(inner, "Tipo de apoyo", DescribeSupportType(detail.SupportType));
                    Field(inner, "Monto solicitado",
                        detail.RequestedAmount.ToString("C", SpanishCulture));
                    Field(inner, "Estado actual", DescribeStatus(detail.Status));
                    Field(inner, "Fecha de solicitud",
                        detail.RequestedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm", SpanishCulture));
                    Field(inner, "Fecha de emisión",
                        issuedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm", SpanishCulture));
                }));

            column.Item().Element(section =>
                Section(section, "Descripción", inner =>
                {
                    inner.Item().Text(detail.Description).FontSize(11);
                }));

            column.Item().Element(section =>
                Section(section, "Historial de estados y comentarios", inner =>
                {
                    BuildHistory(inner, detail.History);
                }));
        });
    }

    private static void BuildHistory(
        ColumnDescriptor column,
        IReadOnlyList<SupportRequestHistoryItem> history)
    {
        if (history.Count == 0)
        {
            column.Item().Text("Sin registros de historial.")
                .FontColor(Colors.Grey.Darken1)
                .Italic();
            return;
        }

        column.Spacing(10);

        for (var index = 0; index < history.Count; index++)
        {
            var entry = history[index];
            var transition =
                $"{DescribeStatus(entry.PreviousStatus)} → {DescribeStatus(entry.NewStatus)}";
            var changedAt = entry.ChangedAt.ToLocalTime()
                .ToString("dd/MM/yyyy HH:mm", SpanishCulture);
            var notes = string.IsNullOrWhiteSpace(entry.Notes)
                ? "Sin observación."
                : entry.Notes.Trim();

            column.Item().Column(entryColumn =>
            {
                entryColumn.Spacing(2);
                entryColumn.Item().Text($"{index + 1}. {transition}")
                    .SemiBold()
                    .FontSize(11);
                entryColumn.Item().Text($"{changedAt} · {entry.ChangedByFullName}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken2);
                entryColumn.Item().Text(notes).FontSize(10);
            });
        }
    }

    private static void Section(
        IContainer container,
        string title,
        Action<ColumnDescriptor> builder)
    {
        container.Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(12)
            .Column(column =>
            {
                column.Spacing(6);
                column.Item().Text(title)
                    .SemiBold()
                    .FontSize(12)
                    .FontColor(Colors.Blue.Darken2);
                column.Item().Column(builder);
            });
    }

    private static void Field(ColumnDescriptor column, string label, string value)
    {
        column.Item().Row(row =>
        {
            row.ConstantItem(150).Text(label).SemiBold();
            row.RelativeItem().Text(value);
        });
    }

    private static string DescribeDocumentType(int documentType) =>
        Enum.IsDefined(typeof(DocumentType), documentType)
            ? ((DocumentType)documentType) switch
            {
                DocumentType.NationalId => "Cédula de ciudadanía",
                DocumentType.ForeignerId => "Cédula de extranjería",
                DocumentType.Passport => "Pasaporte",
                _ => documentType.ToString(SpanishCulture),
            }
            : documentType.ToString(SpanishCulture);

    private static string DescribeSupportType(int supportType) =>
        Enum.IsDefined(typeof(SupportType), supportType)
            ? ((SupportType)supportType) switch
            {
                SupportType.Scholarship => "Beca",
                SupportType.Loan => "Préstamo",
                SupportType.Subsidy => "Subsidio",
                _ => supportType.ToString(SpanishCulture),
            }
            : supportType.ToString(SpanishCulture);

    private static string DescribeStatus(int status) =>
        Enum.IsDefined(typeof(SupportRequestStatus), status)
            ? ((SupportRequestStatus)status) switch
            {
                SupportRequestStatus.Pending => "Pendiente",
                SupportRequestStatus.UnderReview => "En Revisión",
                SupportRequestStatus.Approved => "Aprobada",
                SupportRequestStatus.Rejected => "Rechazada",
                _ => status.ToString(SpanishCulture),
            }
            : status.ToString(SpanishCulture);
}
