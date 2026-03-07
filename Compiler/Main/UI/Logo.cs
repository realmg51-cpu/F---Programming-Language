using System;
using System.Reflection;
using System.IO;

namespace Fminusminus.Main.UI
{
    /// <summary>
    /// Display F-- logo
    /// </summary>
    public static class Logo
    {
        private static readonly Random _random = new();
        
        private static readonly string[] Tips = {
            "💡 Tip: Use 'fminus help' to see all commands",
            "💡 Tip: You can import multiple packages with 'import'",
            "💡 Tip: Check out the 'examples/' folder for sample code",
            "💡 Tip: Use 'fminus ast' to see the syntax tree",
            "💡 Tip: Join our Discord community! (coming soon)",
            "💡 Tip: Try 'fminus codegen' to generate other languages",
            "💡 Tip: Use 'fminus compile' to create .NET executables",
            "💡 Tip: The 'computer' package gives you system info",
            "💡 Tip: Variables are dynamically typed!",
            "💡 Tip: Use 'println' for output with newline"
        };

        public static void Display(bool showTip = true)
        {
            bool useUnicode = !Console.IsOutputRedirected && 
                              Console.OutputEncoding.CodePage == 65001; // UTF-8
            
            // Check console width
            if (Console.WindowWidth < 50)
            {
                DisplaySmallLogo();
                return;
            }

            // Set colors
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;

            if (useUnicode)
                DisplayUnicode();
            else
                DisplayAscii();

            Console.ResetColor();

            if (showTip)
            {
                Console.WriteLine();
                ShowRandomTip();
            }
        }

