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
            memory[0x00] = 0xF8;
            memory[0x01] = 0xA9;
            memory[0x02] = 0x05;
            memory[0x03] = 0x18;
            memory[0x04] = 0x69;
            memory[0x05] = 0x05;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.AreEqual(0x10, cpu.A);
        }

        [TestMethod]
        public void BruceClarkExampleTestB()
        {
            // F8      SED                  ; Decimal mode (has no effect on this sequence)
            // A9 05   LDA   #$05
            // 0A      ASL
            // 00      DB    0              ; the accumulator is $0A

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xF8;
            memory[0x01] = 0xA9;
            memory[0x02] = 0x05;
            memory[0x03] = 0x0A;
            memory[0x04] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.AreEqual(0x0A, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0xA9;
            memory[0x02] = 0x09;
            memory[0x03] = 0x18;
            memory[0x04] = 0x69;
            memory[0x05] = 0x01;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.AreEqual(0x10, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0xA9;
            memory[0x02] = 0x09;
            memory[0x03] = 0x85;
            memory[0x04] = 0xE0;
            memory[0x05] = 0xE6;
            memory[0x06] = 0xE0;
            memory[0x07] = 0x00;

            RunTinyTest(memory);
            Assert.AreEqual(0x0a, memory[0xe0]);
        }

        [TestMethod]
        public void FullBinaryModeLoopWithoutCarry()
        {
            // D8      CLD                  ; Binary mode
            // 18      CLC                  ; Note: carry is clear!
            // A9 xx   LDA   #$xx           ; a
            // 69 xx   ADC   #$xx           ; b
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xD8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x00;    // a
            memory[0x04] = 0x69;
            memory[0x05] = 0x00;    // b
            memory[0x06] = 0x00;

            for (var a = 0x00; a < 256; a++)
            {
                for (var b = 0x00; b < 256; b++)
                {
                    memory[0x03] = (byte)a;
                    memory[0x05] = (byte)b;

                    var cpu = RunTinyTest(memory);

                    ushort expected = (ushort)(a + b);
                    bool carry = expected > 0xff;

                    Assert.AreEqual(carry, cpu.Carry);
                    Assert.AreEqual((byte)expected, cpu.A);
                }
            }
        }

        [TestMethod]
        public void FullDecimalModeLoopWithoutCarry()
        {
            // F8      SED                  ; Decimal mode
            // 18      CLC                  ; Note: carry is clear!
            // A9 xx   LDA   #$xx           ; a
            // 69 xx   ADC   #$xx           ; b
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xF8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x00;    // a
            memory[0x04] = 0x69;
            memory[0x05] = 0x00;    // b
            memory[0x06] = 0x00;

            for (var a = 0; a <= 99; a++)
            {
                for (var b = 0; b <= 99; b++)
                {
                    var decimalA = (byte)((a / 10) << 4) | (a % 10);
                    var decimalB = (byte)((b / 10) << 4) | (b % 10);

                    memory[0x03] = (byte)decimalA;
                    memory[0x05] = (byte)decimalB;

                    var cpu = RunTinyTest(memory);

                    // check for a carry
                    bool carry = (a + b) >= 100;
                    Assert.AreEqual(carry, cpu.Carry, $"{a} + {b} (Carry)");

                    var expected = (a + b) % 100;
                    var actual = (ushort)(((cpu.A & 0xf0) >> 4) * 10) + (cpu.A & 0x0f) % 100;

                    Assert.AreEqual(expected, actual, $"{a} + {b} (sum)");
                }
            }
        }

        [TestMethod]
        public void FullBinaryModeLoopWithCarry()
        {
            // D8      CLD                  ; Binary mode
            // 38      SEC                  ; Note: carry is set
            // A9 xx   LDA   #$xx           ; a
            // 69 xx   ADC   #$xx           ; b
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xD8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x00;    // a
            memory[0x04] = 0x69;
            memory[0x05] = 0x00;    // b
            memory[0x06] = 0x00;

            for (var a = 0x00; a < 256; a++)
            {
                for (var b = 0x00; b < 256; b++)
                {
                    memory[0x03] = (byte)a;
                    memory[0x05] = (byte)b;

                    var cpu = RunTinyTest(memory);

                    ushort expected = (ushort)(a + b + 1);
                    bool carry = expected > 0xff;

                    Assert.AreEqual(carry, cpu.Carry);
                    Assert.AreEqual((byte)expected, cpu.A);
                }
            }
        }

        [TestMethod]
        public void FullDecimalModeLoopWithCarry()
        {
            // F8      SED                  ; Decimal mode
            // 38      SEC                  ; Note: carry is set
            // A9 xx   LDA   #$xx           ; a
            // 69 xx   ADC   #$xx           ; b
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xF8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x00;    // a
            memory[0x04] = 0x69;
            memory[0x05] = 0x00;    // b
            memory[0x06] = 0x00;

            for (var a = 0; a <= 99; a++)
            {
                for (var b = 0; b <= 99; b++)
                {
                    var decimalA = (byte)((a / 10) << 4) | (a % 10);
                    var decimalB = (byte)((b / 10) << 4) | (b % 10);

                    memory[0x03] = (byte)decimalA;
                    memory[0x05] = (byte)decimalB;

                    var cpu = RunTinyTest(memory);

                    // check for a carry
                    bool carry = (a + b + 1) >= 100;
                    Assert.AreEqual(carry, cpu.Carry, $"{a} + {b} (Carry)");

                    var expected = (a + b + 1) % 100;
                    var actual = (ushort)(((cpu.A & 0xf0) >> 4) * 10) + (cpu.A & 0x0f) % 100;

                    Assert.AreEqual(expected, actual, $"{a} + {b} (sum)");
                }
            }
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
            memory[0x00] = 0xD8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x58;
            memory[0x04] = 0x69;
            memory[0x05] = 0x46;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsFalse(cpu.Carry);
            Assert.AreEqual(0x9f, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x58;
            memory[0x04] = 0x69;
            memory[0x05] = 0x46;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsTrue(cpu.Carry);
            Assert.AreEqual(0x05, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x12;
            memory[0x04] = 0x69;
            memory[0x05] = 0x34;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsFalse(cpu.Carry);
            Assert.AreEqual(0x46, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x15;
            memory[0x04] = 0x69;
            memory[0x05] = 0x26;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsFalse(cpu.Carry);
            Assert.AreEqual(0x41, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x81;
            memory[0x04] = 0x69;
            memory[0x05] = 0x92;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsTrue(cpu.Carry);
            Assert.AreEqual(0x73, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x46;
            memory[0x04] = 0xE9;
            memory[0x05] = 0x12;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsTrue(cpu.Carry);
            Assert.AreEqual(0x34, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x40;
            memory[0x04] = 0xE9;
            memory[0x05] = 0x13;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsTrue(cpu.Carry);
            Assert.AreEqual(0x27, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x32;
            memory[0x04] = 0xE9;
            memory[0x05] = 0x02;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsTrue(cpu.Carry);
            Assert.AreEqual(0x29, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x12;
            memory[0x04] = 0xE9;
            memory[0x05] = 0x21;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsFalse(cpu.Carry);
            Assert.AreEqual(0x91, cpu.A);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x21;
            memory[0x04] = 0xE9;
            memory[0x05] = 0x34;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsFalse(cpu.Carry);
            Assert.AreEqual(0x87, cpu.A);
        }

        [TestMethod]
        public void BruceClarkExample13()
        {
            // F8      SED                  ; Decimal mode
            // 18      CLC
            // A9 90   LDA   #$90
            // 69 90   ADC   #$90

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xF8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x90;
            memory[0x04] = 0x69;
            memory[0x05] = 0x90;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsTrue(cpu.Overflow);
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
            memory[0x00] = 0xF8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x01;
            memory[0x04] = 0xE9;
            memory[0x05] = 0x01;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.IsTrue(cpu.Zero);
            Assert.AreEqual(0x00, cpu.A);
        }

        [TestMethod]
        public void AppendixA1()
        {
            // D8      CLD                  ; binary mode: 99 + 1
            // 18      CLC
            // A9 99   LDA   #$99
            // 69 01   ADC   #$01

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xD8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x99;
            memory[0x04] = 0x69;
            memory[0x05] = 0x01;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.AreEqual(0x9a, cpu.A);
            Assert.IsFalse(cpu.Zero);
        }

        [TestMethod]
        public void AppendixA2()
        {
            // F8      SED                  ; decimal mode: 99 + 1
            // 18      CLC
            // A9 99   LDA   #$99
            // 69 01   ADC   #$01

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xF8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x99;
            memory[0x04] = 0x69;
            memory[0x05] = 0x01;
            memory[0x06] = 0x00;

            var cpu = RunTinyTest(memory);
            Assert.AreEqual(0x00, cpu.A);
            Assert.IsTrue(cpu.Zero);
        }

        private static Cpu RunTinyTest(byte[] memory)
        {
            // power up initialization
            memory[Cpu.RstVectorH] = 0x00;
            memory[Cpu.RstVectorL] = 0x00;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory))
            {
                SkipTimingWait = true
            };

            cpu.Reset();
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            return cpu;
        }
    }
}
