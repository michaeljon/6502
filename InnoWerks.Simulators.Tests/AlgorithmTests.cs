using System;
using System.IO;
using InnoWerks.Assemblers;
using InnoWerks.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class AlgorithmTests : TestBase
    {
        [TestMethod]
        public void BinarySearchNegativeCase()
        {
            const string Filename = "Modules/BinarySearch/binarySearch.S";
            const ushort Origin = 0x6000;
            const ushort InitializationVector = 0x606f;

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
                (cpu, pc) => DummyTraceCallback(cpu, pc, memory, null),
                (cpu) => DummyLoggerCallback(cpu, memory, 0));

            cpu.Reset();

            // run
            Console.WriteLine();
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            Assert.IsTrue(cpu.Registers.Carry);
        }

        [TestMethod]
        public void BinarySearchPositiveCase()
        {
            const string Filename = "Modules/BinarySearch/binarySearch.S";
            const ushort Origin = 0x6000;
            const ushort InitializationVector = 0x605c;

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
                (cpu, pc) => DummyTraceCallback(cpu, pc, memory, null),
                (cpu) => DummyLoggerCallback(cpu, memory, 0));

            cpu.Reset();

            // run
            Console.WriteLine();
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            Assert.AreEqual(0x04, cpu.Registers.A);
            Assert.IsFalse(cpu.Registers.Carry);
        }
    }
}
