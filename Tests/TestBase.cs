using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class TestBase
    {
        public TestContext TestContext { get; set; }

        protected void LoggerCallback(Cpu cpu, byte[] memory)
        {
            ArgumentNullException.ThrowIfNull(cpu);
            ArgumentNullException.ThrowIfNull(memory);

            TestContext.Write($"PC: 0x{cpu.ProgramCounter:X4} ");
            TestContext.Write($"A: 0x{cpu.A:X2} X: 0x{cpu.X:X2} Y: 0x{cpu.Y:X2} SP: 0x{cpu.StackPointer:X2} PS: 0x{cpu.ProcessorStatus:X2} ");
            TestContext.WriteLine($"PS: {(cpu.Negative ? 1 : 0)}{(cpu.Overflow ? 1 : 0)}{1}{(cpu.Break ? 1 : 0)}{(cpu.Decimal ? 1 : 0)}{(cpu.Interrupt ? 1 : 0)}{(cpu.Zero ? 1 : 0)}{(cpu.Carry ? 1 : 0)}");

            TestContext.Write($"{memory[0x6059]:X2} ");
            TestContext.Write($"{memory[0x605A]:X2} ");
            TestContext.WriteLine($"{memory[0x605B]:X2}\n");
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
