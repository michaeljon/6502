using System.Collections.Generic;
using System.Text.RegularExpressions;
using InnoWerks.Assemblers;
using InnoWerks.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class NotTests
    {
        public TestContext TestContext { get; set; }

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
        private void GenerateOpTable(Dictionary<byte, OpCodeDefinition> opCodeTable)
        {
            TestContext.WriteLine("\r");
            GenerateHeaderFooter();

            for (var row = 0; row <= 0x0f; row++)
            {
                TestContext.Write($"|  {row:x1}  |");
                for (var col = 0; col <= 0x0f; col++)
                {
                    var index = (byte)(row << 4 | col);
                    var ocd = opCodeTable[index];
                    var disp = ocd.OpCode != OpCode.Unknown ? ocd.OpCode.ToString() : "   ";
                    disp = disp.Substring(0, 3);

                    TestContext.Write(disp.Length == 3 ? $"   {disp}   " : $"   {disp}  ");

                    TestContext.Write("|");
                }

                TestContext.WriteLine("\r");

                TestContext.Write($"|     |");
                for (var col = 0; col <= 0x0f; col++)
                {
                    var opcode = (byte)(row << 4 | col);
                    var ocd = opCodeTable[opcode];

                    TestContext.Write($"{AddressModeLookup.GetDisplay(ocd.AddressingMode)}");
                    TestContext.Write("|");
                }

                TestContext.WriteLine("\r");
                GenerateSeparator();
            }

            GenerateHeaderFooter(true);
        }

        private void GenerateHeaderFooter(bool last = false)
        {
            if (last == false)
            {
                GenerateSeparator();
            }

            TestContext.Write("|     |");
            for (var col = 0; col <= 0x0f; col++)
            {
                TestContext.Write($"    {col:x1}    ");
                TestContext.Write("|");
            }
            TestContext.WriteLine("\r");

            GenerateSeparator();
        }

        private void GenerateSeparator()
        {
            TestContext.Write($"|-----|");
            for (var col = 0; col <= 0x0f; col++)
            {
                TestContext.Write($"---------");
                TestContext.Write("|");
            }
            TestContext.WriteLine("\r");
        }

        [TestMethod]
        public void RegexSwitch()
        {
            var commandRegex = new Regex(
                "^((?<command>t) (?<steps>[0-9]+))$?|" +
                "^((?<command>pc) (?<addr>[a-f0-9]{4}))$?|" +
                "^((?<command>jsr) (?<addr>[a-f0-9]{4}))$?|" +
                "^(?<command>sb (?<addr>[a-f0-9a-f]{4}))$?|" +
                "^(?<command>cb (?<addr>[a-f0-9]{4}))$?|" +
                "^(?<command>ca)$?|" +
                "^(?<command>lb)$?|" +
                "^(?<command>df)$?|" +
                "^(?<command>sf (?<flag>[cnvz]))$?|" +
                "^(?<command>cf (?<flag>[cnvz]))$?|" +
                "^(?<command>dr)$?|" +
                "^(?<command>sr (?<register>[axys]) (?<value>[a-f0-9]{1,2}))$?|" +
                "^(?<command>zr (?<register>[axys]))$?|" +
                "^(?<command>w (?<addr>[a-f0-9]{1,4}) (?<values>[a-f0-9]{1,2}( [a-f0-9]{1,2})*))$?|" +
                "^(?<command>r (?<addr>[a-f0-9]{1,4}) (?<len>[0-9]*))$?|" +
                "^(?<command>d (?<page>[a-f0-9]{1,2}))$?|" +
                "^(?<command>o ts (?<speed>[0-9]+))?|$" +
                "^(?<command>o tv (?<flag>(true|false)))$?|" +
                "^(?<command>(q|quit))$?|" +
                "^(?<command>g)$?|" +
                "^(?<command>s)$?|" +
                "^(?<command>(\\?|h))$?"
            );

            commandRegex.IsMatch("t 10");
            commandRegex.MatchNamedCaptures("t 10");
        }
    }
}
