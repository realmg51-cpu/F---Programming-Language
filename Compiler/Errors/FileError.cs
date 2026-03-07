using System;

namespace Fminusminus.Errors
{
    /// <summary>
    /// File-related errors for F-- programming language
    /// </summary>
    public class FileError : Exception
    {
        public string ErrorCode { get; }
        public string FilePath { get; }
        public ErrorSeverity Severity { get; }

        public enum ErrorSeverity
        {
            Warning,
            Error,
            Fatal
        }

        private FileError(string message, string code, string path, ErrorSeverity severity) 
            : base($"[{code}] {message}")
        {
            ErrorCode = code;
            FilePath = path;
            Severity = severity;
        }

        /// <summary>
        /// FMM050: Access denied error
        /// </summary>
        public static FileError AccessDenied(string path)
        {
            return new FileError(
                $"Access denied! Cannot access '{path}'",
                "FMM050",
                path,
                ErrorSeverity.Error
            );
        }

        /// <summary>
        /// FMM051: Invalid filename characters
        /// </summary>
        public static FileError InvalidCharacters(string filename)
        {
            return new FileError(
                $"File name shouldn't have special characters! Invalid: '{filename}'",
                "FMM051",
                filename,
                ErrorSeverity.Error
            );
        }

        /// <summary>
        /// FMM052: File not found
        /// </summary>
        public static FileError NotFound(string path)
        {
            return new FileError(
                $"File or directory not found: '{path}'",
                "FMM052",
                path,
                ErrorSeverity.Error
            );
        }

        /// <summary>
        /// FMM053: Path too long
        /// </summary>
        public static FileError PathTooLong(string path)
        {
            return new FileError(
                $"Path is too long: '{path}'",
                "FMM053",
                path,
                ErrorSeverity.Error
            );
        }

        /// <summary>
        /// FMM054: Directory not empty
        /// </summary>
        public static FileError DirectoryNotEmpty(string path)
        {
            return new FileError(
                $"Directory is not empty: '{path}'",
                "FMM054",
                path,
                ErrorSeverity.Warning
            );
        }

        /// <summary>
        /// Check if filename contains invalid characters
        /// </summary>
        public static bool HasInvalidCharacters(string filename)
        {
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                if (filename.Contains(c))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Validate filename and throw if invalid
        /// </summary>
        public static void ValidateFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new FileError("Filename cannot be empty", "FMM055", filename, ErrorSeverity.Error);
            
            if (HasInvalidCharacters(filename))
                throw InvalidCharacters(filename);
            
            if (filename.Length > 255)
                throw new FileError("Filename too long (max 255 characters)", "FMM056", filename, ErrorSeverity.Error);
        }
    }
}
