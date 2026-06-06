namespace AIInterviewPlatform.Application.Interfaces.Services
{
    /// <summary>
    /// Extracts plain text from DOCX resume files.
    /// </summary>
    public interface IDocxTextExtractor
    {
        /// <summary>
        /// Extracts normalized text from a DOCX document.
        /// </summary>
        /// <param name="filePath">Absolute path to the DOCX file.</param>
        /// <returns>A task that resolves to the extracted text.</returns>
        Task<string> ExtractTextAsync(string filePath);
    }
}