        private static void DisplayUnicode()
        {
            string version = GetVersion();
            string buildDate = GetBuildDate();
            
            Console.WriteLine($@"
    ╔══════════════════════════════════════════════╗
    ║  ███████╗  ███╗   ███╗  ██╗██╗               ║
    ║  ██╔════╝  ████╗ ████║  ██║██║               ║
    ║  █████╗    ██╔████╔██║  ██║██║               ║
    ║  ██╔══╝    ██║╚██╔╝██║  ██║██║               ║
    ║  ██║       ██║ ╚═╝ ██║  ██║██║               ║
    ║  ╚═╝       ╚═╝     ╚═╝  ╚═╝╚═╝               ║
    ║                                              ║
    ║     F-- PROGRAMMING LANGUAGE                 ║
    ║        Version {version,-14}         ║
    ║        Build {buildDate,-12}            ║
    ║     Created by RealMG (13 tuổi)              ║
    ╚══════════════════════════════════════════════╝");
        }

        private static void DisplayAscii()
        {
            string version = GetVersion();
            
            Console.WriteLine($@"
    +--------------------------------------+
    |  F-- PROGRAMMING LANGUAGE            |
    |  Version {version,-10}                  |
    |  Created by RealMG (13 tuổi)          |
    +--------------------------------------+");
        }

        private static void DisplaySmallLogo()
        {
            Console.WriteLine("F-- v" + GetVersion());
            Console.WriteLine("Created by RealMG (13)");
        }

        private static string GetVersion()
        {
            try
            {
                var assembly = typeof(Logo).Assembly;
                var version = assembly.GetName().Version;
                if (version != null)
                {
                    return $"{version.Major}.{version.Minor}.{version.Build}";
                }
                
                var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (infoVersion != null)
                {
                    return infoVersion.InformationalVersion;
                }
            }
            catch { }
            
            return "2.0.0";
        }

        private static string GetBuildDate()
        {
            try
            {
                var assembly = typeof(Logo).Assembly;
                var fileInfo = new FileInfo(assembly.Location);
                return fileInfo.LastWriteTime.ToString("yyyy-MM-dd");
            }
            catch
            {
                return DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        private static void ShowRandomTip()
        {
            int index = _random.Next(Tips.Length);
            Console.WriteLine($"   {Tips[index]}");
        }

        public static void DisplayWithAnimation(int frameDelayMs = 30)
        {
            if (Console.WindowWidth < 50)
            {
                DisplaySmallLogo();
                return;
            }

            string[] frames = {
                @"
    ╔══════════════════════════════════════╗
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ╚══════════════════════════════════════╝",
                
                @"
    ╔══════════════════════════════════════╗
    ║  ███████╗                            ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ╚══════════════════════════════════════╝",
                
                @"
    ╔══════════════════════════════════════╗
    ║  ███████╗  ███╗   ███╗               ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ╚══════════════════════════════════════╝",
                
                @"
    ╔══════════════════════════════════════╗
    ║  ███████╗  ███╗   ███╗  ██╗██╗      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ╚══════════════════════════════════════╝",
                
                @"
    ╔══════════════════════════════════════╗
    ║  ███████╗  ███╗   ███╗  ██╗██╗      ║
    ║  ██╔════╝  ████╗ ████║  ██║██║      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ╚══════════════════════════════════════╝",
                
                @"
    ╔══════════════════════════════════════╗
    ║  ███████╗  ███╗   ███╗  ██╗██╗      ║
    ║  ██╔════╝  ████╗ ████║  ██║██║      ║
    ║  █████╗    ██╔████╔██║  ██║██║      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ╚══════════════════════════════════════╝",
                
                @"
    ╔══════════════════════════════════════╗
    ║  ███████╗  ███╗   ███╗  ██╗██╗      ║
    ║  ██╔════╝  ████╗ ████║  ██║██║      ║
    ║  █████╗    ██╔████╔██║  ██║██║      ║
    ║  ██╔══╝    ██║╚██╔╝██║  ██║██║      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ╚══════════════════════════════════════╝",
                
                @"
    ╔══════════════════════════════════════╗
    ║  ███████╗  ███╗   ███╗  ██╗██╗      ║
    ║  ██╔════╝  ████╗ ████║  ██║██║      ║
    ║  █████╗    ██╔████╔██║  ██║██║      ║
    ║  ██╔══╝    ██║╚██╔╝██║  ██║██║      ║
    ║  ██║       ██║ ╚═╝ ██║  ██║██║      ║
    ║                                      ║
    ║                                      ║
    ║                                      ║
    ╚══════════════════════════════════════╝",
                
                @"
    ╔══════════════════════════════════════╗
    ║  ███████╗  ███╗   ███╗  ██╗██╗      ║
    ║  ██╔════╝  ████╗ ████║  ██║██║      ║
    ║  █████╗    ██╔████╔██║  ██║██║      ║
    ║  ██╔══╝    ██║╚██╔╝██║  ██║██║      ║
    ║  ██║       ██║ ╚═╝ ██║  ██║██║      ║
    ║  ╚═╝       ╚═╝     ╚═╝  ╚═╝╚═╝      ║
    ║                                      ║
    ║                                      ║
    ╚══════════════════════════════════════╝",
                
                @"
    ╔══════════════════════════════════════╗
    ║  ███████╗  ███╗   ███╗  ██╗██╗      ║
    ║  ██╔════╝  ████╗ ████║  ██║██║      ║
    ║  █████╗    ██╔████╔██║  ██║██║      ║
    ║  ██╔══╝    ██║╚██╔╝██║  ██║██║      ║
    ║  ██║       ██║ ╚═╝ ██║  ██║██║      ║
    ║  ╚═╝       ╚═╝     ╚═╝  ╚═╝╚═╝      ║
    ║                                      ║
    ║     F-- PROGRAMMING LANGUAGE         ║
    ║                                      ║
    ╚══════════════════════════════════════╝"
            };

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;

            foreach (var frame in frames)
            {
                Console.Clear();
                Console.WriteLine(frame);
                System.Threading.Thread.Sleep(frameDelayMs);
            }

            Console.WriteLine($"        Version {GetVersion(),-14}");
            Console.WriteLine($"     Created by RealMG (13 tuổi)");
            Console.WriteLine($"    Build: {GetBuildDate()}");
            Console.WriteLine($"    ╚══════════════════════════════════════╝");

            Console.ResetColor();
            
            Console.WriteLine();
            ShowRandomTip();
        }
    }
}
