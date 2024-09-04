using System;
using System.IO;
using InnoWerks.Assemblers;
using InnoWerks.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class BruceClarkTests : TestBase
    {
        [TestMethod]
        public void BruceClark6502()
        {
            const string Filename = "Modules/BcdTest/BruceClark6502.S";
            const ushort Origin = 0x8000;
            const ushort InitializationVector = 0x8000;

            const ushort ERROR = 0x2F;

            var programLines = File.ReadAllLines(Filename);
            var assembler = new Assembler(
                programLines,
                Origin
            );
            assembler.Assemble();

            var memory = new AccessCountingMemory();
            memory.LoadProgram(assembler.ObjectCode, Origin);

            // power up initialization
            memory[Cpu.RstVectorH] = (InitializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = InitializationVector & 0xff;

            var cpu = new Cpu(
                CpuClass.WDC6502,
                memory,
                (cpu, pc) => TraceCallback(cpu, pc, memory, assembler.ProgramByAddress),
                (cpu) => FlagsLoggerCallback(cpu, memory, 2));

            cpu.Reset();

            // run
            Console.WriteLine();
            var instructionsProcessed = cpu.Run(stopOnBreak: true, writeInstructions: false);

            TestContext.WriteLine($"INST: {instructionsProcessed}");
            Assert.AreEqual(0x00, memory[ERROR]);
        }

        [TestMethod]
        public void BruceClark65C02()
        {
            const string Filename = "Modules/BcdTest/BruceClark65C02.S";
            const ushort Origin = 0x8000;
            const ushort InitializationVector = 0x8000;

            const ushort ERROR = 0x2F;

            var programLines = File.ReadAllLines(Filename);
            var assembler = new Assembler(
                programLines,
                Origin
            );
            assembler.Assemble();

            var memory = new AccessCountingMemory();
            memory.LoadProgram(assembler.ObjectCode, Origin);

            // power up initialization
            memory[Cpu.RstVectorH] = (InitializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = InitializationVector & 0xff;

            var cpu = new Cpu(
                CpuClass.WDC65C02,
                memory,
                (cpu, pc) => TraceCallback(cpu, pc, memory, assembler.ProgramByAddress),
                (cpu) => FlagsLoggerCallback(cpu, memory, 2));

            cpu.Reset();

            // run
            Console.WriteLine();
            var instructionsProcessed = cpu.Run(stopOnBreak: true, writeInstructions: false);

            TestContext.WriteLine($"INST: {instructionsProcessed}");
            Assert.AreEqual(0x00, memory[ERROR]);
        }

        [TestMethod]
        public void BruceClarkOverflowTest()
        {
            const string Filename = "Modules/BcdTest/OverflowTest.S";
            const ushort Origin = 0x8000;
            const ushort InitializationVector = 0x8000;

            const ushort ERROR = 0x30;

            var programLines = File.ReadAllLines(Filename);
            var assembler = new Assembler(
                programLines,
                Origin
            );
            assembler.Assemble();

            var memory = new AccessCountingMemory();
            memory.LoadProgram(assembler.ObjectCode, Origin);

            // power up initialization
            memory[Cpu.RstVectorH] = (InitializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = InitializationVector & 0xff;

            var cpu = new Cpu(
                CpuClass.WDC65C02,
                memory,
                (cpu, pc) => TraceCallback(cpu, pc, memory, assembler.ProgramByAddress),
                (cpu) => FlagsLoggerCallback(cpu, memory, 0));

            cpu.Reset();

            // run
            Console.WriteLine();
            var instructionsProcessed = cpu.Run(stopOnBreak: true, writeInstructions: false);

            TestContext.WriteLine($"INST: {instructionsProcessed}");
            Assert.AreEqual(0x00, memory[ERROR]);
        }
    }
}
