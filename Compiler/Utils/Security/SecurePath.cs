using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Fminusminus.Utils.Security
{
    /// <summary>
    /// Secure path handling to prevent path traversal attacks
    /// </summary>
    public static class SecurePath
    {
        /// <summary>
        /// Maximum file size to read (10 MB)
        /// </summary>
        public const long MaxFileSize = 10 * 1024 * 1024;
        
        /// <summary>
        /// Maximum lines to read from a file
        /// </summary>
        public const int MaxLines = 10000;
        
        /// <summary>
        /// Maximum files to list in a directory
        /// </summary>
        public const int MaxFilesToList = 1000;

        /// <summary>
        /// Sanitize and validate a file path
        /// </summary>
        public static string Sanitize(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be empty");

            // Normalize Unicode
            path = path.Normalize();
            
            // Decode URL-encoded characters
            try
            {
                path = Uri.UnescapeDataString(path);
            }
            catch
            {
                // If decoding fails, use original path
            }
            
            // Normalize path separators
            path = path.Replace('\\', Path.DirectorySeparatorChar)
                       .Replace('/', Path.DirectorySeparatorChar);
            
            // Split and check each segment for traversal attempts
            var segments = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                if (segment == ".." || segment == ".")
                    throw new UnauthorizedAccessException("Path traversal attacks are not allowed");
                
                // Check for encoded traversal attempts
                if (segment.Contains("%2e", StringComparison.OrdinalIgnoreCase) ||
                    segment.Contains("%2E", StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException("Encoded path traversal detected");
            }

            // Get full path and ensure it's within current directory
            var baseDir = Environment.CurrentDirectory;
            var fullPath = Path.GetFullPath(Path.Combine(baseDir, path));

            if (!fullPath.StartsWith(baseDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) 
                && !fullPath.Equals(baseDir, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Cannot access files outside working directory");

            // Check symbolic links on Unix systems
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(fullPath))
            {
                var fileInfo = new FileInfo(fullPath);
                if (fileInfo.LinkTarget != null)
                {
                    string targetFullPath = Path.GetFullPath(Path.Combine(fileInfo.DirectoryName ?? "", fileInfo.LinkTarget));
                    if (!targetFullPath.StartsWith(baseDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        throw new UnauthorizedAccessException("Symbolic links pointing outside working directory are not allowed");
                }
            }

            return fullPath;
        }

        /// <summary>
        /// Validate filename for invalid characters
        /// </summary>
        public static bool IsValidFileName(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            return !filename.Any(c => invalidChars.Contains(c));
        }

        /// <summary>
        /// Check if file size is within limits
        /// </summary>
        public static bool IsFileSizeValid(string filePath)
        {
            if (!File.Exists(filePath))
                return false;
            
            try
            {
                var info = new FileInfo(filePath);
                return info.Length <= MaxFileSize;
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Rethrow access exceptions
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Safe file reading with size and line limits
        /// </summary>
        public static string[] SafeReadAllLines(string filePath, int maxLines = MaxLines, long maxBytes = MaxFileSize)
        {
            string safePath = Sanitize(filePath);
            
            if (!File.Exists(safePath))
                throw new FileNotFoundException($"File not found: {Path.GetFileName(filePath)}");
            
            var fileInfo = new FileInfo(safePath);
            if (fileInfo.Length > maxBytes)
                throw new InvalidOperationException($"File size ({fileInfo.Length} bytes) exceeds maximum allowed ({maxBytes} bytes)");
            
            var lines = new System.Collections.Generic.List<string>();
            long bytesRead = 0;
            
            using var reader = new StreamReader(safePath);
            for (int i = 0; i < maxLines; i++)
            {
                if (reader.EndOfStream) break;
                
                string? line = reader.ReadLine();
                if (line == null) break;
                
                bytesRead += System.Text.Encoding.UTF8.GetByteCount(line) + 2; // +2 for \r\n
                if (bytesRead > maxBytes)
                    throw new InvalidOperationException($"File read exceeded maximum size limit");
                
                lines.Add(line);
            }

            return lines.ToArray();
        }

        /// <summary>
        /// Safe file reading as text with size limits
        /// </summary>
        public static string SafeReadAllText(string filePath)
        {
            string safePath = Sanitize(filePath);
            
            if (!File.Exists(safePath))
                throw new FileNotFoundException($"File not found: {Path.GetFileName(filePath)}");
            
            var fileInfo = new FileInfo(safePath);
            if (fileInfo.Length > MaxFileSize)
                throw new InvalidOperationException($"File size ({fileInfo.Length} bytes) exceeds maximum allowed ({MaxFileSize} bytes)");
            
            using var reader = new StreamReader(safePath);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Safe file writing with size limits
        /// </summary>
        public static void SafeWriteAllText(string filePath, string content)
        {
            string safePath = Sanitize(filePath);
            
            // Check if file would exceed size limit
            long contentBytes = System.Text.Encoding.UTF8.GetByteCount(content);
            if (contentBytes > MaxFileSize)
                throw new InvalidOperationException($"File content ({contentBytes} bytes) exceeds maximum allowed ({MaxFileSize} bytes)");
            
            File.WriteAllText(safePath, content);
        }

        /// <summary>
        /// List files in directory with limits
        /// </summary>
        public static FileInfoSafe[] SafeListFiles(string directory, string pattern = "*", int maxFiles = MaxFilesToList)
        {
            string safePath = Sanitize(directory);
            
            if (!Directory.Exists(safePath))
                throw new DirectoryNotFoundException($"Directory not found: {directory}");
            
            var files = Directory.GetFiles(safePath, pattern)
                                .Take(maxFiles)
                                .Select(f => GetSafeFileInfo(f))
                                .ToArray();
            
            return files;
        }

        /// <summary>
        /// Get safe file info without exposing full path
        /// </summary>
        public static FileInfoSafe GetSafeFileInfo(string filePath)
        {
            string safePath = Sanitize(filePath);
            
            if (!File.Exists(safePath))
                throw new FileNotFoundException($"File not found: {Path.GetFileName(filePath)}");
            
            var info = new FileInfo(safePath);
            return new FileInfoSafe
            {
                Name = info.Name,
                Size = info.Length,
                CreationTime = info.CreationTime,
                LastWriteTime = info.LastWriteTime,
                IsReadOnly = info.IsReadOnly
            };
        }

        /// <summary>
        /// Check if path is a directory
        /// </summary>
        public static bool IsDirectory(string path)
        {
            string safePath = Sanitize(path);
            return Directory.Exists(safePath);
        }

        /// <summary>
        /// Check if path is a file
        /// </summary>
        public static bool IsFile(string path)
        {
            string safePath = Sanitize(path);
            return File.Exists(safePath);
        }

        /// <summary>
        /// Safe file information (no full path exposure)
        /// </summary>
        public class FileInfoSafe
        {
            public string Name { get; set; } = string.Empty;
            public long Size { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime LastWriteTime { get; set; }
            public bool IsReadOnly { get; set; }
            
            public override string ToString()
            {
                return $"{Name} ({Size} bytes)";
            }
        }
    }
}
