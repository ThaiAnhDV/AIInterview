using AIInterviewPlatform.Application.Interfaces.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace AIInterviewPlatform.Infrastructure.Services.TextExtractors
{
    /// <summary>
    /// Extracts plain text from DOCX resume files.
    /// </summary>
    public class DocxTextExtractor : IDocxTextExtractor
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
                throw new FileNotFoundException("DOCX file was not found.", filePath);
            }

            try
            {
                return await Task.Run(() =>
                {
                    var builder = new StringBuilder();

                    using var document = WordprocessingDocument.Open(filePath, false);
                    var body = document.MainDocumentPart?.Document?.Body;

                    if (body == null)
                    {
                        return string.Empty;
                    }

                    foreach (var paragraph in body.Descendants<Paragraph>())
                    {
                        var paragraphText = paragraph.InnerText;

                        if (!string.IsNullOrWhiteSpace(paragraphText))
                        {
                            builder.AppendLine(paragraphText.Trim());
                        }
                    }

                    return NormalizeText(builder.ToString());
                });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OpenXmlPackageException or InvalidDataException or InvalidOperationException)
            {
                throw new InvalidOperationException($"Failed to extract text from DOCX file '{filePath}'.", ex);
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
