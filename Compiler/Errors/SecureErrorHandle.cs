using System;
using System.IO;

namespace Fminusminus.Errors
{
    /// <summary>
    /// Secure error handling - no sensitive info leakage
    /// </summary>
    public static class SecureErrorHandler
    {
        private static readonly string _logFile = Path.Combine(
            Environment.CurrentDirectory, 
            "fminus-error.log"
        );

        /// <summary>
        /// Handle error safely - log details, show generic message
        /// </summary>
        public static void Handle(Exception ex, string userMessage = "An error occurred")
        {
            // Log full details
            LogError(ex);

            // Show safe message to user
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ {userMessage}");
            Console.WriteLine("   Check fminus-error.log for details.");
            Console.ResetColor();
        }

        /// <summary>
        /// Handle multiple errors safely
        /// </summary>
        public static void HandleMultiple(AggregateException ex)
        {
            foreach (var inner in ex.InnerExceptions)
            {
                LogError(inner);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Multiple errors occurred ({ex.InnerExceptions.Count} errors)");
            Console.WriteLine("   Check fminus-error.log for details.");
            Console.ResetColor();
        }

        /// <summary>
        /// Log error to file (safe, no sensitive data)
        /// </summary>
        private static void LogError(Exception ex)
        {
            try
            {
                var safeMessage = SanitizeErrorMessage(ex.Message);
                var logEntry = $@"
=== Error at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===
Type: {ex.GetType().Name}
Message: {safeMessage}
Stack: {SanitizeStackTrace(ex.StackTrace)}
==========================================
";

                File.AppendAllText(_logFile, logEntry);
            }
            catch
            {
                // Can't log, ignore
            }
        }

        /// <summary>
        /// Remove sensitive info from error message
        /// </summary>
        private static string SanitizeErrorMessage(string message)
        {
            // Remove file paths
            message = System.Text.RegularExpressions.Regex.Replace(
                message,
                @"[A-Za-z]:\\[^\s]+",
                "[PATH]"
            );

            // Remove user names
            message = System.Text.RegularExpressions.Regex.Replace(
                message,
                Environment.UserName,
                "[USER]"
            );

            return message;
        }

        /// <summary>
        /// Remove sensitive info from stack trace
        /// </summary>
        private static string SanitizeStackTrace(string? stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return "";

            var lines = stackTrace.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                // Keep only first 5 lines
                if (i >= 5)
                {
                    lines[i] = "";
                }
            }

            return string.Join("\n", lines.Where(l => !string.IsNullOrEmpty(l)));
        }

        /// <summary>
        /// Show warning to user
        /// </summary>
        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n⚠️ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Show info to user
        /// </summary>
        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nℹ️ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Show success to user
        /// </summary>
        public static void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✅ {message}");
            Console.ResetColor();
        }
    }
}
