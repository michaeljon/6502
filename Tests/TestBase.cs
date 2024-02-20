using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class TestBase
    {
        public TestContext TestContext { get; set; }

        protected static void DummyLoggerCallback(Cpu _1, byte[] _2)
        {
        }

        protected static void LoggerCallback(Cpu cpu, byte[] memory)
        {
            ArgumentNullException.ThrowIfNull(cpu);
            ArgumentNullException.ThrowIfNull(memory);

            Console.Write($"PC:{cpu.ProgramCounter:X4} {cpu.Registers.GetRegisterDisplay} ");
            Console.WriteLine(cpu.Registers.GetFlagsDisplay);

            for (var l = 0; l < 2; l++)
            {
                Console.Write($"{l:X4}  ");

                for (var b = 0; b < 8; b++)
                {
                    Console.Write($"{memory[l + b]:X2} ");
                }

                Console.Write("  ");

                for (var b = 8; b < 16; b++)
                {
                    Console.Write($"{memory[l + b]:X2} ");
                }

                Console.WriteLine("");
            }

            Console.WriteLine("");
        }

        protected static Cpu RunTinyTest(byte[] memory)
        {
            ArgumentNullException.ThrowIfNull(memory);

            // power up initialization
            memory[Cpu.RstVectorH] = 0x00;
            memory[Cpu.RstVectorL] = 0x00;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => DummyLoggerCallback(cpu, memory))
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
                TestContext.Write("{0:X4}:  ", l);

                for (var b = 0; b < 8; b++)
                {
                    TestContext.Write("{0:X2} ", memory[l + b]);
                }

                TestContext.Write("  ");

                for (var b = 8; b < 16; b++)
                {
                    TestContext.Write("{0:X2} ", memory[l + b]);
                }

                TestContext.WriteLine("");
            }

            TestContext.WriteLine("");
        }
    }
}
