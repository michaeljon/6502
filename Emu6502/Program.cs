using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using InnoWerks.Processors;
using InnoWerks.Simulators;

#pragma warning disable CA1859

namespace Emu6502
{
    internal sealed class Program
    {
        private readonly IMemory memory = new IOInterceptor();

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                Console.WriteLine("Interrupt received.");
                Environment.Exit(0);
            };

            var result = Parser.Default.ParseArguments<CliOptions>(args);

            result.MapResult(
                o => new Program().RunEmulator(o),

                errors =>
                {
                    foreach (var error in errors)
                    {
                        Console.Error.WriteLine(error.ToString());
                    }

                    return 1;
                }
            );
        }

        private int RunEmulator(CliOptions options)
        {
            var bytes = File.ReadAllBytes(options.RomFile);
            memory.LoadProgram(bytes, options.Location);

            var kbdThread = Task.Run(delegate
                {
                    while (true)
                    {
                        var input = Console.Read();
                        if (input != -1)
                        {
                            memory[0xc000] = (byte)(input | 0x80);
                        }
                    }
                }
            );

            var cpu = new Cpu(
                CpuClass.WDC65C02,
                memory,
                (cpu, programCounter) => { },
                (cpu) =>
                {
                });

            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;

            cpu.Reset();
            cpu.Run(stopOnBreak: true, writeInstructions: true, stepsPerSecond: options.StepsPerSecond);

            return 0;
        }
    }
}
