// #define REPORT_IO_ADDRESS_USAGE

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using InnoWerks.Emulators.Apple;
using InnoWerks.Processors;
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
            var diskIIRom = File.ReadAllBytes("roms/DiskII.rom");

            var dos33 = File.ReadAllBytes("disks/dos33.dsk");

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                keepRunning = false;

                Console.WriteLine("Interrupt received.");

                Console.CursorVisible = true;
                Environment.Exit(0);
            };

            var config = new AppleConfiguration(AppleModel.AppleIIe)
            {
                CpuClass = CpuClass.WDC65C02,
                HasAuxMemory = true,
                Has80Column = false,
                HasLowercase = false,
                RamSize = 64
            };

            var softSwitches = new SoftSwitches();

            // create the bus
            var bus = new AppleBus(config, softSwitches);

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

            // create the devices
            var keyboard = new Keyboard(softSwitches);
            var display = new Display(bus, softSwitches);

            // add system the devices to the bus
            bus.AddDevice(keyboard);
            bus.AddDevice(display);
            bus.AddDevice(new SlotRomSoftSwitchHandler(softSwitches));
            bus.AddDevice(new Annunciators(softSwitches));
            bus.AddDevice(new Paddles(softSwitches));
            bus.AddDevice(new Cassette(softSwitches));
            bus.AddDevice(new Speaker(softSwitches));
            bus.AddDevice(new Strobe(softSwitches));

            // // add slotted devices
            // var diskDevice = new DiskIISlotDevice(softSwitches, diskIIRom);
            // DiskIINibble.LoadDisk(diskDevice.GetDrive(1), dos33);
            // bus.AddDevice(diskDevice);

#if REPORT_IO_ADDRESS_USAGE == true
            var readConfigured = new string[0xC080 - 0xC000];
            foreach (var (address, name) in bus.ConfiguredAddresses(true).OrderBy(p => p.address))
            {
                readConfigured[address - 0xC000] = name;

                SimDebugger.Info("[R] {0:X4} -- {1}\n", address, name);
            }

            var writeConfigured = new string[0xC080 - 0xC000];
            foreach (var (address, name) in bus.ConfiguredAddresses(false).OrderBy(p => p.address))
            {
                writeConfigured[address - 0xC000] = name;

                SimDebugger.Info("[W] {0:X4} -- {1}\n", address, name);
            }

            for (var address = 0xC000; address < 0xC080; address++)
            {
                if (string.IsNullOrEmpty(readConfigured[address - 0xC000]))
                {
                    SimDebugger.Info("Missing read: {0:X4}\n", address);
                }
            }

            for (var address = 0xC000; address < 0xC080; address++)
            {
                if (string.IsNullOrEmpty(writeConfigured[address - 0xC000]))
                {
                    SimDebugger.Info("Missing write: {0:X4}\n", address);
                }
            }

            for (var address = 0xC000; address < 0xC080; address++)
            {
                if (string.IsNullOrEmpty(readConfigured[address - 0xC000]) && string.IsNullOrEmpty(writeConfigured[address - 0xC000]))
                {
                    SimDebugger.Info("Missing combined: {0:X4}\n", address);
                }
            }
#endif

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

                    keyboard.InjectKey(MapToAppleKey(key));
                }
            });


            bus.LoadProgramToRom(mainRom);

            cpu.Reset();

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
                        var (opcode, decode) = cpu.PeekInstruction();

                        Console.Write(decode);
                        Console.Write("> ");
                        var key = Console.ReadKey();
                        if (key.KeyChar == 'G')
                        {
                            options.SingleStep = false;
                        }
                    }

                    cpu.Step(writeInstructions: options.Trace);

                    if (options.SingleStep == true)
                    {
                        Console.WriteLine(cpu.Registers);
                    }
                }

                if (options.SingleStep == false) { display.Render(); }

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
