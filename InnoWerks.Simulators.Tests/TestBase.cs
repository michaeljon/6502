using System;
using System.Collections.Generic;
using InnoWerks.Assemblers;
using InnoWerks.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CA1822

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class TestBase
    {
        public TestContext TestContext { get; set; }

        protected static void DummyLoggerCallback(Cpu _1, IMemory _2, int _3 = 0) { }

        protected static void FlagsLoggerCallback(Cpu cpu, IMemory memory, int lines = 0)
        {
            ArgumentNullException.ThrowIfNull(cpu);
            ArgumentNullException.ThrowIfNull(memory);

            Console.WriteLine($"\tPC:{cpu.Registers.ProgramCounter:X4} {cpu.Registers.GetRegisterDisplay} {cpu.Registers.InternalGetFlagsDisplay}");
            Console.WriteLine();

            PrintMemoryLines(memory, lines);
        }

        protected static void LoggerCallback(Cpu cpu, IMemory memory, int lines = 1)
        {
            ArgumentNullException.ThrowIfNull(cpu);
            ArgumentNullException.ThrowIfNull(memory);

            PrintMemoryLines(memory, lines);
        }

        protected static void DummyTraceCallback(Cpu _1, ushort _2, IMemory _3, Dictionary<ushort, LineInformation> _4 = null) { }

        protected static void FlagsTraceCallback(Cpu cpu, ushort _1, IMemory memory, Dictionary<ushort, LineInformation> _2 = null)
        {
            ArgumentNullException.ThrowIfNull(cpu);
            ArgumentNullException.ThrowIfNull(memory);

            Console.WriteLine($"\tPC:{cpu.Registers.ProgramCounter:X4} {cpu.Registers.GetRegisterDisplay} {cpu.Registers.InternalGetFlagsDisplay}");
        }

        protected static void TraceCallback(Cpu cpu, ushort programCounter, IMemory memory, Dictionary<ushort, LineInformation> code)
        {
            ArgumentNullException.ThrowIfNull(cpu);
            ArgumentNullException.ThrowIfNull(memory);

            if (code != null)
            {
                if (code.TryGetValue(programCounter, out var lineInformation))
                {
                    Console.WriteLine($"{lineInformation.EffectiveAddress:X4} | {lineInformation.MachineCodeAsString,-10}| {lineInformation.RawInstructionText}");
                }
                else
                {
                    Console.WriteLine($"{programCounter:X4} | {{no information found}}");
                }
            }
        }


        protected Cpu RunTinyTest(IMemory memory, Dictionary<ushort, LineInformation> code = null, CpuClass cpuClass = CpuClass.WDC65C02, int lines = 1)
        {
            ArgumentNullException.ThrowIfNull(memory);

            // power up initialization
            memory[Cpu.RstVectorH] = 0x00;
            memory[Cpu.RstVectorL] = 0x00;

            var cpu = new Cpu(
                cpuClass,
                memory,
                (cpu, pc) => DummyTraceCallback(cpu, pc, memory, code),
                (cpu) => DummyLoggerCallback(cpu, memory, lines))
            {
                SkipTimingWait = true
            };

            cpu.Reset();
            if (code != null)
            {
                for (var s = 0; s < code.Count; s++)
                {
                    cpu.Step(stopOnBreak: true, writeInstructions: false);
                }
            }
            else
            {
                cpu.Run(stopOnBreak: true, writeInstructions: false);
            }

            return cpu;
        }

        protected void PrintPage(IMemory memory, byte page)
        {
            ArgumentNullException.ThrowIfNull(memory);

            for (var l = page * 0x100; l < (page + 1) * 0x100; l += 16)
            {
                Console.Write("{0:X4}:  ", l);

                for (var b = 0; b < 8; b++)
                {
                    Console.Write("{0:X2} ", memory[(ushort)(l + b)]);
                }

                Console.Write("  ");

                for (var b = 8; b < 16; b++)
                {
                    Console.Write("{0:X2} ", memory[(ushort)(l + b)]);
                }

                Console.WriteLine("");
            }

            Console.WriteLine("");
        }

        private static void PrintMemoryLines(IMemory memory, int lines)
        {
            if (lines == 0)
            {
                return;
            }

            Console.Write($"\t      ");

            for (var b = 0; b < 8; b++)
            {
                Console.Write($"{b:X2} ");
            }

            Console.Write("\t  ");

            for (var b = 8; b < 16; b++)
            {
                Console.Write($"{b:X2} ");
            }

            Console.WriteLine("");

            for (var l = 0; l < lines; l++)
            {
                Console.Write($"\t{l:X4}  ");

                for (var b = 0; b < 8; b++)
                {
                    Console.Write($"{memory[(ushort)((l * 16) + b)]:X2} ");
                }

                Console.Write("\t  ");

                for (var b = 8; b < 16; b++)
                {
                    Console.Write($"{memory[(ushort)((l * 16) + b)]:X2} ");
                }

                Console.WriteLine("");
            }

            Console.WriteLine("");
        }
    }
}
