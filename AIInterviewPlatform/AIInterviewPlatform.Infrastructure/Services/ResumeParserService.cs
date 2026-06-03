using AIInterviewPlatform.Application.Interfaces.Services;

namespace AIInterviewPlatform.Infrastructure.Services
{
    /// <summary>
    /// Coordinates resume text extraction by routing to the correct file-specific extractor.
    /// </summary>
    public class ResumeParserService : IResumeParserService
    {
        private readonly IPdfTextExtractor _pdfTextExtractor;
        private readonly IDocxTextExtractor _docxTextExtractor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResumeParserService"/> class.
        /// </summary>
        /// <param name="pdfTextExtractor">PDF text extractor dependency.</param>
        /// <param name="docxTextExtractor">DOCX text extractor dependency.</param>
        public ResumeParserService(
            IPdfTextExtractor pdfTextExtractor,
            IDocxTextExtractor docxTextExtractor)
        {
            _pdfTextExtractor = pdfTextExtractor ?? throw new ArgumentNullException(nameof(pdfTextExtractor));
            _docxTextExtractor = docxTextExtractor ?? throw new ArgumentNullException(nameof(docxTextExtractor));
        }

        /// <inheritdoc />
        public async Task<string> ExtractTextAsync(string filePath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path is required.", nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name is required.", nameof(fileName));
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => await _pdfTextExtractor.ExtractTextAsync(filePath),
                ".docx" => await _docxTextExtractor.ExtractTextAsync(filePath),
                _ => throw new NotSupportedException($"Unsupported resume format '{extension}'. Only .pdf and .docx files are supported.")
            };
        }
    }
}
