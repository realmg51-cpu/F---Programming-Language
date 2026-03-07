using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        
        private static readonly Queue<string> _logQueue = new();
        private static readonly object _queueLock = new();
        private static bool _isLoggingThreadRunning = false;
        private static bool _loggingDisabled = false;
        private static readonly string _version;
        
        private const int MaxQueueSize = 1000;
        private const int MaxLogFileSize = 10 * 1024 * 1024; // 10MB
        private const int MaxBackupFiles = 5;
        
        static SecureErrorHandler()
        {
            try
            {
                _version = typeof(SecureErrorHandler).Assembly.GetName().Version?.ToString() ?? "unknown";
                EnsureLogFileAccessible();
            }
            catch
            {
                _version = "unknown";
            }
        }

        /// <summary>
        /// Handle error safely - log details, show generic message
        /// </summary>
        public static void Handle(Exception ex, string userMessage = "An error occurred")
        {
            // Log full details
            LogErrorAsync(ex);

            // Show safe message to user
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ {userMessage}");
            if (!_loggingDisabled)
            {
                Console.WriteLine("   Check fminus-error.log for details.");
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Handle multiple errors safely
        /// </summary>
        public static void HandleMultiple(AggregateException ex)
        {
            foreach (var inner in ex.InnerExceptions)
            {
                LogErrorAsync(inner);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Multiple errors occurred ({ex.InnerExceptions.Count} errors)");
            if (!_loggingDisabled)
            {
                Console.WriteLine("   Check fminus-error.log for details.");
            }
            Console.ResetColor();
        }

        private static void EnsureLogFileAccessible()
        {
            try
            {
                string logDir = Path.GetDirectoryName(_logFile) ?? ".";
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);
                
                // Test write permission
                using (File.Open(_logFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read)) { }
                _loggingDisabled = false;
            }
            catch
            {
                _loggingDisabled = true;
            }
        }

        private static void RotateLogIfNeeded()
        {
            if (_loggingDisabled) return;
            
            try
            {
                var fileInfo = new FileInfo(_logFile);
                if (fileInfo.Exists && fileInfo.Length > MaxLogFileSize)
                {
                    for (int i = MaxBackupFiles; i >= 1; i--)
                    {
                        string oldBackup = _logFile + "." + i;
                        string newBackup = _logFile + "." + (i + 1);
                        
                        if (File.Exists(oldBackup))
                        {
                            if (File.Exists(newBackup))
                                File.Delete(newBackup);
                            File.Move(oldBackup, newBackup);
                        }
                    }
                    
                    File.Move(_logFile, _logFile + ".1");
                }
            }
            catch { }
        }

        private static string SanitizeErrorMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return message;
            
            try
            {
                // Windows paths
                message = Regex.Replace(message, @"[A-Za-z]:\\[^\s<>:""/\\|?*]*", "[PATH]", 
                    RegexOptions.None, TimeSpan.FromMilliseconds(100));
                
                // Linux/macOS paths
                message = Regex.Replace(message, @"(?:/home|/Users|/tmp|/var|/etc|/opt)[^\s]*", "[PATH]",
                    RegexOptions.None, TimeSpan.FromMilliseconds(100));
                
                // IP addresses
                message = Regex.Replace(message, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", "[IP]",
                    RegexOptions.None, TimeSpan.FromMilliseconds(100));
                
                // Email addresses
                message = Regex.Replace(message, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", "[EMAIL]",
                    RegexOptions.None, TimeSpan.FromMilliseconds(100));
                
                // User names
                if (!string.IsNullOrEmpty(Environment.UserName))
                {
                    message = message.Replace(Environment.UserName, "[USER]");
                }
                
                // Machine name
                if (!string.IsNullOrEmpty(Environment.MachineName))
                {
                    message = message.Replace(Environment.MachineName, "[MACHINE]");
                }
                
                // Connection strings (simplified)
                message = Regex.Replace(message, @"(?:server|database|uid|pwd)=[^;]+", "$1=[REDACTED]",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
            }
            catch (RegexMatchTimeoutException)
            {
                return message.Length > 200 ? message.Substring(0, 200) + "..." : message;
            }
            
            return message;
        }

        private static string SanitizeStackTrace(string? stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return "";
            
            try
            {
                var lines = stackTrace.Split('\n');
                var result = new System.Text.StringBuilder();
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    // Remove file paths from stack trace
                    string sanitized = Regex.Replace(line, @" in [^\s]+\.cs:line \d+", "",
                        RegexOptions.None, TimeSpan.FromMilliseconds(50));
                    
                    // Keep only method names and parameters
                    sanitized = Regex.Replace(sanitized, @"=\s*[^,)]+", "=?",
                        RegexOptions.None, TimeSpan.FromMilliseconds(50));
                    
                    result.AppendLine(sanitized);
                }
                
                return result.ToString();
            }
            catch
            {
                return "[Stack trace sanitized]";
            }
        }

        private static string GetErrorCode(Exception ex)
        {
            return ex switch
            {
                FileError fe => fe.ErrorCode,
                SyntaxError se => "SYNTAX",
                _ => ex.GetType().Name
            };
        }

        private static string GetSeverity(Exception ex)
        {
            return ex switch
            {
                FileError fe => fe.Severity.ToString(),
                _ => "Error"
            };
        }

        private static void LogErrorAsync(Exception ex)
        {
            if (_loggingDisabled) return;
            
            var safeMessage = SanitizeErrorMessage(ex.Message);
            var logEntry = new
            {
                Timestamp = DateTime.Now,
                Version = _version,
                Type = ex.GetType().Name,
                Code = GetErrorCode(ex),
                Severity = GetSeverity(ex),
                Message = safeMessage,
                StackTrace = SanitizeStackTrace(ex.StackTrace)
            };
            
            string jsonEntry = System.Text.Json.JsonSerializer.Serialize(logEntry) + "\n";

            lock (_queueLock)
            {
                if (_logQueue.Count < MaxQueueSize)
                {
                    _logQueue.Enqueue(jsonEntry);
                }
            }
            
            if (!_isLoggingThreadRunning)
            {
                _isLoggingThreadRunning = true;
                System.Threading.ThreadPool.QueueUserWorkItem(_ => ProcessLogQueue());
            }
        }

        private static void ProcessLogQueue()
        {
            while (true)
            {
                string? entry = null;
                lock (_queueLock)
                {
                    if (_logQueue.Count == 0)
                    {
                        _isLoggingThreadRunning = false;
                        break;
                    }
                    entry = _logQueue.Dequeue();
                }
                
                if (entry != null && !_loggingDisabled)
                {
                    try
                    {
                        RotateLogIfNeeded();
                        File.AppendAllText(_logFile, entry);
                    }
                    catch
                    {
                        _loggingDisabled = true;
                        break;
                    }
                }
            }
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

        /// <summary>
        /// Get log file path
        /// </summary>
        public static string GetLogPath() => _logFile;
    }
}
