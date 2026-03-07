using System;
using Fminusminus.Main.Commands;
using Fminusminus.Main.UI;

namespace Fminusminus.Main
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Logo.Display();

            if (args.Length == 0)
            {
                HelpCommand.Show();
                return 1;
            }

            string command = args[0].ToLower();
            
            try
            {
                return command switch
                {
                    "run" => RunCommand.Execute(args),
                    "ast" => AstCommand.Execute(args),
                    "codegen" => CodeGenCommand.Execute(args),
                    "compile" => CompileCommand.Execute(args),  // 👈 MỚI
                    "--version" or "-v" => VersionCommand.Execute(),
                    "--help" or "-h" => HelpCommand.Execute(),
                    _ => RunCommand.Execute(new[] { "run", args[0] })
                };
            }
            catch (AggregateException ex)
            {
                ErrorHandler.DisplayMultiple(ex);
                return 1;
            }
            catch (Exception ex)
            {
                ErrorHandler.Display(ex);
                return 1;
            }
        }
    }
}
