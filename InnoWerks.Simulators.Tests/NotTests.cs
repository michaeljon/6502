using System.Collections.Generic;
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

        [TestMethod]
        public void LineGeneratorWorks()
        {
            // top 1/3
            Assert.AreEqual(0x0400, GenerateBaseLineAddress(0));
            Assert.AreEqual(0x0480, GenerateBaseLineAddress(1));
            Assert.AreEqual(0x0500, GenerateBaseLineAddress(2));
            Assert.AreEqual(0x0580, GenerateBaseLineAddress(3));
            Assert.AreEqual(0x0600, GenerateBaseLineAddress(4));
            Assert.AreEqual(0x0680, GenerateBaseLineAddress(5));
            Assert.AreEqual(0x0700, GenerateBaseLineAddress(6));
            Assert.AreEqual(0x0780, GenerateBaseLineAddress(7));

            // middle 1/3
            Assert.AreEqual(0x0428, GenerateBaseLineAddress(8));
            Assert.AreEqual(0x04a8, GenerateBaseLineAddress(9));
            Assert.AreEqual(0x0528, GenerateBaseLineAddress(10));
            Assert.AreEqual(0x05a8, GenerateBaseLineAddress(11));
            Assert.AreEqual(0x0628, GenerateBaseLineAddress(12));
            Assert.AreEqual(0x06a8, GenerateBaseLineAddress(13));
            Assert.AreEqual(0x0728, GenerateBaseLineAddress(14));
            Assert.AreEqual(0x07a8, GenerateBaseLineAddress(15));

            // bottom 1/3
            Assert.AreEqual(0x0450, GenerateBaseLineAddress(16));
            Assert.AreEqual(0x04d0, GenerateBaseLineAddress(17));
            Assert.AreEqual(0x0550, GenerateBaseLineAddress(18));
            Assert.AreEqual(0x05d0, GenerateBaseLineAddress(19));
            Assert.AreEqual(0x0650, GenerateBaseLineAddress(20));
            Assert.AreEqual(0x06d0, GenerateBaseLineAddress(21));
            Assert.AreEqual(0x0750, GenerateBaseLineAddress(22));
            Assert.AreEqual(0x07d0, GenerateBaseLineAddress(23));
        }

        /*
            F847: 48        141  GBASCALC PHA
            F848: 4A        142           LSR
            F849: 29 03     143           AND   #$03
            F84B: 09 04     144           ORA   #$04
            F84D: 85 27     145           STA   GBASH
            F84F: 68        146           PLA
            F850: 29 18     147           AND   #$18
            F852: 90 02     148           BCC   GBCALC
            F854: 69 7F     149           ADC   #$7F
            F856: 85 26     150  GBCALC   STA   GBASL
            F858: 0A        151           ASL
            F859: 0A        152           ASL
            F85A: 05 26     153           ORA   GBASL
            F85C: 85 26     154           STA   GBASL
            F85E: 60        155           RTS
        */
        private static ushort GenerateBaseLineAddress(int lineNumber)
        {
            int gbash = ((lineNumber >> 1) & 0x03) | 0x04; // | 0x05 for p.2
            int gbasl = lineNumber & 0x18;
            if ((lineNumber & 0x01) == 0x01)
            {
                gbasl += 0x80;
            }
            gbasl = ((gbasl << 2) | gbasl) & 0xff;

            return (ushort)((gbash << 8) | gbasl);
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
    }
}
