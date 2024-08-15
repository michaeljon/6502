using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    /// <summary>
    /// These tests execute the snippits that are in
    /// http://www.6502.org/tutorials/decimal_mode.html
    /// </summary>
    [TestClass]
    public class BruceClarkTinyTests : TestBase
    {
        [TestMethod]
        public void BruceClarkExampleTestA()
        {
            // F8      SED                  ; Decimal mode
            // A9 05   LDA   #$05
            // 18      CLC
            // 69 05   ADC   #$05
            // 00      DB    0              ; the accumulator is $10

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x05;
            memory[addr++] = 0x18;
            memory[addr++] = 0x69;
            memory[addr++] = 0x05;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.AreEqual(0x10, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExampleTestB()
        {
            // F8      SED                  ; Decimal mode (has no effect on this sequence)
            // A9 05   LDA   #$05
            // 0A      ASL
            // 00      DB    0              ; the accumulator is $0A

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x05;
            memory[addr++] = 0x0A;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.AreEqual(0x0A, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExampleTestC()
        {
            // F8      SED                  ; Decimal mode
            // A9 09   LDA   #$09
            // 18      CLC
            // 69 01   ADC   #$01
            // 00      DB    0              ; the accumulator is $10

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x09;
            memory[addr++] = 0x18;
            memory[addr++] = 0x69;
            memory[addr++] = 0x01;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.AreEqual(0x10, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExampleTestD()
        {
            // F8      SED                  ; Decimal mode (has no effect on this sequence)
            // A9 09   LDA   #$09
            // 85 E0   STA   {$E0}
            // E6 E0   INC   {$E0}
            // 00      DB    0              ; NUM (assuming it is an ordinary RAM location) will contain $0A.

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x09;
            memory[addr++] = 0x85;
            memory[addr++] = 0xE0;
            memory[addr++] = 0xE6;
            memory[addr++] = 0xE0;
            memory[addr++] = 0x00;

            RunTinyTest(memory, CpuClass.WDC6502);
            Assert.AreEqual(0x0a, memory[0xe0]);
        }

        [TestMethod]
        public void BruceClarkExample1()
        {
            // D8      CLD                  ; Binary mode (binary addition: 88 + 70 + 1 = 159)
            // 38      SEC                  ; Note: carry is set, not clear!
            // A9 58   LDA   #$58           ; 88
            // 69 46   ADC   #$46           ; 70 (after this instruction, C = 0, A = $9F = 159)
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xD8;
            memory[addr++] = 0x38;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x58;
            memory[addr++] = 0x69;
            memory[addr++] = 0x46;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsFalse(cpu.Registers.Carry);
            Assert.AreEqual(0x9f, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExample2()
        {
            // F8      SED                  ; Decimal mode (BCD addition: 58 + 46 + 1 = 105)
            // 38      SEC                  ; Note: carry is set, not clear!
            // A9 58   LDA   #$58
            // 69 46   ADC   #$46           ; After this instruction, C = 1, A = $05
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x38;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x58;
            memory[addr++] = 0x69;
            memory[addr++] = 0x46;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsTrue(cpu.Registers.Carry);
            Assert.AreEqual(0x05, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExample3()
        {
            // F8      SED                  ; Decimal mode (BCD addition: 12 + 34 = 46)
            // 18      CLC
            // A9 12   LDA   #$12
            // 69 34   ADC   #$34           ; After this instruction, C = 0, A = $46
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x18;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x12;
            memory[addr++] = 0x69;
            memory[addr++] = 0x34;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsFalse(cpu.Registers.Carry);
            Assert.AreEqual(0x46, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExample4()
        {
            // F8      SED                  ; Decimal mode (BCD addition: 15 + 26 = 41)
            // 18      CLC
            // A9 15   LDA   #$15
            // 69 26   ADC   #$26           ; After this instruction, C = 0, A = $41
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x18;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x15;
            memory[addr++] = 0x69;
            memory[addr++] = 0x26;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsFalse(cpu.Registers.Carry);
            Assert.AreEqual(0x41, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExample5()
        {
            // F8      SED                  ; Decimal mode (BCD addition: 81 + 92 = 173)
            // 18      CLC
            // A9 81   LDA   #$81
            // 69 92   ADC   #$92           ; After this instruction, C = 1, A = $73
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x18;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x81;
            memory[addr++] = 0x69;
            memory[addr++] = 0x92;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsTrue(cpu.Registers.Carry);
            Assert.AreEqual(0x73, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExample6()
        {
            // F8      SED                  ; Decimal mode (BCD subtraction: 46 - 12 = 34)
            // 38      SEC
            // A9 46   LDA   #$46
            // E9 12   SBC   #$12           ; After this instruction, C = 1, A = $34)
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x38;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x46;
            memory[addr++] = 0xE9;
            memory[addr++] = 0x12;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsTrue(cpu.Registers.Carry);
            Assert.AreEqual(0x34, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExample7()
        {
            // F8      SED                  ; Decimal mode (BCD subtraction: 40 - 13 = 27)
            // 38      SEC
            // A9 40   LDA   #$40
            // E9 13   SBC   #$13           ; After this instruction, C = 1, A = $27)
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x38;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x40;
            memory[addr++] = 0xE9;
            memory[addr++] = 0x13;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsTrue(cpu.Registers.Carry);
            Assert.AreEqual(0x27, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExample8()
        {
            // F8      SED                  ; Decimal mode (BCD subtraction: 32 - 2 - 1 = 29)
            // 18      CLC                  ; Note: carry is clear, not set!
            // A9 32   LDA   #$32
            // E9 02   SBC   #$02           ; After this instruction, C = 1, A = $29)
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x18;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x32;
            memory[addr++] = 0xE9;
            memory[addr++] = 0x02;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsTrue(cpu.Registers.Carry);
            Assert.AreEqual(0x29, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExample9()
        {
            // F8      SED                  ; Decimal mode (BCD subtraction: 12 - 21)
            // 38      SEC
            // A9 12   LDA   #$12
            // E9 21   SBC   #$21           ; After this instruction, C = 0, A = $91)
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x38;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x12;
            memory[addr++] = 0xE9;
            memory[addr++] = 0x21;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC65C02);
            Assert.IsFalse(cpu.Registers.Carry);
            Assert.AreEqual(0x91, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExample10()
        {
            // F8      SED                  ; Decimal mode (BCD subtraction: 21 - 34)
            // 38      SEC
            // A9 21   LDA   #$21
            // E9 34   SBC   #$34           ; After this instruction, C = 0, A = $87)
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x38;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x21;
            memory[addr++] = 0xE9;
            memory[addr++] = 0x34;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC65C02);
            Assert.IsFalse(cpu.Registers.Carry);
            Assert.AreEqual(0x87, cpu.Registers.A);
        }

        [TestMethod]
        public void BruceClarkExample13()
        {
            // F8      SED                  ; Decimal mode
            // 18      CLC
            // A9 90   LDA   #$90
            // 69 90   ADC   #$90

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x18;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x90;
            memory[addr++] = 0x69;
            memory[addr++] = 0x90;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsTrue(cpu.Registers.Overflow);
        }

        [TestMethod]
        public void BruceClarkExample14()
        {
            // F8      SED                  ; Decimal mode
            // 38      SEC
            // A9 01   LDA   #$01
            // E9 01   SBC   #$01           ; expect A = 0, Z = 1
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x38;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x01;
            memory[addr++] = 0xE9;
            memory[addr++] = 0x01;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsTrue(cpu.Registers.Zero);
            Assert.AreEqual(0x00, cpu.Registers.A);
        }

        [TestMethod]
        public void AppendixA1()
        {
            // D8      CLD                  ; binary mode: 99 + 1
            // 18      CLC
            // A9 99   LDA   #$99
            // 69 01   ADC   #$01

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xD8;
            memory[addr++] = 0x18;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x99;
            memory[addr++] = 0x69;
            memory[addr++] = 0x01;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.AreEqual(0x9a, cpu.Registers.A);
            Assert.IsFalse(cpu.Registers.Zero);
        }

        [TestMethod]
        public void AppendixA2()
        {
            // F8      SED                  ; decimal mode: 99 + 1
            // 18      CLC
            // A9 99   LDA   #$99
            // 69 01   ADC   #$01

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x18;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x99;
            memory[addr++] = 0x69;
            memory[addr++] = 0x01;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC65C02);
            Assert.AreEqual(0x00, cpu.Registers.A);
            Assert.IsTrue(cpu.Registers.Zero);
        }

        [TestMethod]
        public void BruceClarkExample99()
        {
            // F8      SED
            // 38      SEC
            // A9 00   LDA   #$00
            // E9 00   SBC   #$00

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xF8;
            memory[addr++] = 0x38;
            memory[addr++] = 0xA9;
            memory[addr++] = 0x00;
            memory[addr++] = 0xE9;
            memory[addr++] = 0x00;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.IsTrue(cpu.Registers.Zero);
            Assert.AreEqual(0x00, cpu.Registers.A);
        }

        [DataTestMethod]
        [DataRow((byte)0x01, (byte)0x01, (byte)0x02, false, false, false)]
        [DataRow((byte)0x01, (byte)0xFF, (byte)0x00, true, true, false)]
        [DataRow((byte)0x7f, (byte)0x01, (byte)0x80, false, false, true)]
        [DataRow((byte)0x80, (byte)0xff, (byte)0x7f, true, false, true)]
        public void ClcAdcOverflowFlagTests(byte xx, byte yy, byte accum, bool c, bool z, bool v)
        {
            // D8      CLD
            // 18      CLC
            // A9 xx   LDA   #$xx
            // 69 yy   ADC   #$yy

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xD8;
            memory[addr++] = 0x18;
            memory[addr++] = 0xA9;
            memory[addr++] = xx;
            memory[addr++] = 0x69;
            memory[addr++] = yy;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.AreEqual(c, cpu.Registers.Carry);
            Assert.AreEqual(z, cpu.Registers.Zero);
            Assert.AreEqual(v, cpu.Registers.Overflow);

            Assert.AreEqual(accum, cpu.Registers.A);
        }

        [DataTestMethod]
        [DataRow((byte)0x00, (byte)0x01, (byte)0xff, false, false, false)]
        [DataRow((byte)0x80, (byte)0x01, (byte)0x7f, true, false, true)]
        [DataRow((byte)0x7f, (byte)0xff, (byte)0x80, false, false, true)]
        public void SecSbcOverflowFlagTests(byte xx, byte yy, byte accum, bool c, bool z, bool v)
        {
            // D8      CLD
            // 38      SEC
            // A9 xx   LDA   #$xx
            // E9 yy   SBC   #$yy

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xD8;
            memory[addr++] = 0x38;
            memory[addr++] = 0xA9;
            memory[addr++] = xx;
            memory[addr++] = 0xE9;
            memory[addr++] = yy;

            var cpu = RunTinyTest(memory, CpuClass.WDC65C02);
            Assert.AreEqual(c, cpu.Registers.Carry);
            Assert.AreEqual(z, cpu.Registers.Zero);
            Assert.AreEqual(v, cpu.Registers.Overflow);

            Assert.AreEqual(accum, cpu.Registers.A);
        }

        [DataTestMethod]
        [DataRow((byte)0x3f, (byte)0x40, (byte)0x80, false, false, true)]
        public void SecAdcOverflowFlagTests(byte xx, byte yy, byte accum, bool c, bool z, bool v)
        {
            // D8      CLD
            // 38      SEC
            // A9 xx   LDA   #$xx
            // 69 yy   ADC   #$yy

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xD8;
            memory[addr++] = 0x38;
            memory[addr++] = 0xA9;
            memory[addr++] = xx;
            memory[addr++] = 0x69;
            memory[addr++] = yy;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.AreEqual(c, cpu.Registers.Carry);
            Assert.AreEqual(z, cpu.Registers.Zero);
            Assert.AreEqual(v, cpu.Registers.Overflow);

            Assert.AreEqual(accum, cpu.Registers.A);
        }

        [DataTestMethod]
        [DataRow((byte)0xc0, (byte)0x40, (byte)0x7f, true, false, true)]
        public void ClcSbcOverflowFlagTests(byte xx, byte yy, byte accum, bool c, bool z, bool v)
        {
            // D8      CLD
            // 18      CLC
            // A9 xx   LDA   #$xx
            // E9 yy   SBC   #$yy

            byte[] memory = new byte[1024 * 64];
            byte addr = 0x00;

            memory[addr++] = 0xD8;
            memory[addr++] = 0x18;
            memory[addr++] = 0xA9;
            memory[addr++] = xx;
            memory[addr++] = 0xE9;
            memory[addr++] = yy;

            var cpu = RunTinyTest(memory, CpuClass.WDC6502);
            Assert.AreEqual(c, cpu.Registers.Carry);
            Assert.AreEqual(z, cpu.Registers.Zero);
            Assert.AreEqual(v, cpu.Registers.Overflow);

            Assert.AreEqual(accum, cpu.Registers.A);
        }
    }
}
