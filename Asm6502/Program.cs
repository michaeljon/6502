using System;
using System.IO;

namespace Asm6502
{
    partial class Program
    {
        static void Main(string[] args)
        {
            // var inputLines = File.ReadAllLines("./tests/mini.S");
            string[] inputLines = [
                "ADDR   EQ  $22",
                "       AND ADDR",
                "       AND $20",
                "W      DW  $2021",
                "B      DB  $20",
            ];

            var assembler = new Assembler(inputLines, 0x8000);
            assembler.Assemble();

            if (assembler.SymbolTable.Count > 0)
            {
                Console.WriteLine("Symbol table");

                foreach (var (label, symbol) in assembler.SymbolTable)
                {
                    Console.WriteLine($"{label,-10} => {symbol.ToString().Replace("\n", " ").Replace("\t", "")}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Program statements");

            foreach (var line in assembler.Program)
            {
                Console.WriteLine($"{line.LineNumber,10} => {line.ToString().Replace("\n", " ").Replace("\t", "")}");
            }
        }
    }
}