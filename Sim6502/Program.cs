using System;
using System.IO;
using InnoWerks.Processors;

namespace InnoWerks.Simulators.Driver
{
    internal sealed class Program
    {
        // we have 64k to play with for now
        private readonly Memory memory = new();

        private static void Main(string[] _)
        {
            var program = new Program();
            program.Run("../Modules/BcdTest/BruceClark", 0x8000, 0x8000);
        }

        private void Run(string filename, ushort org, ushort startAddr, Action<Cpu> test = null)
        {
            byte[] program = File.ReadAllBytes(filename);
            memory.LoadProgram(program, org);

            var cpu = new Cpu(
                CpuClass.WDC65C02,
                memory,
                (cpu, programCounter) => { },
                (cpu) =>
                {
                    cpu.PrintStatus();

                    Console.WriteLine();
                    PrintPage(0x00, 0x02);

                    Console.Write("<enter> to continue ");
                    Console.ReadLine();
                    Console.WriteLine();
                });

            // power up initialization
            Write(Cpu.RstVectorH, (byte)((startAddr & 0xff00) >> 8));
            Write(Cpu.RstVectorL, (byte)(startAddr & 0xff));

            cpu.Reset();

            // run
            Console.WriteLine();
            cpu.Run(stopOnBreak: true, writeInstructions: false);
            test?.Invoke(cpu);

            Console.WriteLine($"Ticks: {cpu.Cycles}");
        }

        private byte Read(ushort addr)
        {
            return memory[addr];
        }

        private void Write(ushort addr, byte b)
        {
            memory[addr] = b;
        }

        private void PrintPage(byte page, byte lines)
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
