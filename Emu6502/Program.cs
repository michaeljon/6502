using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using InnoWerks.Assemblers;
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

            var audit = File.ReadAllBytes("tests/audit.o");

            var config = new AppleConfiguration(AppleModel.AppleIIe)
            {
                CpuClass = CpuClass.WDC65C02,
                HasAuxMemory = true,
                Has80Column = false,
                HasLowercase = false,
                RamSize = 64
            };

            var machineState = new MachineState();
            var memoryBlocks = new MemoryBlocks(machineState);

            var bus = new AppleBus(config, memoryBlocks, machineState);
            var iou = new IOU(memoryBlocks, machineState, bus);
            var mmu = new MMU(machineState, bus);

            // var disk = new DiskIISlotDevice(bus, machineState, diskIIRom);
            // DiskIINibble.LoadDisk(disk.GetDrive(1), dos33);

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

            foreach (var (address, name) in SoftSwitchAddress.Lookup.OrderBy(a => a.Key))
            {
                bool assigned = iou.HandlesRead(address) || iou.HandlesWrite(address) || mmu.HandlesRead(address) || mmu.HandlesWrite(address);
                if (assigned == false)
                {
                    SimDebugger.Info("Address {0:X4} ({1}) is not assigned to any device", address, name);
                }
            }

            var keyListener = Task.Run(() =>
            {
                while (keepRunning)
                {
                    if (!Console.KeyAvailable)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    var key = Console.ReadKey(intercept: true);

                    iou.InjectKey(MapToAppleKey(key));
                }
            });

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;

                Console.CursorVisible = true;

                Console.SetCursorPosition(0, 25);
                Console.WriteLine("Interrupt received.");
                Console.WriteLine(cpu.Registers);
                Console.Write("[QINRST]> ");

                var key = Console.ReadKey();

                switch (key.KeyChar)
                {
                    case 'Q':
                    case 'q':
                        keepRunning = false;

                        Console.CursorVisible = true;
                        Environment.Exit(0);
                        break;

                    case 'I':
                    case 'i':
                        cpu.IRQ();
                        break;

                    case 'N':
                    case 'n':
                        cpu.NMI();
                        break;

                    case 'R':
                    case 'r':
                        cpu.Reset();
                        break;

                    case 'S':
                    case 's':
                        options.SingleStep = !options.SingleStep;
                        break;

                    case 'T':
                    case 't':
                        options.Trace = !options.Trace;
                        break;
                }

                Console.CursorVisible = false;
            };

            bus.LoadProgramToRom(mainRom);
            bus.LoadProgramToRam(audit, 0x6000);

            cpu.Reset();

            Console.CursorVisible = false;
            Console.Clear();

            while (keepRunning)
            {
                // Run roughly one frame worth of cycles
                ulong target = bus.CycleCount + 17030;

                while (bus.CycleCount < target)
                {
                    // if (cpu.Registers.ProgramCounter == 0x6ced)
                    // {
                    //     options.SingleStep = true;
                    // }

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

                if (options.SingleStep == false) { iou.Render(); }

                Thread.Sleep(16);
            }

            Console.ResetColor();
            Console.CursorVisible = true;

            // not-reached
#pragma warning disable CS0162 // Unreachable code detected
            return 0;
#pragma warning restore CS0162 // Unreachable code detected
        }

        // todo: use apple iie ref table 2-3 to construct full mapping
        static byte MapToAppleKey(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Backspace)
                return 0x08;

            if (key.Key == ConsoleKey.LeftArrow)
                return 0x08;

            if (key.Key == ConsoleKey.RightArrow)
                return 0x15;

            if (key.Key == ConsoleKey.UpArrow)
                return 0x0B;

            if (key.Key == ConsoleKey.DownArrow)
                return 0x0A;

            // char c = char.ToUpperInvariant(key.KeyChar);

            // if (c >= 0x20 && c <= 0x7E)
            //     return (byte)c;

            return (byte)key.KeyChar;
        }
    }
}
