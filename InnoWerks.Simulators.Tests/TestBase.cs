using System;
using InnoWerks.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CA1822

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class TestBase
    {
        public TestContext TestContext { get; set; }

        protected static void DummyLoggerCallback(Cpu _1, byte[] _2, int _3 = 0) { }

        protected static void LoggerCallback(Cpu cpu, byte[] memory, int lines = 1)
        {
            ArgumentNullException.ThrowIfNull(cpu);
            ArgumentNullException.ThrowIfNull(memory);

            // Console.Write($"PC:{cpu.ProgramCounter:X4} {cpu.Registers.GetRegisterDisplay} ");
            // Console.WriteLine(cpu.Registers.GetFlagsDisplay);

            for (var l = 0; l < lines; l++)
            {
                Console.Write($"{l:X4}  ");

                for (var b = 0; b < 8; b++)
                {
                    Console.Write($"{memory[(l * 16) + b]:X2} ");
                }

                Console.Write("  ");

                for (var b = 8; b < 16; b++)
                {
                    Console.Write($"{memory[(l * 16) + b]:X2} ");
                }

                Console.WriteLine("");
            }

            Console.WriteLine("");
        }

        protected Cpu RunTinyTest(byte[] memory, CpuClass cpuClass = CpuClass.WDC65C02, int lines = 1)
        {
            ArgumentNullException.ThrowIfNull(memory);

            // power up initialization
            memory[Cpu.RstVectorH] = 0x00;
            memory[Cpu.RstVectorL] = 0x00;

            var cpu = new Cpu(
                cpuClass,
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => DummyLoggerCallback(cpu, memory, lines))
            {
                SkipTimingWait = true
            };

            cpu.Reset();
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            return cpu;
        }

        protected void PrintPage(byte[] memory, byte page)
        {
            ArgumentNullException.ThrowIfNull(memory);

            for (var l = page * 0x100; l < (page + 1) * 0x100; l += 16)
            {
                Console.Write("{0:X4}:  ", l);

                for (var b = 0; b < 8; b++)
                {
                    Console.Write("{0:X2} ", memory[l + b]);
                }

                Console.Write("  ");

                for (var b = 8; b < 16; b++)
                {
                    Console.Write("{0:X2} ", memory[l + b]);
                }

                Console.WriteLine("");
            }

            Console.WriteLine("");
        }
    }
}
