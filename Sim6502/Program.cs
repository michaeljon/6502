using System;
using System.IO;
using InnoWerks.Assemblers;
using InnoWerks.Processors;

namespace InnoWerks.Simulators.Driver
{
    internal sealed class Program
    {
        private static void Main(string[] _)
        {
            Run("../Modules/BcdTest/BruceClark65C02.S", 0x8000, 0x8000);
        }

        private static void Run(string filename, ushort org, ushort startAddr)
        {
            var programLines = File.ReadAllLines(filename);
            var assembler = new Assembler(
                programLines,
                org
            );
            assembler.Assemble();

            var memory = new Memory();
            memory.LoadProgram(assembler.ObjectCode, org);

            var cpu = new Cpu(
                CpuClass.WDC65C02,
                memory,
                (cpu, programCounter) =>
                {
                    if (assembler.ProgramByAddress != null)
                    {
                        if (assembler.ProgramByAddress.TryGetValue(programCounter, out var lineInformation))
                        {
                            Console.WriteLine($"{lineInformation.EffectiveAddress:X4} | {lineInformation.MachineCodeAsString,-10}| {lineInformation.RawInstructionText}");
                        }
                        else
                        {
                            Console.WriteLine($"{programCounter:X4} | {{no information found}}");
                        }
                    }

                    Console.Write("<dbg> ");
                    Console.Read();
                },
                (cpu) =>
                {
                    Console.WriteLine();
                    Console.WriteLine($"PC:{cpu.Registers.ProgramCounter:X4} {cpu.Registers.GetRegisterDisplay} {cpu.Registers.InternalGetFlagsDisplay}");
                    Console.WriteLine($"Error: ${memory[0x2f]:X2}");
                    PrintPage(memory, 0, 2);
                });

            // power up initialization
            memory[Cpu.RstVectorH] = (byte)((startAddr & 0xff00) >> 8);
            memory[Cpu.RstVectorL] = (byte)(startAddr & 0xff);

            cpu.Reset();

            // run
            Console.WriteLine();
            while (true)
            {
                cpu.Run(stopOnBreak: true, writeInstructions: false);
            }
        }

        private static void PrintPage(Memory memory, ushort page, byte lines)
        {
            for (var l = page * 0x100; l < (page + 1) * 0x100 && lines-- > 0; l += 16)
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

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}
