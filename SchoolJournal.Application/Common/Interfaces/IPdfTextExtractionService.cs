using System.Collections.Generic;

namespace SchoolJournal.Application.Common.Interfaces;

public interface IPdfTextExtractionService
{
    public string ExtractTextFromPdf(IReadOnlyCollection<byte> pdfBytes, int? startPage = null, int? endPage = null);
}