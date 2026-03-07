using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Fminusminus.Main.Commands
{
    /// <summary>
    /// Handle version display
    /// </summary>
    public static class VersionCommand
    {
        private static readonly string[] Contributors = new[]
        {
            "realmg51-cpu (Creator, 13 years old)",
            "chaunguyen12477-cmyk (Contributor)",
            "GitHub Copilot (AI Assistant)"
        };

        public static int Execute(string[]? args = null)
        {
            bool verbose = false;
            bool helpRequested = false;
            
            if (args != null)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
                        case "--verbose":
                        case "-v":
                            verbose = true;
                            break;
                        case "--help":
                        case "-h":
                            helpRequested = true;
                            break;
                    }
                }
            }

            if (helpRequested)
            {
                ShowHelp();
                return 0;
            }

            ShowVersion(verbose);
            
            if (verbose)
            {
                // Check for updates asynchronously (don't wait)
                _ = Task.Run(CheckForUpdates);
            }
            
            return 0;
        }

        private static void ShowVersion(bool verbose)
        {
            // Main version info
            Console.WriteLine($"F-- Programming Language v{GetVersion()}");
            Console.WriteLine($"Build: {GetBuildDate()}");
            
            if (GetCommitHash() is string commitHash)
            {
                Console.WriteLine($"Commit: {commitHash}");
            }
            
            Console.WriteLine($"Copyright (c) 2026 RealMG");
            Console.WriteLine($"License: MIT (https://opensource.org/licenses/MIT)");
            
            // Quote
            Console.WriteLine("\n\"The backward step of humanity, but forward step in creativity!\"");
            
            // Contributors
            Console.WriteLine("\n👥 Contributors:");
            foreach (var contributor in Contributors)
            {
                Console.WriteLine($"  • {contributor}");
            }

            // Verbose information
            if (verbose)
            {
                Console.WriteLine("\n📊 System Information:");
                Console.WriteLine($"  • Runtime: {RuntimeInformation.FrameworkDescription}");
                Console.WriteLine($"  • OS: {RuntimeInformation.OSDescription}");
                Console.WriteLine($"  • Architecture: {RuntimeInformation.ProcessArchitecture}");
                Console.WriteLine($"  • 64-bit: {Environment.Is64BitProcess}");
                Console.WriteLine($"  • Processors: {Environment.ProcessorCount}");
                Console.WriteLine($"  • Working Directory: {Environment.CurrentDirectory}");
                Console.WriteLine($"  • User: {Environment.UserName}");
                Console.WriteLine($"  • Machine: {Environment.MachineName}");
                
                Console.WriteLine("\n📦 Dependencies:");
                Console.WriteLine("  • No external dependencies - pure .NET!");
                
                Console.WriteLine("\n🔧 Environment:");
                Console.WriteLine($"  • Version: {Environment.Version}");
                Console.WriteLine($"  • Command Line: {Environment.CommandLine}");
                Console.WriteLine($"  • Tick Count: {Environment.TickCount64} ms");
                
                Console.WriteLine("\n📍 Assembly Locations:");
                Console.WriteLine($"  • Entry: {Assembly.GetEntryAssembly()?.Location ?? "N/A"}");
                Console.WriteLine($"  • Executing: {Assembly.GetExecutingAssembly().Location}");
            }
        }

        private static string GetVersion()
        {
            try
            {
                var assembly = typeof(VersionCommand).Assembly;
                var version = assembly.GetName().Version;
                if (version != null)
                {
                    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
                
                // Try to get from informational version
                var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (informationalVersion != null && !string.IsNullOrEmpty(informationalVersion.InformationalVersion))
                {
                    return informationalVersion.InformationalVersion;
                }
            }
            catch { }
            
            return "2.0.0.0-alpha1"; // fallback
        }

        private static string GetBuildDate()
        {
            try
            {
                var assembly = typeof(VersionCommand).Assembly;
                var fileInfo = new System.IO.FileInfo(assembly.Location);
                return fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        private static string? GetCommitHash()
        {
            try
            {
                var assembly = typeof(VersionCommand).Assembly;
                var attributes = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false);
                foreach (AssemblyMetadataAttribute attr in attributes)
                {
                    if (attr.Key == "CommitHash")
                        return attr.Value;
                }
            }
            catch { }
            return null;
        }

        private static async Task CheckForUpdates()
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Fminusminus/2.0");
                
                var response = await client.GetStringAsync("https://api.nuget.org/v3-flatcontainer/fminusminus/index.json");
                
                // Simple check - in production, parse JSON properly
                if (response.Contains("2.0.1"))
                {
                    Console.WriteLine("\n✨ Update available: v2.0.1");
                    Console.WriteLine("   Run: dotnet tool update -g fminusminus");
                }
            }
            catch
            {
                // Silently fail - don't bother user with network errors
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("version - Display version information");
            Console.WriteLine("======================================");
            Console.WriteLine();
            Console.WriteLine("Usage: fminus version [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -v, --verbose    Show detailed system information");
            Console.WriteLine("  -h, --help       Show this help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  fminus version");
            Console.WriteLine("  fminus version --verbose");
        }
    }
}
