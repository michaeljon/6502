using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class BruceClarkTests : TestBase
    {
        [TestMethod]
        public void BruceClark6502()
        {
            const string Filename = "Modules/BcdTest/BruceClark6502";
            const ushort Origin = 0x8000;
            const ushort InitializationVector = 0x8000;

            const ushort ERROR = 0x30;

            byte[] memory = new byte[1024 * 64];
            byte[] program = File.ReadAllBytes(Filename);

            Array.Copy(program, 0, memory, Origin, program.Length);

            // power up initialization
            memory[Cpu.RstVectorH] = (InitializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = InitializationVector & 0xff;

            var cpu = new Cpu(
                CpuClass.WDC6502,
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => DummyLoggerCallback(cpu, memory))
            {
                SkipTimingWait = true
            };

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: true);

            if (memory[ERROR] != 0x00)
            {
                PrintPage(memory, 0x00);
                cpu.PrintStatus();
            }

            TestContext.WriteLine($"INST: {cpu.InstructionsProcessed}");
            Assert.AreEqual(0x00, memory[ERROR]);
        }

        [TestMethod]
        public void BruceClark65C02()
        {
            const string Filename = "Modules/BcdTest/BruceClark65C02";
            const ushort Origin = 0x8000;
            const ushort InitializationVector = 0x8000;

            const ushort ERROR = 0x30;

            byte[] memory = new byte[1024 * 64];
            byte[] program = File.ReadAllBytes(Filename);

            Array.Copy(program, 0, memory, Origin, program.Length);

            // power up initialization
            memory[Cpu.RstVectorH] = (InitializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = InitializationVector & 0xff;

            var cpu = new Cpu(
                CpuClass.WDC65C02,
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => DummyLoggerCallback(cpu, memory, 2))
            {
                SkipTimingWait = true
            };

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: true);

            if (memory[ERROR] != 0x00)
            {
                PrintPage(memory, 0x00);
                cpu.PrintStatus();
            }

            TestContext.WriteLine($"INST: {cpu.InstructionsProcessed}");
            Assert.AreEqual(0x00, memory[ERROR]);
        }

        [TestMethod]
        public void BruceClarkOverflowTest()
        {
            const string Filename = "Modules/BcdTest/OverflowTest";
            const ushort Origin = 0x8000;
            const ushort InitializationVector = 0x8000;

            const ushort ERROR = 0x30;

            byte[] memory = new byte[1024 * 64];
            byte[] program = File.ReadAllBytes(Filename);

            Array.Copy(program, 0, memory, Origin, program.Length);

            // power up initialization
            memory[Cpu.RstVectorH] = (InitializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = InitializationVector & 0xff;

            var cpu = new Cpu(
                CpuClass.WDC65C02,
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => DummyLoggerCallback(cpu, memory, 1))
            {
                SkipTimingWait = true
            };

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: true);

            if (memory[ERROR] != 0x00)
            {
                PrintPage(memory, 0x00);
                cpu.PrintStatus();
            }

            TestContext.WriteLine($"INST: {cpu.InstructionsProcessed}");
            Assert.AreEqual(0x00, memory[ERROR]);
        }
    }
}
