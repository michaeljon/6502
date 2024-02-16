using System;
using System.IO;

namespace InnoWerks.Simulators.Driver
{
    internal sealed class Program
    {
        // we have 64k to play with for now
        private readonly byte[] memory = new byte[1024 * 64];

        private static void Main(string[] _)
        {
            var program = new Program();
            program.Run("../Modules/BinarySearch/binarySearch", 0x6000, 0x605c, (cpu) =>
            {
                if (cpu.A != 0x04 || cpu.Carry != false)
                {
                    Console.WriteLine("POSCASE failed");
                }
            });
            program.Run("../Modules/BinarySearch/binarySearch", 0x6000, 0x606f, (cpu) =>
            {
                if (cpu.Carry != true)
                {
                    Console.WriteLine("NEGCASE failed");
                }
            });
        }

        private void Run(string filename, ushort org, ushort startAddr, Action<Cpu> test)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                long length = fs.Length;
                fs.Read(memory, org, (int)length);
            }

            // todo: load a program
            var cpu = new Cpu(Read, Write, (cpu) =>
            {
                // cpu.PrintStatus();
                // Console.Write($"{memory[0x6059]:X2} ");
                // Console.Write($"{memory[0x605A]:X2} ");
                // Console.WriteLine($"{memory[0x605B]:X2}\n");
            });

            // power up initialization
            Write(Cpu.RstVectorH, (byte)((startAddr & 0xff00) >> 8));
            Write(Cpu.RstVectorL, (byte)(startAddr & 0xff));

            cpu.Reset();

            // run
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

        private void PrintPage(byte page)
        {
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

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}
