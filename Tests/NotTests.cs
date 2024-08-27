using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class NotTests
    {
        private static readonly Dictionary<AddressingMode, string> addressModeLookup = new()
        {
            { AddressingMode.Unknown,                   "         " },
            { AddressingMode.Implied,                   "    i    " },
            { AddressingMode.Accumulator,               "    A    " },
            { AddressingMode.Immediate,                 "    #    " },
            { AddressingMode.Absolute,                  "    a    " },
            { AddressingMode.ZeroPage,                  "    zp   " },
            { AddressingMode.Stack,                     "    s    " },
            { AddressingMode.AbsoluteXIndexed,          "   a,x   " },
            { AddressingMode.AbsoluteYIndexed,          "   a,y   " },
            { AddressingMode.ZeroPageXIndexed,          "   zp,x  " },
            { AddressingMode.ZeroPageYIndexed,          "   zp,y  " },
            { AddressingMode.Relative,                  "    r    " },
            { AddressingMode.ZeroPageIndirect,          "  (zp)   " },
            { AddressingMode.AbsoluteIndexedIndirect,   "   (a,x) " },
            { AddressingMode.XIndexedIndirect,          "  (zp,x) " },
            { AddressingMode.IndirectYIndexed,          "  (zp),y " },
            { AddressingMode.AbsoluteIndirect,          "   (a)   " },
        };

        [TestMethod]
        public void Generate6502OpCodeTable()
        {
            GenerateOpTable(OpCodes.OpCode6502);
        }

        [TestMethod]
        public void Generate65C02OpCodeTable()
        {
            GenerateOpTable(OpCodes.OpCode65C02);
        }
        private static void GenerateOpTable(Dictionary<byte, OpCodeDefinition> opCodeTable)
        {
            GenerateHeaderFooter();

            for (var row = 0; row <= 0x0f; row++)
            {
                Console.Write($"|  {row:x1}  |");
                for (var col = 0; col <= 0x0f; col++)
                {
                    var opcode = (byte)(row << 4 | col);
                    var ocd = opCodeTable[opcode];

                    Console.Write($"   {ocd.Nmemonic ?? "   "}   ");
                    Console.Write("|");
                }

                Console.WriteLine();

                Console.Write($"|     |");
                for (var col = 0; col <= 0x0f; col++)
                {
                    var opcode = (byte)(row << 4 | col);
                    var ocd = opCodeTable[opcode];

                    Console.Write($"{addressModeLookup[ocd.AddressingMode]}");
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
