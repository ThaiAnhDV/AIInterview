namespace AIInterviewPlatform.Application.Interfaces.Services
{
    /// <summary>
    /// Coordinates resume text parsing by selecting the appropriate file-specific extractor.
    /// </summary>
    public interface IResumeParserService
    {
        /// <summary>
        /// Extracts normalized plain text from a resume file.
        /// </summary>
        /// <param name="filePath">Absolute path to the uploaded resume file.</param>
        /// <param name="fileName">Original file name used to infer the document format.</param>
        /// <returns>A task that resolves to the extracted resume text.</returns>
        Task<string> ExtractTextAsync(string filePath, string fileName);
    }
}
