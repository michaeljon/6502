using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using InnoWerks.Simulators;

#pragma warning disable CA1859

namespace Emu6502
{
    internal sealed class Program
    {
        private readonly IBus memory = new IOInterceptor();

        public static void Main(string[] args)
        {
            Console.TreatControlCAsInput = true;

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
            long lastInterrupt = 0;

            var bytes = File.ReadAllBytes(options.RomFile);
            memory.LoadProgram(bytes, options.Location);

            var kbdThread = Task.Run(async delegate
                {
                    while (true)
                    {
                        var input = Console.ReadKey();

                        if (input.KeyChar == 0x03)
                        {
                            if (lastInterrupt == 0)
                            {
                                await Console.Error.WriteLineAsync($"initializing lastInterrupt");
                                lastInterrupt = DateTime.Now.Ticks;
                            }
                            else
                            {
                                await Console.Error.WriteLineAsync($"checking lastInterrupt");
                                if (DateTime.Now.Ticks <= lastInterrupt + 500_000_000)  // 500ms
                                {
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    await Console.Error.WriteLineAsync($"not fast enough {DateTime.Now.Ticks - lastInterrupt}");
                                    lastInterrupt = 0;
                                }
                            }
                        }

                        // store the character and mark as unread
                        memory[0xc000] = (byte)((input.KeyChar & 0xff) | 0x80);

                        // set the strobe
                        memory[0xc010] = 0x80;
                    }
                }
            );

            var cpu = new Cpu65C02(
                memory,
                (cpu, programCounter) => { },
                (cpu) =>
                {
                });

            Console.Clear();
            Console.SetCursorPosition(0, 0);

            cpu.Reset();
            cpu.Run(stopOnBreak: true, writeInstructions: options.VerboseCpu, stepsPerSecond: options.StepsPerSecond);

            return 0;
        }
    }
}
