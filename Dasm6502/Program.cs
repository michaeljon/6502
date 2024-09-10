using System;
using System.IO;
using CommandLine;
using InnoWerks.Disassemblers;

namespace Dasm6502
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CliOptions>(args);

            result.MapResult(
                RunDisassembler,

                errors =>
                {
                    foreach (var error in errors)
                    {
                        Console.Error.WriteLine(error.ToString());
                    }

                    return 1;
                }
            );
        }

        private static int RunDisassembler(CliOptions options)
        {
            if (string.IsNullOrEmpty(options.Output))
            {
                options.Output = Path.ChangeExtension(options.Input, ".s");
            }

            var disassembler = new Disassembler(options.Input, options.Origin);

            try
            {
                disassembler.Disassemble();
            }
            catch
            {
                Console.Error.WriteLine("Something else happened...");
                return 1;
            }

            if (options.Output == "-")
            {
                foreach (var line in disassembler.Disassembly)
                {
                    Console.WriteLine(line);
                }
            }
            else
            {
                File.WriteAllLines(options.Output, disassembler.Disassembly);
            }

            return 0;
        }
    }
}
