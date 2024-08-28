using System;
using InnoWerks.Processors.Common;

namespace InnoWerks.Assemblers.Tests
{
    [TestClass]
    public class NotTests
    {
        [TestMethod]
        public void GenerateOpTable()
        {
            var instructions = new (OpCode opCode, AddressingMode addressingMode)[256];
            foreach (var (k, v) in InstructionInformation.Instructions)
            {
                instructions[v.code] = k;
            }

            GenerateHeaderFooter();

            for (var row = 0; row <= 0x0f; row++)
            {
                Console.Write($"|  {row:x1}  |");
                for (var col = 0; col <= 0x0f; col++)
                {
                    var index = (byte)(row << 4 | col);
                    var (opCode, _) = instructions[index];
                    var disp = opCode != OpCode.Unknown ? opCode.ToString() : "   ";

                    Console.Write(disp.Length == 3 ? $"   {disp}   " : $"   {disp}  ");
                    Console.Write("|");
                }

                Console.WriteLine();

                Console.Write($"|     |");
                for (var col = 0; col <= 0x0f; col++)
                {
                    var index = (byte)(row << 4 | col);
                    var (_, addressingMode) = instructions[index];

                    Console.Write($"{AddressModeLookup.GetDisplay(addressingMode)}");
                    Console.Write("|");
                }

                Console.WriteLine();
                GenerateSeparator();
            }

            GenerateHeaderFooter(true);
        }

        private static void GenerateHeaderFooter(bool last = false)
        {
            if (last == false)
            {
                GenerateSeparator();
            }

            Console.Write("|     |");
            for (var col = 0; col <= 0x0f; col++)
            {
                Console.Write($"    {col:x1}    ");
                Console.Write("|");
            }
            Console.WriteLine();

            GenerateSeparator();
        }

        private static void GenerateSeparator()
        {
            Console.Write($"|-----|");
            for (var col = 0; col <= 0x0f; col++)
            {
                Console.Write($"---------");
                Console.Write("|");
            }
            Console.WriteLine();
        }
    }
}
