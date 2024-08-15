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

            for (short a = 0x00; a < 256; a++)
            {
                for (short b = 0x00; b < 256; b++)
                {
                    memory[0x03] = (byte)(a & 0xff);
                    memory[0x05] = (byte)(b & 0xff);

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

                    int expected = 0x00ff + a - b + 0;
                    bool carry = expected >= 0x100;

                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} - {b} (Carry)");
                    Assert.AreEqual((byte)expected, cpu.Registers.A, $"{a} - {b} (sum)");
                }
            }
        }

        [TestMethod]
        public void AdcPartialDecimalModeLoopWithoutCarry()
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

        [TestMethod]
        public void SbcPartialDecimalModeLoopWithoutCarry()
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

            for (var a = 1; a <= 99; a++)
            {
                for (var b = 0; b <= 99; b++)
                {
                    var decimalA = (byte)(((a / 10) << 4) | (a % 10));
                    var decimalB = (byte)(((b / 10) << 4) | (b % 10));

                    memory[0x03] = decimalA;
                    memory[0x05] = decimalB;

                    var cpu = RunTinyTest(memory);

                    bool carryFlag;
                    bool overflowFlag = ((decimalA ^ decimalB) & 0x0080) != 0;
                    const int carry = 0;

                    int result;

                    // lo nibble
                    int temp = 0x0f + (decimalA & 0x0f) - (decimalB & 0x0f) + carry;
                    if (temp < 0x10)
                    {
                        result = 0;
                        temp -= 0x06;
                    }
                    else
                    {
                        result = 0x10;
                        temp -= 0x10;
                    }

                    // hi nibble
                    result += 0x00f0 + (decimalA & 0x00f0) - (decimalB & 0x00f0);
                    if (result < 0x0100)
                    {
                        carryFlag = false;
                        if (overflowFlag == true && result < 0x0080)
                        {
                            overflowFlag = false;
                        }
                        result -= 0x60;
                    }
                    else
                    {
                        carryFlag = true;
                        if (overflowFlag == true && result >= 0x0180)
                        {
                            overflowFlag = false;
                        }
                    }
                    result += temp;
                    result &= 0xff;

                    Assert.AreEqual(result, cpu.Registers.A, $"{a} - {b} (sum)");
                    Assert.AreEqual(carryFlag, cpu.Registers.Carry, $"{a} - {b} (Carry)");
                    Assert.AreEqual(overflowFlag, cpu.Registers.Overflow, $"{a} - {b} (Overflow)");
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

                    int expected = 0x00ff + a - b + 1;
                    bool carry = expected >= 0x100;

                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} + {b} (Carry)");
                    Assert.AreEqual((byte)expected, cpu.Registers.A, $"{a} + {b} (sum)");
                }
            }
        }

        [TestMethod]
        public void AdcPartialDecimalModeLoopWithCarry()
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

        [TestMethod]
        public void SbcPartialDecimalModeLoopWithCarry()
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

                    bool carryFlag;
                    bool overflowFlag = ((decimalA ^ decimalB) & 0x0080) != 0;
                    const int carry = 1;

                    int result;

                    // lo nibble
                    int temp = 0x0f + (decimalA & 0x0f) - (decimalB & 0x0f) + carry;
                    if (temp < 0x10)
                    {
                        result = 0;
                        temp -= 0x06;
                    }
                    else
                    {
                        result = 0x10;
                        temp -= 0x10;
                    }

                    // hi nibble
                    result += 0x00f0 + (decimalA & 0x00f0) - (decimalB & 0x00f0);
                    if (result < 0x0100)
                    {
                        carryFlag = false;
                        if (overflowFlag == true && result < 0x0080)
                        {
                            overflowFlag = false;
                        }
                        result -= 0x60;
                    }
                    else
                    {
                        carryFlag = true;
                        if (overflowFlag == true && result >= 0x0180)
                        {
                            overflowFlag = false;
                        }
                    }
                    result += temp;
                    result &= 0xff;

                    Assert.AreEqual(result, cpu.Registers.A, $"{a} - {b} (sum)");
                    Assert.AreEqual(carryFlag, cpu.Registers.Carry, $"{a} - {b} (Carry)");
                    Assert.AreEqual(overflowFlag, cpu.Registers.Overflow, $"{a} - {b} (Overflow)");
                }
            }
        }
    }
}
