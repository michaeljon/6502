using System;
using System.IO;
using InnoWerks.Assemblers;

namespace Asm6502
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            AssemblerRunner();
        }

        private static void AssemblerRunner()
        {
            var inputLines = File.ReadAllLines("./tests/BruceClark6502_All.S");
            // string[] inputLines = [
            //     "N2H      EQU   $0F",
            //     "         STA   N2H+1"
            // ];

            var assembler = new Assembler(inputLines, 0x8000);
            var program = assembler.Assemble();

            Console.WriteLine("Program statements");
            foreach (var line in assembler.Program)
            {
                Console.WriteLine($"{line.LineNumber,-10} => {line.ToString().Replace("\n", " ", StringComparison.OrdinalIgnoreCase).Replace("\t", "", StringComparison.OrdinalIgnoreCase)}");
            }

            if (assembler.SymbolTable.Count > 0)
            {
                Console.WriteLine("Symbol table");

                foreach (var (label, symbol) in assembler.SymbolTable)
                {
                    Console.WriteLine($"{label,-10} => {symbol.ToString().Replace("\n", " ", StringComparison.OrdinalIgnoreCase).Replace("\t", "", StringComparison.OrdinalIgnoreCase)}");
                }
            }

            File.WriteAllBytes("./tests/BruceClark6502_All.o", program);
        }
    }
}
