using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class AddSubtractTests : TestBase
    {
        [TestMethod]
        public void AdcFullBinaryModeLoopWithoutCarry()
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

                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} + {b} (Carry)");
                    Assert.AreEqual((byte)expected, cpu.Registers.A, $"{a} + {b} (sum)");
                }
            }
        }

        [TestMethod]
        public void SbcFullBinaryModeLoopWithoutCarry()
        {
            // D8      CLD                  ; Binary mode
            // 18      CLC                  ; Note: carry is clear!
            // A9 xx   LDA   #$xx           ; a
            // E9 xx   SBC   #$xx           ; b
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xD8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x00;    // a
            memory[0x04] = 0xE9;
            memory[0x05] = 0x00;    // b
            memory[0x06] = 0x00;

            for (var a = 0x00; a < 256; a++)
            {
                for (var b = 0x00; b < 256; b++)
                {
                    memory[0x03] = (byte)a;
                    memory[0x05] = (byte)b;

                    var cpu = RunTinyTest(memory);

                    int expected = (ushort)(0xff + a - b + 0);
                    bool carry = expected >= 0x0100;

                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} - {b} (Carry)");
                    Assert.AreEqual((byte)expected, cpu.Registers.A, $"{a} - {b} (sum)");
                }
            }
        }

        [TestMethod]
        public void AdcFullDecimalModeLoopWithoutCarry()
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
                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} + {b} (Carry)");

                    var expected = (a + b) % 100;
                    var actual = (ushort)(((cpu.Registers.A & 0xf0) >> 4) * 10) + (cpu.Registers.A & 0x0f) % 100;

                    Assert.AreEqual(expected, actual, $"{a} + {b} (sum)");
                }
            }
        }

        //[TestMethod]
        public static void SbcFullDecimalModeLoopWithoutCarry()
        {
            // F8      SED                  ; Decimal mode
            // 18      CLC                  ; Note: carry is clear!
            // A9 xx   LDA   #$xx           ; a
            // e9 xx   SBC   #$xx           ; b
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xF8;
            memory[0x01] = 0x18;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x00;    // a
            memory[0x04] = 0xe9;
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
                    bool carry = (0xff + a - b + 0) >= 0x100;
                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} - {b} (Carry)");

                    var expected = (a - b + 1) % 100;
                    var actual = (ushort)(((cpu.Registers.A & 0xf0) >> 4) * 10) + (cpu.Registers.A & 0x0f) % 100;

                    Assert.AreEqual(expected, actual, $"{a} - {b} (sum)");
                }
            }
        }

        [TestMethod]
        public void AdcFullBinaryModeLoopWithCarry()
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

                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} + {b} (Carry)");
                    Assert.AreEqual((byte)expected, cpu.Registers.A, $"{a} + {b} (sum)");
                }
            }
        }

        [TestMethod]
        public void SbcFullBinaryModeLoopWithCarry()
        {
            // D8      CLD                  ; Binary mode
            // 38      SEC                  ; Note: carry is set
            // A9 xx   LDA   #$xx           ; a
            // E9 xx   SBC   #$xx           ; b
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xD8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x00;    // a
            memory[0x04] = 0xE9;
            memory[0x05] = 0x00;    // b
            memory[0x06] = 0x00;

            for (var a = 0x00; a < 256; a++)
            {
                for (var b = 0x00; b < 256; b++)
                {
                    memory[0x03] = (byte)a;
                    memory[0x05] = (byte)b;

                    var cpu = RunTinyTest(memory);

                    int expected = (ushort)(0xff + a - b + 1);
                    bool carry = expected >= 0x0100;

                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} + {b} (Carry)");
                    Assert.AreEqual((byte)expected, cpu.Registers.A, $"{a} + {b} (sum)");
                }
            }
        }

        [TestMethod]
        public void AdcFullDecimalModeLoopWithCarry()
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
                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} + {b} (Carry)");

                    var expected = (a + b + 1) % 100;
                    var actual = (ushort)(((cpu.Registers.A & 0xf0) >> 4) * 10) + (cpu.Registers.A & 0x0f) % 100;

                    Assert.AreEqual(expected, actual, $"{a} + {b} (sum)");
                }
            }
        }

        //[TestMethod]
        public static void SbcFullDecimalModeLoopWithCarry()
        {
            // F8      SED                  ; Decimal mode
            // 38      SEC                  ; Note: carry is set
            // A9 xx   LDA   #$xx           ; a
            // E9 xx   SBC   #$xx           ; b
            // 00      DB    0

            byte[] memory = new byte[1024 * 64];
            memory[0x00] = 0xF8;
            memory[0x01] = 0x38;
            memory[0x02] = 0xA9;
            memory[0x03] = 0x00;    // a
            memory[0x04] = 0xE9;
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
                    bool carry = (0xff + a - b + 1) >= 0x100;
                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} - {b} (Carry)");

                    var expected = (a - b + 0) % 100;
                    var actual = (ushort)(((cpu.Registers.A & 0xf0) >> 4) * 10) + (cpu.Registers.A & 0x0f) % 100;

                    Assert.AreEqual(expected, actual, $"{a} - {b} (sum)");
                }
            }
        }
    }
}
