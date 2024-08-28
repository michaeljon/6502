using System;
using System.Collections.Generic;
using InnoWerks.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class NotTests
    {
        [TestMethod]
        public void Generate6502OpCodeTable()
        {
            GenerateOpTable(CpuInstructions.OpCode6502);
        }

        [TestMethod]
        public void Generate65C02OpCodeTable()
        {
            GenerateOpTable(CpuInstructions.OpCode65C02);
        }
        private static void GenerateOpTable(Dictionary<byte, OpCodeDefinition> opCodeTable)
        {
            GenerateHeaderFooter();

            for (var row = 0; row <= 0x0f; row++)
            {
                Console.Write($"|  {row:x1}  |");
                for (var col = 0; col <= 0x0f; col++)
                {
                    var index = (byte)(row << 4 | col);
                    var ocd = opCodeTable[index];
                    var disp = ocd.OpCode != OpCode.Unknown ? ocd.OpCode.ToString() : "   ";

                    Console.Write(disp.Length == 3 ? $"   {disp}   " : $"   {disp}  ");

                    Console.Write("|");
                }

                Console.WriteLine();

                Console.Write($"|     |");
                for (var col = 0; col <= 0x0f; col++)
                {
                    var opcode = (byte)(row << 4 | col);
                    var ocd = opCodeTable[opcode];

                    Console.Write($"{AddressModeLookup.GetDisplay(ocd.AddressingMode)}");
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
