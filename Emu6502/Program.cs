using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using InnoWerks.Emulators.Apple;
using InnoWerks.Simulators;

#pragma warning disable CA1859, CS0169, CA1823, IDE0005

namespace Emu6502
{
    internal sealed class Program
    {
        private static bool keepRunning = true;

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
            var mainRom = File.ReadAllBytes("roms/apple2e.rom");
            Console.WriteLine($"ROM is {mainRom.Length} bytes");

            byte[] diskIIRom = File.ReadAllBytes("roms/DiskII.rom");
            Console.WriteLine($"diskIIRom is {diskIIRom.Length} bytes");

            // Some ROMs are 256 bytes, some are larger.
            // If larger, extract first 256 bytes.
            if (diskIIRom.Length > 256)
            {
                var trimmed = new byte[256];
                Array.Copy(diskIIRom, trimmed, 256);
                diskIIRom = trimmed;
            }

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

            // create the devices
            var keyboard = new KeyboardDevice();
            var diskII = new DiskIISlotDevice(diskIIRom);

            // create the bus
            var bus = new AppleBus(config);

            // add the devices to the bus
            bus.AddDevice(keyboard);
            bus.AddDevice(diskII);

            Task.Run(() =>
            {
                while (keepRunning)
                {
                    if (!Console.KeyAvailable)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    var key = Console.ReadKey(intercept: true);

                    byte ascii = key.KeyChar switch
                    {
                        '\r' => 0x0D,
                        '\n' => 0x0D,
                        _ => (byte)char.ToUpperInvariant(key.KeyChar)
                    };

                    keyboard.InjectKey(MapToAppleKey(key));

                    // var keyInfo = Console.ReadKey(intercept: true);
                    // var appleKey = MapToAppleKey(keyInfo);

                    // if (appleKey != 0)
                    //     keyboard.KeyDown(appleKey);
                }
            });


            bus.LoadProgramToRom(mainRom);

            var cpu = new Cpu65C02(
                bus,
                (cpu, programCounter) => { },
                (cpu) =>
                {
                    if (keepRunning == false)
                    {
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
                    if (options.SingleStep == true)
                    {
                        Console.Write("> ");
                        Console.ReadKey();
                    }

                    cpu.Step(writeInstructions: options.SingleStep);
                }

                if (options.SingleStep == false) { renderer.Render(); }

                Thread.Sleep(16);
            }

            Console.ResetColor();
            Console.CursorVisible = true;

            // not-reached
#pragma warning disable CS0162 // Unreachable code detected
            return 0;
#pragma warning restore CS0162 // Unreachable code detected
        }

        static byte MapToAppleKey(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Enter)
                return 0x8D;

            if (key.Key == ConsoleKey.Backspace)
                return 0x88;

            if (key.Key == ConsoleKey.Escape)
                return 0x9B;

            char c = char.ToUpperInvariant(key.KeyChar);

            if (c >= 0x20 && c <= 0x7E)
                return (byte)c;

            return 0;
        }
    }
}
