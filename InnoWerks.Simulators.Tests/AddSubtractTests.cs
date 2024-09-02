using InnoWerks.Assemblers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable IDE1006

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class AddSubtractTests : TestBase
    {
        [TestMethod]
        public void AdcFullBinaryModeLoopWithoutCarry()
        {
            for (short a = 0x00; a < 256; a++)
            {
                for (short b = 0x00; b < 256; b++)
                {
                    var memory = new AccessCountingMemory();
                    var assembler = new Assembler(
                        [
                            $"   CLD            ; binary mode",
                            $"   CLC            ; clear carry ",
                            $"   LDA #${a:x2}   ; a",
                            $"   ADC #${b:x2}   ; b",
                        ],
                        0x0000
                    );
                    assembler.Assemble();
                    memory.LoadProgram(assembler.ObjectCode, 0);

                    var cpu = RunTinyTest(memory, assembler.ProgramByAddress);

                    ushort expected = (ushort)(a + b);
                    bool carry = expected > 0xff;

                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} + {b} (Carry)");
                    Assert.AreEqual((byte)(expected & 0xff), cpu.Registers.A, $"{a} + {b} (sum)");
                }
            }
        }

        [TestMethod]
        public void Mini()
        {
            const byte a = 0x01;
            const byte b = 0xff;

            var memory = new AccessCountingMemory();
            var assembler = new Assembler(
                [
                    $"   CLD            ; binary mode",
                    $"   CLC            ; clear carry ",
                    $"   LDA #${a:x2}   ; a",
                    $"   ADC #${b:x2}   ; b",
                ],
                0x0000
            );
            assembler.Assemble();
            memory.LoadProgram(assembler.ObjectCode, 0);

            var cpu = RunTinyTest(memory, assembler.ProgramByAddress);

            const ushort expected = (ushort)(a + b);
            const bool carry = expected > 0xff;

            Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} + {b} (Carry)");
            Assert.AreEqual((byte)(expected & 0xff), cpu.Registers.A, $"{a} + {b} (sum)");
        }

        [TestMethod]
        public void Mini2()
        {
            var memory = new AccessCountingMemory();
            var assembler = new Assembler(
                [
                    $"   CLD            ; binary mode",
                    $"   LDY #$01       ; force carry flag ",
                    $"   CPY #$01       ; ",
                    $"   LDA #$80       ; 128 ",
                    $"   ADC #$80       ; + 128",
                ],
                0x0000
            );
            assembler.Assemble();
            memory.LoadProgram(assembler.ObjectCode, 0);

            var cpu = RunTinyTest(memory, assembler.ProgramByAddress);

            Assert.IsTrue(cpu.Registers.Overflow);
            Assert.IsTrue(cpu.Registers.Carry);
        }

        [TestMethod]
        public void SbcFullBinaryModeLoopWithoutCarry()
        {
            for (var a = 0x00; a < 256; a++)
            {
                for (var b = 0x00; b < 256; b++)
                {
                    var memory = new AccessCountingMemory();
                    var assembler = new Assembler(
                        [
                            $"   CLD            ; binary mode",
                            $"   CLC            ; clear carry ",
                            $"   LDA #${a:x2}   ; a",
                            $"   SBC #${b:x2}   ; b",
                        ],
                        0x0000
                    );
                    assembler.Assemble();
                    memory.LoadProgram(assembler.ObjectCode, 0);

                    var cpu = RunTinyTest(memory, assembler.ProgramByAddress);

                    int expected = a + ~b;
                    bool carry = expected >= 0x100;
                    expected &= 0xff;

                    Assert.AreEqual(carry, cpu.Registers.Carry, $"{a} - {b} (Carry)");
                    Assert.AreEqual((byte)expected, cpu.Registers.A, $"{a} - {b} (sum)");
                }
            }
        }

        [TestMethod]
        public void AdcPartialDecimalModeLoopWithoutCarry()
        {
            for (var a = 0; a <= 99; a++)
            {
                for (var b = 0; b <= 99; b++)
                {
                    var decimalA = (byte)((a / 10) << 4) | (a % 10);
                    var decimalB = (byte)((b / 10) << 4) | (b % 10);

                    var memory = new AccessCountingMemory();
                    var assembler = new Assembler(
                        [
                            $"   SED            ; decimal mode",
                            $"   CLC            ; clear carry ",
                            $"   LDA #${decimalA:x2}   ; a",
                            $"   ADC #${decimalB:x2}   ; b",
                        ],
                        0x0000
                    );
                    assembler.Assemble();
                    memory.LoadProgram(assembler.ObjectCode, 0);

                    var cpu = RunTinyTest(memory, assembler.ProgramByAddress);

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
            for (var a = 1; a <= 99; a++)
            {
                for (var b = 0; b <= 99; b++)
                {
                    var decimalA = (byte)(((a / 10) << 4) | (a % 10));
                    var decimalB = (byte)(((b / 10) << 4) | (b % 10));

                    var memory = new AccessCountingMemory();
                    var assembler = new Assembler(
                        [
                            $"   SED            ; decimal mode",
                            $"   CLC            ; clear carry ",
                            $"   LDA #${decimalA:x2}   ; a",
                            $"   SBC #${decimalB:x2}   ; b",
                        ],
                        0x0000
                    );
                    assembler.Assemble();
                    memory.LoadProgram(assembler.ObjectCode, 0);

                    var cpu = RunTinyTest(memory, assembler.ProgramByAddress);

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
            for (var a = 0x00; a < 256; a++)
            {
                for (var b = 0x00; b < 256; b++)
                {
                    var memory = new AccessCountingMemory();
                    var assembler = new Assembler(
                        [
                            $"   CLD            ; binary mode",
                            $"   SEC            ; set carry ",
                            $"   LDA #${a:x2}   ; a",
                            $"   ADC #${b:x2}   ; b",
                        ],
                        0x0000
                    );
                    assembler.Assemble();
                    memory.LoadProgram(assembler.ObjectCode, 0);

                    var cpu = RunTinyTest(memory, assembler.ProgramByAddress);

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
            for (var a = 0x00; a < 256; a++)
            {
                for (var b = 0x00; b < 256; b++)
                {
                    var memory = new AccessCountingMemory();
                    var assembler = new Assembler(
                        [
                            $"   CLD            ; binary mode",
                            $"   SEC            ; set carry ",
                            $"   LDA #${a:x2}   ; a",
                            $"   SBC #${b:x2}   ; b",
                        ],
                        0x0000
                    );
                    assembler.Assemble();
                    memory.LoadProgram(assembler.ObjectCode, 0);

                    var cpu = RunTinyTest(memory, assembler.ProgramByAddress);

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
            for (var a = 0; a <= 99; a++)
            {
                for (var b = 0; b <= 99; b++)
                {
                    var decimalA = (byte)((a / 10) << 4) | (a % 10);
                    var decimalB = (byte)((b / 10) << 4) | (b % 10);

                    var memory = new AccessCountingMemory();
                    var assembler = new Assembler(
                        [
                            $"   SED            ; decimal mode",
                            $"   SEC            ; set carry ",
                            $"   LDA #${decimalA:x2}   ; a",
                            $"   ADC #${decimalB:x2}   ; b",
                        ],
                        0x0000
                    );
                    assembler.Assemble();
                    memory.LoadProgram(assembler.ObjectCode, 0);

                    var cpu = RunTinyTest(memory, assembler.ProgramByAddress);

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
            for (var a = 0; a <= 99; a++)
            {
                for (var b = 0; b <= 99; b++)
                {
                    var decimalA = (byte)((a / 10) << 4) | (a % 10);
                    var decimalB = (byte)((b / 10) << 4) | (b % 10);

                    var memory = new AccessCountingMemory();
                    var assembler = new Assembler(
                        [
                            $"   SED            ; decimal mode",
                            $"   SEC            ; set carry ",
                            $"   LDA #${decimalA:x2}   ; a",
                            $"   SBC #${decimalB:x2}   ; b",
                        ],
                        0x0000
                    );
                    assembler.Assemble();
                    memory.LoadProgram(assembler.ObjectCode, 0);

                    var cpu = RunTinyTest(memory, assembler.ProgramByAddress);

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
