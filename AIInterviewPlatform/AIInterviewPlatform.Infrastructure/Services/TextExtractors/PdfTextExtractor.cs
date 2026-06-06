using AIInterviewPlatform.Application.Interfaces.Services;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;

namespace AIInterviewPlatform.Infrastructure.Services.TextExtractors
{
    /// <summary>
    /// Extracts plain text from PDF resume files.
    /// </summary>
    public class PdfTextExtractor : IPdfTextExtractor
    {
        /// <inheritdoc />
        public async Task<string> ExtractTextAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path is required.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("PDF file was not found.", filePath);
            }

            try
            {
                return await Task.Run(() =>
                {
                    var builder = new StringBuilder();

                    using var document = PdfDocument.Open(filePath);

                    foreach (var page in document.GetPages())
                    {
                        var pageText = page.Text;

                        if (!string.IsNullOrWhiteSpace(pageText))
                        {
                            builder.AppendLine(pageText.Trim());
                        }
                    }

                    return NormalizeText(builder.ToString());
                });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PdfDocumentFormatException or InvalidOperationException)
            {
                throw new InvalidOperationException($"Failed to extract text from PDF file '{filePath}'.", ex);
            }
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");

            return normalized.Trim();
        }
    }
}
