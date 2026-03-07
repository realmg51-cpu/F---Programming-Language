using System;
using System.IO;

namespace Fminusminus.Errors
{
    /// <summary>
    /// Error codes for file operations
    /// </summary>
    public static class FileErrorCodes
    {
        public const string AccessDenied = "FMM050";
        public const string InvalidCharacters = "FMM051";
        public const string NotFound = "FMM052";
        public const string PathTooLong = "FMM053";
        public const string DirectoryNotEmpty = "FMM054";
        public const string EmptyFilename = "FMM055";
        public const string FilenameTooLong = "FMM056";
        public const string ReservedName = "FMM057";
        public const string DiskFull = "FMM058";
        public const string AlreadyExists = "FMM059";
        public const string IOException = "FMM060";
    }

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

        public FileError(string message, string code, string path, ErrorSeverity severity, Exception? innerException = null) 
            : base($"[{code}] {message}", innerException)
        {
            ErrorCode = code;
            FilePath = path;
            Severity = severity;
        }

        /// <summary>
        /// FMM050: Access denied error
        /// </summary>
        public static FileError AccessDenied(string path, Exception? innerException = null)
        {
            return new FileError(
                $"Access denied! Cannot access '{path}'",
                FileErrorCodes.AccessDenied,
                path,
                ErrorSeverity.Error,
                innerException
            );
        }

        /// <summary>
        /// FMM051: Invalid filename characters
        /// </summary>
        public static FileError InvalidCharacters(string filename)
        {
            return new FileError(
                $"File name shouldn't have special characters! Invalid: '{filename}'",
                FileErrorCodes.InvalidCharacters,
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
                FileErrorCodes.NotFound,
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
                FileErrorCodes.PathTooLong,
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
                FileErrorCodes.DirectoryNotEmpty,
                path,
                ErrorSeverity.Warning
            );
        }

        /// <summary>
        /// FMM058: Disk full
        /// </summary>
        public static FileError DiskFull(string path)
        {
            return new FileError(
                "Not enough disk space to complete operation",
                FileErrorCodes.DiskFull,
                path,
                ErrorSeverity.Fatal
            );
        }

        /// <summary>
        /// FMM059: File already exists
        /// </summary>
        public static FileError AlreadyExists(string path)
        {
            return new FileError(
                $"File already exists: '{path}'",
                FileErrorCodes.AlreadyExists,
                path,
                ErrorSeverity.Warning
            );
        }

        /// <summary>
        /// FMM060: General IO error
        /// </summary>
        public static FileError IOError(string path, Exception innerException)
        {
            return new FileError(
                $"I/O error occurred while accessing '{path}': {innerException.Message}",
                FileErrorCodes.IOException,
                path,
                ErrorSeverity.Error,
                innerException
            );
        }

        /// <summary>
        /// Check if filename contains invalid characters
        /// </summary>
        public static bool HasInvalidCharacters(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return true;
            
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                if (filename.IndexOf(c) >= 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if filename is a reserved Windows name
        /// </summary>
        public static bool IsReservedName(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return false;
            
            string nameWithoutExt = Path.GetFileNameWithoutExtension(filename).ToUpperInvariant();
            string[] reservedNames = { 
                "CON", "PRN", "AUX", "NUL", 
                "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };
            
            foreach (var reserved in reservedNames)
            {
                if (nameWithoutExt == reserved)
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
                throw new FileError("Filename cannot be empty", FileErrorCodes.EmptyFilename, filename, ErrorSeverity.Error);
            
            if (HasInvalidCharacters(filename))
                throw InvalidCharacters(filename);
            
            if (filename.Length > 255)
                throw new FileError("Filename too long (max 255 characters)", FileErrorCodes.FilenameTooLong, filename, ErrorSeverity.Error);
            
            if (IsReservedName(filename))
                throw new FileError($"Cannot use reserved Windows name: {Path.GetFileNameWithoutExtension(filename)}", 
                    FileErrorCodes.ReservedName, filename, ErrorSeverity.Error);
        }

        /// <summary>
        /// Create FileError from IOException
        /// </summary>
        public static FileError FromIOException(IOException ex, string path)
        {
            return ex switch
            {
                FileNotFoundException => NotFound(path),
                DirectoryNotFoundException => NotFound(path),
                UnauthorizedAccessException => AccessDenied(path, ex),
                PathTooLongException => PathTooLong(path),
                _ => IOError(path, ex)
            };
        }

        public override string ToString()
        {
            return $"[{ErrorCode}] {Message} (Severity: {Severity}, Path: {FilePath})";
        }
    }
}
