using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using Tesseract;
using PDFtoImage;
using SchoolJournal.Application.Common.Interfaces;

namespace SchoolJournal.Infrastructure.Services.Files;

public sealed partial class PdfTextExtractionService(ILogger<PdfTextExtractionService> logger) : IPdfTextExtractionService
{
    public string ExtractTextFromPdf(IReadOnlyCollection<byte> pdfBytes, int? startPage = null, int? endPage = null)
    {
        byte[] pdfBytesArray = [.. pdfBytes];

        var textBuilder = new StringBuilder();
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory ?? AppContext.BaseDirectory ?? string.Empty;
        string tessDataPath = Path.Combine(baseDirectory, "tessdata");

        using var ocrEngine = new TesseractEngine(tessDataPath, "ukr+eng", EngineMode.Default);
        using var document = PdfDocument.Open(pdfBytesArray);

        int totalPages = document.NumberOfPages;
        int start = Math.Max(1, startPage ?? 1);
        int end = Math.Min(totalPages, endPage ?? totalPages);
        if (start > end) start = end;

        var stats = new ExtractionStats();

        for (int i = start; i <= end; i++)
        {
            ProcessPage(document, i, pdfBytesArray, ocrEngine, textBuilder, stats, logger);
        }

        string finalResult = textBuilder.ToString();

        if (string.IsNullOrWhiteSpace(finalResult))
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Діагностика! Сторінок: {0}. Картинок: {1}. Помилок OCR: {2}.",
                end - start + 1, stats.TotalImagesFound, stats.FailedInOcr));
        }

        return finalResult.Length > 300000 ? finalResult[..300000] : finalResult;
    }

    private static void ProcessPage(PdfDocument document, int pageIndex, byte[] pdfBytes, TesseractEngine ocrEngine, StringBuilder textBuilder, ExtractionStats stats, ILogger logger)
    {
        try
        {
            var page = document.GetPage(pageIndex);
            string pageText = page.Text ?? string.Empty;
            bool hasImages = page.GetImages().Any();

            if (hasImages) stats.TotalImagesFound++;

            textBuilder.AppendLine(CultureInfo.InvariantCulture, $"{pageText}");

            if (hasImages)
            {
                PerformOcr(pdfBytes, pageIndex, ocrEngine, textBuilder, stats, logger);
            }
        }
        catch (IOException ex)
        {
            stats.FailedToExtractBytes++;
            textBuilder.AppendLine(CultureInfo.InvariantCulture, $"[Помилка сторінки {pageIndex}: {ex.Message}]");
        }
    }

    private static void PerformOcr(byte[] pdfBytes, int pageIndex, TesseractEngine ocrEngine, StringBuilder textBuilder, ExtractionStats stats, ILogger logger)
    {
        try
        {
            using var stream = new MemoryStream();

#pragma warning disable CA1416 
            Conversion.SavePng(stream, pdfBytes, page: pageIndex - 1);
#pragma warning restore CA1416

            using var pix = Pix.LoadFromMemory(stream.ToArray());
            using var ocrPage = ocrEngine.Process(pix);

            string ocrText = ocrPage.GetText() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(ocrText))
            {
                textBuilder.AppendLine(CultureInfo.InvariantCulture, $"\n\n--- РОЗПІЗНАНО ЗІ СКАНУ ---");
                textBuilder.AppendLine(CultureInfo.InvariantCulture, $"{ocrText}");
            }
        }
        catch (TesseractException ex)
        {
            stats.FailedInOcr++;
            stats.LastOcrError = ex.Message;
            LogOcrError(logger, pageIndex, ex.Message);
            textBuilder.AppendLine(CultureInfo.InvariantCulture, $"[Помилка OCR на сторінці {pageIndex}: {ex.Message}]");
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Помилка OCR на сторінці {PageNumber}: {ErrorMessage}")]
    private static partial void LogOcrError(ILogger logger, int pageNumber, string errorMessage);
}

public class ExtractionStats
{
    public int TotalImagesFound { get; set; }
    public int FailedToExtractBytes { get; set; }
    public int FailedInOcr { get; set; }
    public string LastOcrError { get; set; } = string.Empty;
}