using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class MathTests : TestBase
    {
        [TestMethod]
        public void BruceClark6502()
        {
            const string filename = "Modules/BcdTest/BruceClark";
            const ushort origin = 0x8000;
            const ushort initializationVector = 0x8000;

            byte[] memory = new byte[1024 * 64];

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                long length = fs.Length;
                fs.Read(memory, origin, (int)length);
            }

            // power up initialization
            memory[Cpu.RstVectorH] = (initializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = initializationVector & 0xff;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory));

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: true);

            PrintPage(memory, 0x00);

            Assert.AreEqual(0x04, cpu.A);
            Assert.IsFalse(cpu.Carry);
        }

        [TestMethod]
        public void BruceClark65C02()
        {
            const string filename = "Modules/BcdTest/BruceClark";
            const ushort origin = 0x8000;
            const ushort initializationVector = 0x8000;

            byte[] memory = new byte[1024 * 64];

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                long length = fs.Length;
                fs.Read(memory, origin, (int)length);
            }

            // patch for 65c02
            const ushort jump1L = 0x802A;
            const ushort jump1H = 0x802B;
            const ushort s6502 = 0x80AB;

            const ushort jump2L = 0x8035;
            const ushort jump2H = 0x8036;
            const ushort s65c02 = 0x80C6;

            memory[jump1H] = (s6502 & 0xff00) >> 8;
            memory[jump1L] = s6502 & 0xff;

            memory[jump2H] = (s65c02 & 0xff00) >> 8;
            memory[jump2L] = s65c02 & 0xff;

            // power up initialization
            memory[Cpu.RstVectorH] = (initializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = initializationVector & 0xff;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory));

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            PrintPage(memory, 0x00);

            Assert.AreEqual(0x04, cpu.A);
            Assert.IsFalse(cpu.Carry);
        }

        [TestMethod]
        public void AddWithCarryBinaryModeWithoutCarry()
        {
            byte[] memory = new byte[1024 * 64];

            memory[0x00] = 0x69;   // ADC

            // power up initialization
            const ushort initializationVector = 0x0000;

            memory[Cpu.RstVectorH] = (initializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = initializationVector & 0xff;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory));

            byte carry = 0;

            for (var a = 0x00; a < 0x100; a++)
            {
                for (var b = 0x00; b < 0x100; b++)
                {
                    // store the value
                    memory[0x01] = (byte)b;

                    // reset the cpu to a known state
                    cpu.Reset();
                    cpu.A = (byte)a;
                    cpu.Carry = false;

                    // run the instruction
                    cpu.ADC(0x0001, cycles: 0);

                    ushort expectedSum = (ushort)(a + b + carry);

                    // assert the results
                    Assert.AreEqual((byte)expectedSum, cpu.A);

                    bool carrySet = (expectedSum & 0x100) == 0x100;
                    bool zeroSet = (byte)expectedSum == 0;
                    bool overflowSet = ((a ^ expectedSum) & (b ^ expectedSum) & 0x80) != 0;
                    bool negativeSet = ((byte)expectedSum & 0x80) == 0x80;

                    Assert.AreEqual(carrySet, cpu.Carry, $"C: {a} + {b} = {(byte)(a + b)}");
                    Assert.AreEqual(zeroSet, cpu.Zero, $"Z: {a} + {b} = {(byte)(a + b)}");
                    Assert.AreEqual(overflowSet, cpu.Overflow, $"V: {a} + {b} = {(byte)(a + b)}");
                    Assert.AreEqual(negativeSet, cpu.Negative, $"N: {a} + {b} = {(byte)(a + b)}");
                }
            }
        }

        [TestMethod]
        public void AddWithCarryBinaryModeWithCarry()
        {
            byte[] memory = new byte[1024 * 64];

            memory[0x00] = 0x69;   // ADC

            // power up initialization
            const ushort initializationVector = 0x0000;

            memory[Cpu.RstVectorH] = (initializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = initializationVector & 0xff;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory));

            byte carry = 1;

            for (var a = 0x00; a < 0x100; a++)
            {
                for (var b = 0x00; b < 0x100; b++)
                {
                    // store the value
                    memory[0x01] = (byte)b;

                    // reset the cpu to a known state
                    cpu.Reset();
                    cpu.A = (byte)a;
                    cpu.Carry = true;

                    // run the instruction
                    cpu.ADC(0x0001, cycles: 0);

                    ushort expectedSum = (ushort)(a + b + carry);

                    // assert the results
                    Assert.AreEqual((byte)expectedSum, cpu.A);

                    bool carrySet = (expectedSum & 0x100) == 0x100;
                    bool zeroSet = (byte)expectedSum == 0;
                    bool overflowSet = ((a ^ expectedSum) & (b ^ expectedSum) & 0x80) != 0;
                    bool negativeSet = ((byte)expectedSum & 0x80) == 0x80;

                    Assert.AreEqual(carrySet, cpu.Carry, $"C: {a} + {b} = {(byte)(a + b)}");
                    Assert.AreEqual(zeroSet, cpu.Zero, $"Z: {a} + {b} = {(byte)(a + b)}");
                    Assert.AreEqual(overflowSet, cpu.Overflow, $"V: {a} + {b} = {(byte)(a + b)}");
                    Assert.AreEqual(negativeSet, cpu.Negative, $"N: {a} + {b} = {(byte)(a + b)}");
                }
            }
        }

        [TestMethod]
        public void AddWithCarryDecimalModeWithoutCarry()
        {
            byte[] memory = new byte[1024 * 64];

            memory[0x00] = 0x69;   // ADC

            // power up initialization
            const ushort initializationVector = 0x0000;

            memory[Cpu.RstVectorH] = (initializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = initializationVector & 0xff;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory));

            for (var aprime = 0; aprime < 100; aprime++)
            {
                for (var bprime = 0; bprime < 100; bprime++)
                {
                    byte a = (byte)((byte)(aprime / 10) << 4 & (byte)(aprime % 10));
                    byte b = (byte)((byte)(bprime / 10) << 4 & (byte)(bprime % 10));
                    byte carry = 0;

                    // store the value
                    memory[0x01] = b;

                    // reset the cpu to a known state
                    cpu.Reset();
                    cpu.A = a;
                    cpu.Decimal = true;
                    cpu.Carry = false;

                    // run the instruction
                    cpu.ADC(0x0001, cycles: 0);

                    ushort expectedSum = (ushort)(aprime + bprime + carry);

                    // // assert the results
                    // Assert.AreEqual((byte)expectedSum, cpu.A);

                    // bool carrySet = (expectedSum & 0x100) == 0x100;
                    // bool zeroSet = (byte)expectedSum == 0;
                    // bool overflowSet = ((a ^ expectedSum) & (b ^ expectedSum) & 0x80) != 0;
                    // bool negativeSet = ((byte)expectedSum & 0x80) == 0x80;

                    // Assert.AreEqual(carrySet, cpu.Carry, $"C: {a} + {b} = {(byte)(a + b)}");
                    // Assert.AreEqual(zeroSet, cpu.Zero, $"Z: {a} + {b} = {(byte)(a + b)}");
                    // Assert.AreEqual(overflowSet, cpu.Overflow, $"V: {a} + {b} = {(byte)(a + b)}");
                    // Assert.AreEqual(negativeSet, cpu.Negative, $"N: {a} + {b} = {(byte)(a + b)}");
                }
            }
        }

        [TestMethod]
        public void SpecificDecimalTest()
        {
            byte[] memory = new byte[1024 * 64];

            memory[0x00] = 0x69;   // ADC

            // power up initialization
            const ushort initializationVector = 0x0000;

            memory[Cpu.RstVectorH] = (initializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = initializationVector & 0xff;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory));

            byte a = 0x99;
            byte b = 0x99;
            byte carry = 0;

            // store the value
            memory[0x01] = b;

            // reset the cpu to a known state
            cpu.Reset();
            cpu.A = a;
            cpu.Decimal = true;
            cpu.Carry = false;

            // run the instruction
            cpu.ADC(0x0001, cycles: 0);

            ushort expectedSum = (ushort)(99 + 99 + carry);

            // assert the results
            Assert.AreEqual((byte)expectedSum, cpu.A);

            bool carrySet = (expectedSum & 0x100) == 0x100;
#pragma warning disable CA1508 // Avoid dead conditional code
            bool zeroSet = (byte)expectedSum == 0;
#pragma warning restore CA1508 // Avoid dead conditional code
            bool overflowSet = ((a ^ expectedSum) & (b ^ expectedSum) & 0x80) != 0;
            bool negativeSet = ((byte)expectedSum & 0x80) == 0x80;

            Assert.AreEqual(carrySet, cpu.Carry, $"C: {a} + {b} = {(byte)(99 + 99)}");
            Assert.AreEqual(zeroSet, cpu.Zero, $"Z: {a} + {b} = {(byte)(99 + 99)}");
            Assert.AreEqual(overflowSet, cpu.Overflow, $"V: {a} + {b} = {(byte)(99 + 99)}");
            Assert.AreEqual(negativeSet, cpu.Negative, $"N: {a} + {b} = {(byte)(99 + 99)}");
        }
    }
}
