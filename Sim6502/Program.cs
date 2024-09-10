using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using InnoWerks.Assemblers;
using InnoWerks.Simulators;

#pragma warning disable CA1822, RCS1213, SYSLIB1045

namespace Sim6502
{
    internal sealed class Program
    {
        private static bool keepRunning = true;

        private readonly Memory memory = new();

        private readonly Dictionary<ushort, byte> breakpoints = new();

        private int stepSpeed = 1;

        private bool verboseSteps = true;

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                keepRunning = false;

                Console.WriteLine("Interrupt received.");
            };

            var result = Parser.Default.ParseArguments<CliOptions>(args);

            result.MapResult(
                o => new Program().RunSimulator(o),

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

        private int RunSimulator(CliOptions options)
        {
            var programLines = File.ReadAllLines(options.Input);
            var assembler = new Assembler(
                programLines,
                options.Origin
            );
            assembler.Assemble();

            memory.LoadProgram(assembler.ObjectCode, options.Origin);

            Console.WriteLine($"Debugging {options.Input}");
            Console.WriteLine("? for help");

            var cpu = new Cpu(
                options.CpuClass,
                memory,
                (cpu, programCounter) => { },
                (cpu) =>
                {
                    Console.WriteLine();
                    Console.WriteLine($"PC:{cpu.Registers.ProgramCounter:X4} {cpu.Registers.GetRegisterDisplay} {cpu.Registers.InternalGetFlagsDisplay}");
                });

            // power up initialization
            memory[Cpu.RstVectorH] = (byte)((options.Origin & 0xff00) >> 8);
            memory[Cpu.RstVectorL] = (byte)(options.Origin & 0xff);

            cpu.Reset();

            DebugTheThing(cpu, assembler);

            return 0;
        }

        /*
         * Commands:
         *
         *    q                     - quit
         *
         *    s                     - step
         *    t <steps>             - run n <steps>
         *    g                     - go
         *    pc <addr>             - set PC to <addr> (PC <- addr)
         *    jsr <addr>            - call subroutine ad <addr> (S <- PC + 2, PC <- addr)
         *    sb <addr>             - set breakpoint at <addr>
         *    cb <addr>             - clear breakpoint at <addr>
         *    ca                    - clear all breakpoints
         *    b                     - list breakpoints
         *    sf <flag>             - set flag (CVNZ) to true
         *    cf <flag>             - set flag (CVNZ) to false
         *
         *    f                     - dump flags
         *    e                     - dump registers
         *    sr <reg> <value>      - set register (A,X,Y,S) to value
         *    zr <reg>              - set register (A,X,Y,S) to 0 (shortcut)
         *
         *    w <addr> <byte>...    - write <byte> starting at <addr>
         *    r <addr> <len>        - read <len>
         *    d <page>              - dump page <page>
         *
         *    o ts <steps/sec>      - set trace speed to steps / second
         *    o tv <true|false>     - show cpu instructions during trace / run
         *
         *    ? | h                 - display this list
         */
        private void DebugTheThing(Cpu cpu, Assembler assembler)
        {
            // commands
            var quitRegex = new Regex("^q$");

            var stepRegex = new Regex("^s$");
            var traceRegex = new Regex("^t (?<steps>[0-9]+)$");
            var goRegex = new Regex("^g$");
            var setProgramCounterRegex = new Regex("^jmp (?<addr>[a-f0-9]{4})$");
            var jsrRegex = new Regex("^jsr (?<addr>[a-f0-9]{4})$");
            var setBreakpointRegex = new Regex("^sb (?<addr>[a-f0-9a-f]{4})$");
            var clearBreakpointRegex = new Regex("^cb (?<addr>[a-f0-9]{4})$");
            var clearAllBreakpointsRegex = new Regex("^ca$");
            var listBreakpointsRegex = new Regex("^b$");
            var setFlagRegex = new Regex("^sf (?<flag>[cnvz])$");
            var clearFlagRegex = new Regex("^cf (?<flag>[cnvz])$");

            var dumpFlagsRegex = new Regex("^f$");
            var dumpRegistersRegex = new Regex("^e$");
            var setRegisterRegex = new Regex("^sr (?<register>[axys]) (?<value>[a-f0-9]{2})$");
            var zeroRegisterRegex = new Regex("^zr (?<register>[axys])$");

            var writeRegex = new Regex("^w (?<addr>[a-f0-9]{4}) (?<value>[a-f0-9]{2})$");
            var readRegex = new Regex("^r (?<addr>[a-f0-9]{4}) (?<len>[0-9]+)$");
            var writePageRegex = new Regex("^d (?<page>[a-f0-9]{2})$");

            // options
            var setTraceSpeedRegex = new Regex("^o ts (?<speed>[0-9]+)$");
            var setTraceVerbosityRegex = new Regex("^o tv (?<flag>(true|false))$");

            // help
            var helpRegex = new Regex("^(\\?|h)$");

            var simulationComplete = false;

            while (simulationComplete == false)
            {
                // set the flag to keep going, but be ready to jump
                keepRunning = true;

                var programCounter = cpu.Registers.ProgramCounter;

                if (assembler.ProgramByAddress != null)
                {
                    if (assembler.ProgramByAddress.TryGetValue(programCounter, out var lineInformation))
                    {
                        Console.WriteLine($"{lineInformation.EffectiveAddress:X4} | {lineInformation.MachineCodeAsString,-10}| {lineInformation.RawInstructionText}");
                    }
                    else
                    {
                        Console.WriteLine($"{programCounter:X4} | {{no assembly found}}");
                    }
                }

                Console.Write("<dbg> ");
                var command = Console.ReadLine().ToLowerInvariant();

                if (quitRegex.IsMatch(command) == true)
                {
                    simulationComplete = true;
                }
                else if (stepRegex.IsMatch(command) == true)
                {
                    Step(cpu);
                }
                else if (traceRegex.IsMatch(command) == true)
                {
                    var captures = traceRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("steps", out string steps);

                    Trace(cpu, int.Parse(steps, CultureInfo.InvariantCulture));
                }
                else if (goRegex.IsMatch(command) == true)
                {
                    Go(cpu);
                }
                else if (setProgramCounterRegex.IsMatch(command) == true)
                {
                    var captures = setProgramCounterRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("addr", out string addr);

                    cpu.Registers.ProgramCounter = ushort.Parse(addr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
                else if (jsrRegex.IsMatch(command) == true)
                {
                    var captures = jsrRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("addr", out string addr);

                    cpu.StackPush((byte)(((cpu.Registers.ProgramCounter + 2) & 0xff00) >> 8));
                    cpu.StackPush((byte)((cpu.Registers.ProgramCounter + 2) & 0x00ff));

                    cpu.Registers.ProgramCounter = ushort.Parse(addr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
                else if (setBreakpointRegex.IsMatch(command) == true)
                {
                    var captures = setBreakpointRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("addr", out string addr);

                    SetBreakpoint(cpu, ushort.Parse(addr, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                }
                else if (clearBreakpointRegex.IsMatch(command) == true)
                {
                    var captures = clearBreakpointRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("addr", out string addr);

                    ClearBreakpoint(cpu, ushort.Parse(addr, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                }
                else if (clearAllBreakpointsRegex.IsMatch(command) == true)
                {
                    ClearAllBreakpoints(cpu);
                }
                else if (listBreakpointsRegex.IsMatch(command) == true)
                {
                    ListBreakpoints(cpu);
                }
                else if (setFlagRegex.IsMatch(command) == true)
                {
                    var captures = setFlagRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("flag", out string flag);

                    SetFlag(cpu, Enum.Parse<ProcessorFlag>(flag, true));
                }
                else if (clearFlagRegex.IsMatch(command) == true)
                {
                    var captures = clearFlagRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("flag", out string flag);

                    ClearFlag(cpu, Enum.Parse<ProcessorFlag>(flag, true));
                }
                else if (dumpFlagsRegex.IsMatch(command) == true)
                {
                    DumpFlags(cpu);
                }
                else if (dumpRegistersRegex.IsMatch(command) == true)
                {
                    DumpRegisters(cpu);
                }
                else if (setRegisterRegex.IsMatch(command) == true)
                {
                    var captures = setRegisterRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("register", out string register);
                    captures.TryGetValue("value", out string value);

                    SetRegister(cpu, Enum.Parse<ProcessorRegister>(register), byte.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                }
                else if (zeroRegisterRegex.IsMatch(command) == true)
                {
                    var captures = zeroRegisterRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("register", out string register);

                    ZeroRegister(cpu, Enum.Parse<ProcessorRegister>(register));
                }
                else if (writeRegex.IsMatch(command) == true)
                {
                    var captures = writeRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("addr", out string addr);
                    captures.TryGetValue("value", out string value);

                    WriteMemory(
                        cpu,
                        ushort.Parse(addr, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                        [byte.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)]
                        );
                }
                else if (readRegex.IsMatch(command) == true)
                {
                    var captures = readRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("addr", out string addr);
                    captures.TryGetValue("len", out string len);

                    ReadMemory(
                        cpu,
                        ushort.Parse(addr, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                        ushort.Parse(len, CultureInfo.InvariantCulture)
                    );
                }
                else if (writePageRegex.IsMatch(command) == true)
                {
                    var captures = writePageRegex.MatchNamedCaptures(command);

                    captures.TryGetValue("page", out string page);
                }
                else if (setTraceSpeedRegex.IsMatch(command) == true)
                {
                    var captures = setTraceSpeedRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("speed", out string speed);

                    stepSpeed = int.Parse(speed, CultureInfo.InvariantCulture);
                }
                else if (setTraceVerbosityRegex.IsMatch(command) == true)
                {
                    var captures = setTraceVerbosityRegex.MatchNamedCaptures(command);
                    captures.TryGetValue("flag", out string flag);

                    verboseSteps = bool.Parse(flag);
                }
                else if (helpRegex.IsMatch(command) == true)
                {
                    PrintHelp();
                }
                else
                {
                    Console.WriteLine($"Unknown command {command}");
                }
            }
        }

        private bool Step(Cpu cpu)
        {
            var breakEncountered = cpu.Step(writeInstructions: verboseSteps, returnPriorToBreak: true);

            if (breakEncountered)
            {
                Console.WriteLine($"BRK encountered at ${cpu.Registers.ProgramCounter:X4}");
            }

            return breakEncountered;
        }

        private bool Trace(Cpu cpu, int steps)
        {
            var breakEncountered = false;

            // well, we might as well run for a while
            if (steps == 0)
            {
                steps = int.MaxValue;
            }

            for (var step = 0; step < steps; step++)
            {
                breakEncountered = cpu.Step(writeInstructions: verboseSteps, returnPriorToBreak: true);

                if (breakEncountered)
                {
                    Console.WriteLine($"BRK encountered at ${cpu.Registers.ProgramCounter:X4}");
                    return breakEncountered;
                }
                else
                {
                    var t = Task.Run(async delegate
                                  {
                                      await Task.Delay(new TimeSpan((long)(1.0 / stepSpeed * 1000) * TimeSpan.TicksPerMillisecond));
                                      return 0;
                                  });
                    t.Wait();
                }

                if (keepRunning == false)
                {
                    return false;
                }
            }

            return breakEncountered;
        }

        private bool Go(Cpu cpu)
        {
            var breakEncountered = false;

            while (breakEncountered == false)
            {
                breakEncountered = cpu.Step(writeInstructions: verboseSteps, returnPriorToBreak: true);

                if (keepRunning == false)
                {
                    return false;
                }
            }

            Console.WriteLine($"BRK encountered at ${cpu.Registers.ProgramCounter:X4}");
            return breakEncountered;
        }

        private void SetBreakpoint(Cpu cpu, ushort addr)
        {
            breakpoints.Add(addr, memory[addr]);
            memory[addr] = 0x00;
        }

        private void ClearBreakpoint(Cpu cpu, ushort addr)
        {

            memory[addr] = breakpoints[addr];
            breakpoints.Remove(addr);
        }

        private void ClearAllBreakpoints(Cpu cpu)
        {
            foreach (var (addr, value) in breakpoints)
            {
                memory[addr] = value;
            }
        }

        private void ListBreakpoints(Cpu cpu)
        {
            foreach (var (addr, _) in breakpoints)
            {
                Console.WriteLine($"{addr}");
            }
        }

        private void SetFlag(Cpu cpu, ProcessorFlag processorFlag)
        {
            switch (processorFlag)
            {
                case ProcessorFlag.C:
                    cpu.Registers.Carry = true;
                    break;

                case ProcessorFlag.Z:
                    cpu.Registers.Zero = true;
                    break;

                case ProcessorFlag.V:
                    cpu.Registers.Overflow = true;
                    break;

                case ProcessorFlag.N:
                    cpu.Registers.Negative = true;
                    break;
            }
        }

        private void ClearFlag(Cpu cpu, ProcessorFlag processorFlag)
        {
            switch (processorFlag)
            {
                case ProcessorFlag.C:
                    cpu.Registers.Carry = false;
                    break;

                case ProcessorFlag.Z:
                    cpu.Registers.Zero = false;
                    break;

                case ProcessorFlag.V:
                    cpu.Registers.Overflow = false;
                    break;

                case ProcessorFlag.N:
                    cpu.Registers.Negative = false;
                    break;
            }
        }

        private void DumpFlags(Cpu cpu)
        {
            Console.WriteLine($"N: {(cpu.Registers.Negative ? '1' : '0')}");
            Console.WriteLine($"V: {(cpu.Registers.Overflow ? '1' : '0')}");
            Console.WriteLine($"-: {(cpu.Registers.Unused ? '1' : '0')}");
            Console.WriteLine($"D: {(cpu.Registers.Decimal ? '1' : '0')}");
            Console.WriteLine($"B: {(cpu.Registers.Break ? '1' : '0')}");
            Console.WriteLine($"I: {(cpu.Registers.Interrupt ? '1' : '0')}");
            Console.WriteLine($"Z: {(cpu.Registers.Zero ? '1' : '0')}");
            Console.WriteLine($"C: {(cpu.Registers.Carry ? '1' : '0')}");
        }

        private void DumpRegisters(Cpu cpu)
        {
            Console.WriteLine($"PC:{cpu.Registers.ProgramCounter:X4} {cpu.Registers.GetRegisterDisplay} {cpu.Registers.InternalGetFlagsDisplay}");
        }

        private void SetRegister(Cpu cpu, ProcessorRegister processorRegister, byte value)
        {
            switch (processorRegister)
            {
                case ProcessorRegister.A:
                    cpu.Registers.A = value;
                    break;

                case ProcessorRegister.X:
                    cpu.Registers.X = value;
                    break;

                case ProcessorRegister.Y:
                    cpu.Registers.Y = value;
                    break;

                case ProcessorRegister.S:
                    cpu.Registers.ProcessorStatus = value;
                    break;
            }
        }

        private void ZeroRegister(Cpu cpu, ProcessorRegister processorRegister)
        {
            switch (processorRegister)
            {
                case ProcessorRegister.A:
                    cpu.Registers.A = 0;
                    break;

                case ProcessorRegister.X:
                    cpu.Registers.X = 0;
                    break;

                case ProcessorRegister.Y:
                    cpu.Registers.Y = 0;
                    break;
            }
        }

        private void WriteMemory(Cpu cpu, ushort addr, List<byte> bytes)
        {
            for (var i = 0; i < bytes.Count; i++)
            {
                memory[(ushort)(addr + i)] = bytes[i];
            }
        }

        private void ReadMemory(Cpu cpu, ushort addr, ushort len)
        {
            // get page number
            // round down to 16 byte boundary
            // print blank values until start of dump
            // continue printing until we've written all the bytes

            // but for now
            for (var i = 0; i < len; i++)
            {
                Console.WriteLine($"{(addr + i):X4}: ${memory[(ushort)(addr + i)]:X2}");
            }
        }

        private void DumpPage(Cpu cpu, byte page)
        {
            var lines = 16;

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

        private void PrintHelp()
        {
            Console.WriteLine("    s                     - step");
            Console.WriteLine("    t <steps>             - run n <steps>");
            Console.WriteLine("    g                     - go");
            Console.WriteLine("    pc <addr>             - set PC to <addr> (PC <- addr)");
            Console.WriteLine("    jsr <addr>            - call subroutine ad <addr> (S <- PC + 2, PC <- addr)");
            Console.WriteLine("    sb <addr>             - set breakpoint at <addr>");
            Console.WriteLine("    cb <addr>             - clear breakpoint at <addr>");
            Console.WriteLine("    ca                    - clear all breakpoints");
            Console.WriteLine("    b                     - list breakpoints");
            Console.WriteLine("    sf <flag>             - set flag (CVNZ) to true");
            Console.WriteLine("    cf <flag>             - set flag (CVNZ) to false");
            Console.WriteLine("");
            Console.WriteLine("    f                     - dump flags");
            Console.WriteLine("    e                     - dump registers");
            Console.WriteLine("    sr <reg> <value>      - set register (A,X,Y,S) to value");
            Console.WriteLine("    zr <reg>              - set register (A,X,Y,S) to 0 (shortcut)");
            Console.WriteLine("");
            Console.WriteLine("    w <addr> <byte>...    - write <byte> starting at <addr>");
            Console.WriteLine("    r <addr> <len>        - read <len>");
            Console.WriteLine("    d <page>              - dump page <page>");
            Console.WriteLine("");
            Console.WriteLine("    o ts <steps/sec>      - set trace speed to steps / second");
            Console.WriteLine("");
            Console.WriteLine("    ? | h                 - display this list");
        }
    }
}
