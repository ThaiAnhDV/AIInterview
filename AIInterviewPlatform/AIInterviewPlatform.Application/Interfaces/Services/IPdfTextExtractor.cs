namespace AIInterviewPlatform.Application.Interfaces.Services
{
    /// <summary>
    /// Extracts plain text from PDF resume files.
    /// </summary>
    public interface IPdfTextExtractor
    {
        /// <summary>
        /// Extracts normalized text from a PDF document.
        /// </summary>
        /// <param name="filePath">Absolute path to the PDF file.</param>
        /// <returns>A task that resolves to the extracted text.</returns>
        Task<string> ExtractTextAsync(string filePath);
    }
}
