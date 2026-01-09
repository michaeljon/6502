using System;
using System.IO;
using System.Threading;
using CommandLine;
using InnoWerks.Emulators.Apple;
using InnoWerks.Simulators;

#pragma warning disable CA1859

namespace Emu6502
{
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            Console.TreatControlCAsInput = true;

            var result = Parser.Default.ParseArguments<CliOptions>(args);

            result.MapResult(
                RunEmulator,

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

        private static int RunEmulator(CliOptions options)
        {
            Console.CursorVisible = false;
            Console.Clear();

            var bytes = File.ReadAllBytes(options.RomFile);
            Console.WriteLine($"ROM is {bytes.Length} bytes");

            var config = new AppleConfiguration
            {
                Model = AppleModel.AppleIIe,
                HasAuxMemory = true
            };

            var bus = new AppleBus(config);
            bus.LoadProgram(bytes, options.Location);

            var cpu = new Cpu65C02(
                bus,
                (cpu, programCounter) => { },
                (cpu) =>
                { });

            cpu.Reset();

            var renderer = new AppleTextConsoleRenderer(bus, bus.SoftSwitches);

            while (true)
            {
                // Run roughly one frame worth of cycles
                long target = bus.CycleCount + 17030;

                while (bus.CycleCount < target)
                    cpu.Step();

                renderer.Render();

                Thread.Sleep(16);
            }

            // not-reached
#pragma warning disable CS0162 // Unreachable code detected
            return 0;
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
