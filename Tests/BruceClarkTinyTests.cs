using System.Collections.Generic;
using System.IO;
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
        private readonly Dictionary<string, ushort> tinyTestEntryPoints = new()
        {
            { "TEST1", 0x0000 },
            { "TEST2", 0x0007 },
            { "TEST3", 0x000E },
            { "TEST4", 0x0015 },
            { "TEST5", 0x001C },
            { "TEST6", 0x0023 },
            { "TEST7", 0x002A },
            { "TEST8", 0x0031 },
            { "TEST9", 0x0038 },
            { "TEST10", 0x003F },
            { "TEST11", 0x0046 },
            { "TEST12", 0x0055 },
            { "TEST13", 0x0064 },
            { "TEST14", 0x006B },

            { "TESTA", 0x0072 },
            { "TESTB", 0x0079 },
            { "TESTC", 0x007E },
            { "TESTD", 0x0085 },
        };

        private readonly Dictionary<string, ushort> tinyMemoryLocations = new()
        {
            { "TESTD", 0xE0 },
        };

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

            var cpu = RunTinyTest(memory);
            Assert.AreEqual(0x0a, memory[tinyMemoryLocations["TESTD"]]);
        }

        [TestMethod]
        public void BruceClarkExample1()
        {
            byte[] memory = new byte[1024 * 64];

            // D8      CLD                  ; Binary mode (binary addition: 88 + 70 + 1 = 159)
            // 38      SEC                  ; Note: carry is set, not clear!
            // A9 58   LDA   #$58           ; 88
            // 69 46   ADC   #$46           ; 70 (after this instruction, C = 0, A = $9F = 159)
            // 00      DB    0

            var cpu = RunTinyTest("TEST1", memory);
            Assert.IsFalse(cpu.Carry);
            Assert.AreEqual(0x9f, cpu.A);
        }

        [TestMethod]
        public void BruceClarkExample2()
        {
            byte[] memory = new byte[1024 * 64];

            // F8      SED                  ; Decimal mode (BCD addition: 58 + 46 + 1 = 105)
            // 38      SEC                  ; Note: carry is set, not clear!
            // A9 58   LDA   #$58
            // 69 46   ADC   #$46           ; After this instruction, C = 1, A = $05
            // 00      DB    0

            var cpu = RunTinyTest("TEST2", memory);
            Assert.IsTrue(cpu.Carry);
            Assert.AreEqual(0x05, cpu.A);
        }

        [TestMethod]
        public void BruceClarkExample3()
        {
            byte[] memory = new byte[1024 * 64];

            // F8      SED                  ; Decimal mode (BCD addition: 12 + 34 = 46)
            // 18      CLC
            // A9 12   LDA   #$12
            // 69 34   ADC   #$34           ; After this instruction, C = 0, A = $46
            // 00      DB    0

            var cpu = RunTinyTest("TEST3", memory);
            Assert.IsFalse(cpu.Carry);
            Assert.AreEqual(0x46, cpu.A);
        }

        [TestMethod]
        public void BruceClarkExample4()
        {
            byte[] memory = new byte[1024 * 64];

            // F8      SED                  ; Decimal mode (BCD addition: 15 + 26 = 41)
            // 18      CLC
            // A9 15   LDA   #$15
            // 69 26   ADC   #$26           ; After this instruction, C = 0, A = $41
            // 00      DB    0

            var cpu = RunTinyTest("TEST4", memory);
            Assert.IsFalse(cpu.Carry);
            Assert.AreEqual(0x41, cpu.A);
        }

        [TestMethod]
        public void BruceClarkExample5()
        {
            byte[] memory = new byte[1024 * 64];

            // F8      SED                  ; Decimal mode (BCD addition: 81 + 92 = 173)
            // 18      CLC
            // A9 81   LDA   #$81
            // 69 92   ADC   #$92           ; After this instruction, C = 1, A = $73
            // 00      DB    0

            var cpu = RunTinyTest("TEST5", memory);
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
            byte[] memory = new byte[1024 * 64];

            // F8      SED                  ; Decimal mode (BCD subtraction: 40 - 13 = 27)
            // 38      SEC
            // A9 40   LDA   #$40
            // E9 13   SBC   #$13           ; After this instruction, C = 1, A = $27)
            // 00      DB    0

            var cpu = RunTinyTest("TEST7", memory);
            Assert.IsTrue(cpu.Carry);
            Assert.AreEqual(0x27, cpu.A);
        }

        [TestMethod]
        public void BruceClarkExample8()
        {
            byte[] memory = new byte[1024 * 64];

            // F8      SED                  ; Decimal mode (BCD subtraction: 32 - 2 - 1 = 29)
            // 18      CLC                  ; Note: carry is clear, not set!
            // A9 32   LDA   #$32
            // E9 02   SBC   #$02           ; After this instruction, C = 1, A = $29)
            // 00      DB    0

            var cpu = RunTinyTest("TEST8", memory);
            Assert.IsTrue(cpu.Carry);
            Assert.AreEqual(0x29, cpu.A);
        }

        [TestMethod]
        public void BruceClarkExample9()
        {
            byte[] memory = new byte[1024 * 64];

            // F8      SED                  ; Decimal mode (BCD subtraction: 12 - 21)
            // 38      SEC
            // A9 12   LDA   #$12
            // E9 21   SBC   #$21           ; After this instruction, C = 0, A = $91)
            // 00      DB    0

            var cpu = RunTinyTest("TEST9", memory);
            Assert.IsFalse(cpu.Carry);
            Assert.AreEqual(0x91, cpu.A);
        }

        [TestMethod]
        public void BruceClarkExample10()
        {
            byte[] memory = new byte[1024 * 64];

            // F8      SED                  ; Decimal mode (BCD subtraction: 21 - 34)
            // 38      SEC
            // A9 21   LDA   #$21
            // E9 34   SBC   #$34           ; After this instruction, C = 0, A = $87)
            // 00      DB    0

            var cpu = RunTinyTest("TEST10", memory);
            Assert.IsFalse(cpu.Carry);
            Assert.AreEqual(0x87, cpu.A);
        }

        [TestMethod]
        public void BruceClarkExample13()
        {
            byte[] memory = new byte[1024 * 64];

            // F8      SED                  ; Decimal mode
            // 18      CLC
            // A9 90   LDA   #$90
            // 69 90   ADC   #$90

            var cpu = RunTinyTest("TEST13", memory);
            Assert.IsTrue(cpu.Overflow);
        }

        [TestMethod]
        public void BruceClarkExample14()
        {
            byte[] memory = new byte[1024 * 64];

            // F8      SED                  ; Decimal mode
            // 38      SEC
            // A9 01   LDA   #$01
            // E9 01   SBC   #$01           ; expect A = 0, Z = 1
            // 00      DB    0

            var cpu = RunTinyTest("TEST14", memory);
            Assert.IsTrue(cpu.Zero);
            Assert.AreEqual(0x00, cpu.A);
        }

        private Cpu RunTinyTest(string name, byte[] memory)
        {
            const string filename = "Modules/tiny/tiny";
            ushort origin = 0x0000;
            ushort initializationVector = tinyTestEntryPoints[name];

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                long length = fs.Length;
                fs.Read(memory, origin, (int)length);
            }

            // power up initialization
            memory[Cpu.RstVectorH] = (byte)((initializationVector & 0xff00) >> 8);
            memory[Cpu.RstVectorL] = (byte)(initializationVector & 0xff);

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory));

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            // PrintPage(memory, 0x00);

            return cpu;
        }

        private static Cpu RunTinyTest(byte[] memory)
        {
            // power up initialization
            memory[Cpu.RstVectorH] = 0x00;
            memory[Cpu.RstVectorL] = 0x00;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory));

            cpu.Reset();
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            return cpu;
        }
    }
}
