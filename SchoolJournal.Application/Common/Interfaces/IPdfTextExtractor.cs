namespace SchoolJournal.Application.Common.Interfaces;

public interface IPdfTextExtractor
{
    public string ExtractText(IReadOnlyCollection<byte> pdfBytes, int? startPage = null, int? endPage = null);
}