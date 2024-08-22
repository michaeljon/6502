using System;
using System.IO;
using CommandLine;
using InnoWerks.Assemblers;

namespace Asm6502
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CliOptions>(args);

            result.MapResult(
                RunAssembler,

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

        private static int RunAssembler(CliOptions options)
        {
            if (string.IsNullOrEmpty(options.Output))
            {
                options.Output = Path.ChangeExtension(options.Input, ".o");
            }

            var inputLines = File.ReadAllLines(options.Input);

            var assembler = new Assembler(inputLines, 0x8000);

            try
            {
                assembler.Assemble();
            }
            catch (SymbolRedefinedException sre)
            {
                Console.Error.WriteLine(sre.Message);
                return 1;
            }
            catch (IndexerException ie)
            {
                Console.Error.WriteLine(ie.Message);
                return 1;
            }
            catch
            {
                Console.Error.WriteLine("Something else happened...");
                return 1;
            }

            if (options.Debug)
            {
                Console.WriteLine("");
                Console.WriteLine("Program statements");
                foreach (var line in assembler.Program)
                {
                    Console.WriteLine($"{line.LineNumber,-10} => {line.ToString().Replace("\n", " ", StringComparison.OrdinalIgnoreCase).Replace("\t", "", StringComparison.OrdinalIgnoreCase)}");
                }
            }

            if (options.Verbose)
            {
                Console.WriteLine("");
                Console.WriteLine("Symbol table");
                if (assembler.SymbolTable.Count > 0)
                {
                    foreach (var (label, symbol) in assembler.SymbolTable)
                    {
                        Console.WriteLine($"{label,-10} => {symbol.ToString().Replace("\n", " ", StringComparison.OrdinalIgnoreCase).Replace("\t", "", StringComparison.OrdinalIgnoreCase)}");
                    }
                }
            }

            var code = assembler.GenerateBytes();

            Console.WriteLine("");
            Console.WriteLine("Complete");
            Console.WriteLine("Object size ${0:X4}", code.Length);

            File.WriteAllBytes(options.Output, code);

            return 0;
        }
    }
}
