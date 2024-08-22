using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
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

            if (options.PrintProgramText)
            {
                Console.WriteLine("------|-----------------|-------------------|-------|" + "".PadLeft(80, '-'));
                Console.WriteLine(
                    "{0,5} | {1,-15} | {2,-17} | {3,-5} | {4}",
                    "line",
                    "type",
                    "addr   : code",
                    "size",
                    "source code");
                Console.WriteLine("------|-----------------|-------------------|-------|" + "".PadLeft(80, '-'));

                foreach (var instruction in assembler.Program)
                {
                    Console.WriteLine(
                        "{0,5} | {1,-15} | ${2,-16} | {3,-5} | {4}",
                        instruction.LineNumber,
                        instruction.LineType,
                        instruction.EffectiveAddress.ToString("X4", CultureInfo.InvariantCulture) +
                            (instruction.MachineCode.Length == 0 ? "" : " : " + instruction.MachineCodeAsString),
                        instruction.EffectiveSize,
                        GetInstructionText(instruction));
                }
            }

            if (options.PrintSymbolTable)
            {
                Console.WriteLine("");
                Console.WriteLine("Symbols");
                if (assembler.SymbolTable.Count > 0)
                {
                    Console.WriteLine("----------|---------|-----------|------------|-------");
                    Console.WriteLine(
                        "{0,-10}| {1,-8}| {2,-10}| {3,-11}| {4,-6}",
                        "label",
                        "line",
                        "type",
                        "addr / val",
                        "size"
                    );
                    Console.WriteLine("----------|---------|-----------|------------|-------");

                    foreach (var (_, symbol) in assembler.SymbolTable.Where(s => s.Value.SymbolType == SymbolType.DefineByte || s.Value.SymbolType == SymbolType.DefineWord).OrderBy(s => s.Key))
                    {
                        PrintSymbol(symbol);
                    }

                    if (assembler.SymbolTable.Any(s => s.Value.SymbolType == SymbolType.DefineByte || s.Value.SymbolType == SymbolType.DefineWord))
                    {
                        Console.WriteLine();
                    }

                    foreach (var (_, symbol) in assembler.SymbolTable.Where(s => s.Value.SymbolType == SymbolType.AbsoluteAddress).OrderBy(s => s.Key))
                    {
                        PrintSymbol(symbol);
                    }
                }
            }

            var code = assembler.ObjectCode;

            Console.WriteLine("");
            Console.WriteLine("Complete");
            Console.WriteLine("Object size ${0:X4}", code.Length);

            File.WriteAllBytes(options.Output, code);

            return 0;
        }

        private static string GetInstructionText(LineInformation instruction)
        {
            switch (instruction.LineType)
            {
                case LineType.Code:
                    if (instruction.ApplicableOffset != 0)
                    {
                        return string.Format(
                            CultureInfo.InvariantCulture,
                            "{0,-10}{1,-6}{2,-10}{3}",
                            instruction.Label ?? "",
                            instruction.OpCode != OpCode.Unknown ? instruction.OpCode.ToString() : "",
                            (InstructionInformation.BranchingOperations.Contains(instruction.OpCode) ?
                                instruction.ExtractedArgument :
                                instruction.RawArgumentWithReplacement) + (instruction.ApplicableOffset < 0 ? "-" : "+") + Math.Abs(instruction.ApplicableOffset).ToString(CultureInfo.InvariantCulture),
                            string.IsNullOrEmpty(instruction.Comment) ? "" : "; " + instruction.Comment);
                    }
                    else
                    {
                        return string.Format(
                            CultureInfo.InvariantCulture,
                            "{0,-10}{1,-6}{2,-10}{3}",
                            instruction.Label ?? "",
                            instruction.OpCode != OpCode.Unknown ? instruction.OpCode.ToString() : "",
                            InstructionInformation.BranchingOperations.Contains(instruction.OpCode) ?
                                instruction.ExtractedArgument :
                                instruction.RawArgumentWithReplacement,
                            string.IsNullOrEmpty(instruction.Comment) ? "" : "; " + instruction.Comment);
                    }

                case LineType.Comment:
                    return "* " + instruction.Comment;

                case LineType.FloatingComment:
                    return string.IsNullOrEmpty(instruction.Comment) ? "" : "                          ; " + instruction.Comment;

                case LineType.Data:
                case LineType.Directive:
                    return string.Format(
                        CultureInfo.InvariantCulture,
                        "{0,-10}{1,-6}{2,-10}{3}",
                        instruction.Label ?? "",
                        instruction.Directive.ToString(),
                        instruction.Value,
                        string.IsNullOrEmpty(instruction.Comment) ? "" : "; " + instruction.Comment);

                case LineType.Equivalence:
                    return string.Format(
                        CultureInfo.InvariantCulture,
                        "{0,-10}{1,-6}{2,-10}{3}",
                        instruction.Label ?? "",
                        instruction.Directive,
                        instruction.Value,
                        string.IsNullOrEmpty(instruction.Comment) ? "" : "; " + instruction.Comment);

                case LineType.Empty:
                    return "";

                case LineType.Label:
                    return string.Format(
                        CultureInfo.InvariantCulture,
                        "{0,-10}{1,-6}{2,-10}{3}",
                        instruction.Label ?? "",
                        "",
                        "",
                        string.IsNullOrEmpty(instruction.Comment) ? "" : "; " + instruction.Comment);
            }

            return "*****";
        }

        private static void PrintSymbol(Symbol symbol)
        {
            Console.WriteLine(
                "{0,-10}| {1,-8}| {2,-10}| {3,-11}| {4,-6}",
                symbol.Label,
                symbol.LineNumber,
                symbol.SymbolType switch
                {
                    SymbolType.DefineByte => $"Equate",
                    SymbolType.DefineWord => $"Equate",
                    SymbolType.AbsoluteAddress => $"Label",
                    _ => ""
                },
                symbol.SymbolType switch
                {
                    SymbolType.DefineByte => $"${symbol.Value:X2}",
                    SymbolType.DefineWord => $"${symbol.Value:X4}",
                    SymbolType.AbsoluteAddress => $"${symbol.Value:X4}",
                    _ => ""
                },
                symbol.Size
            );
        }
    }
}
