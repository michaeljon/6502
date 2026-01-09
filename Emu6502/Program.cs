using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CommandLine;
using InnoWerks.Emulators.Apple;
using InnoWerks.Simulators;

#pragma warning disable CA1859, CS0169, CA1823, IDE0005

namespace Emu6502
{
    internal sealed class Program
    {
        private static bool keepRunning = true;

        private const bool VerboseSteps = false;

        private static int stepIndex;

        private static readonly List<string> steps = ["|", "/", "-", "\\"];

        public static void Main(string[] args)
        {
            Console.TreatControlCAsInput = false;

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
            var bytes = File.ReadAllBytes(options.RomFile);
            Console.WriteLine($"ROM is {bytes.Length} bytes");

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                keepRunning = false;

                Console.WriteLine("Interrupt received.");

                Console.CursorVisible = true;
                Environment.Exit(0);
            };

            var config = new AppleConfiguration
            {
                Model = AppleModel.AppleIIe,
                HasAuxMemory = true
            };

            var bus = new AppleBus(config);
            bus.LoadProgram(bytes, options.Location);

            // power up initialization
            bus[MosTechnologiesCpu.RstVectorH] = (byte)((options.Location & 0xff00) >> 8);
            bus[MosTechnologiesCpu.RstVectorL] = (byte)(options.Location & 0xff);

            var cpu = new Cpu65C02(
                bus,
                (cpu, programCounter) => { },
                (cpu) =>
                {
                    if (keepRunning == false)
                    {
                        Console.Clear();
                        Console.CursorVisible = true;
                        Environment.Exit(0);
                    }
                });

            cpu.Reset();

            var renderer = new AppleTextConsoleRenderer(bus, bus.SoftSwitches);

            Console.CursorVisible = false;
            Console.Clear();

            while (keepRunning)
            {
                // Run roughly one frame worth of cycles
                ulong target = bus.CycleCount + 17030;

                while (bus.CycleCount < target)
                {
                    cpu.Step(writeInstructions: VerboseSteps);
                    // Console.ReadKey();
                }

                renderer.RenderPage(1);

                // Console.SetCursorPosition(0, 0);
                // Console.Write("{0} -- {1}", steps[stepIndex++ % steps.Count], bus.CycleCount);

                Thread.Sleep(16);
            }

            Console.ResetColor();
            Console.CursorVisible = true;

            // not-reached
#pragma warning disable CS0162 // Unreachable code detected
            return 0;
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
