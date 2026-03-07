using System;

namespace Fminusminus.Main.Commands
{
    /// <summary>
    /// Handle help display
    /// </summary>
    public static class HelpCommand
    {
        public static int Execute(string[]? args = null)
        {
            Show();
            return 0;
        }

        public static void Show()
        {
            Console.WriteLine("F-- Programming Language - Usage Guide");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("📌 COMMANDS:");
            Console.WriteLine();
            Console.WriteLine("  run <file>           Run an F-- program");
            Console.WriteLine("  ast <file>           Display Abstract Syntax Tree");
            Console.WriteLine("  codegen <file> [target] [opt]  Generate code for other platforms");
            Console.WriteLine("  --version, -v        Show version information");
            Console.WriteLine("  --help, -h           Show this help message");
            Console.WriteLine();
            Console.WriteLine("🎯 CODEGEN TARGETS:");
            Console.WriteLine("  cil        .NET Common Intermediate Language (default)");
            Console.WriteLine("  c          C programming language");
            Console.WriteLine("  js         JavaScript");
            Console.WriteLine("  py         Python");
            Console.WriteLine("  f--        F-- itself (self-hosting)");
            Console.WriteLine();
            Console.WriteLine("⚡ OPTIMIZATION LEVELS:");
            Console.WriteLine("  o0         No optimization");
            Console.WriteLine("  o1         Basic optimizations (default)");
            Console.WriteLine("  o2         Aggressive optimizations");
            Console.WriteLine("  o3         Maximum optimizations");
            Console.WriteLine();
            Console.WriteLine("📋 EXAMPLES:");
            Console.WriteLine("  fminus run examples/hello.f--");
            Console.WriteLine("  fminus ast examples/hello.f--");
            Console.WriteLine("  fminus codegen examples/hello.f-- c");
            Console.WriteLine("  fminus codegen examples/hello.f-- js o2");
            Console.WriteLine("  fminus codegen examples/hello.f-- py");
            Console.WriteLine();
            Console.WriteLine("📁 PROJECT STRUCTURE:");
            Console.WriteLine("  Compiler/     Source code");
            Console.WriteLine("  examples/     Example programs");
            Console.WriteLine("  docs/         Documentation");
            Console.WriteLine();
            Console.WriteLine("🌐 RESOURCES:");
            Console.WriteLine("  GitHub: https://github.com/realmg51-cpu/F--Programming-Language");
            Console.WriteLine("  NuGet: https://www.nuget.org/packages/Fminusminus");
            Console.WriteLine("  Discord: https://discord.gg/fminus (coming soon)");
        }
    }
}
