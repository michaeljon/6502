using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;
using InnoWerks.Processors;
using System;

#pragma warning disable CA1002

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class HarteBase : TestBase
    {
        protected static readonly JsonSerializerOptions SerializerOptions = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = false,
            IgnoreReadOnlyFields = false,
            AllowTrailingCommas = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new HarteCycleConverter(),
                new HarteRamConverter()
            }
        };

        protected static void RunIndividualTest(CpuClass cpuClass, JsonHarteTestStructure test, List<string> results)
        {
            ArgumentNullException.ThrowIfNull(test);
            ArgumentNullException.ThrowIfNull(results);

            var memory = new AccessCountingMemory();

            // set up initial memory state
            memory.Initialize(test.Initial.Ram);

            var cpu = new Cpu(
                cpuClass,
                memory,
                // (cpu, pc) => FlagsTraceCallback(cpu, pc, memory),
                // (cpu) => FlagsLoggerCallback(cpu, memory, 0))
                (cpu, pc) => DummyTraceCallback(cpu, pc, memory),
                (cpu) => DummyLoggerCallback(cpu, memory, 0))
            {
                SkipTimingWait = true
            };

            cpu.Reset();

            // initialize processor
            cpu.Registers.ProgramCounter = test.Initial.ProgramCounter;
            cpu.Registers.StackPointer = test.Initial.S;
            cpu.Registers.A = test.Initial.A;
            cpu.Registers.X = test.Initial.X;
            cpu.Registers.Y = test.Initial.Y;
            cpu.Registers.ProcessorStatus = test.Initial.P;

            // initial registers in local format
            var initialRegisters = new Registers()
            {
                ProgramCounter = test.Initial.ProgramCounter,
                StackPointer = test.Initial.S,
                A = test.Initial.A,
                X = test.Initial.X,
                Y = test.Initial.Y,
                ProcessorStatus = test.Initial.P,
            };

            // run test
            cpu.Step(stopOnBreak: true, writeInstructions: false);

            var finalRegisters = new Registers()
            {
                ProgramCounter = test.Final.ProgramCounter,
                StackPointer = test.Final.S,
                A = test.Final.A,
                X = test.Final.X,
                Y = test.Final.Y,
                ProcessorStatus = test.Final.P,
            };

#if DUMP_TEST_DATA
            Console.WriteLine($"TestName {test.Name}");
            Console.WriteLine($"Initial registers  {initialRegisters}");
            Console.WriteLine($"Expected registers {finalRegisters}");
            Console.WriteLine($"Actual registers   {cpu.Registers}");

            foreach (var busAccess in memory.BusAccesses)
            {
                Console.WriteLine(busAccess);
            }
#endif

            // verify results
            if (test.Final.ProgramCounter != cpu.Registers.ProgramCounter) results.Add($"{test.Name}: ProgramCounter expected {test.Final.ProgramCounter:X4} actual {cpu.Registers.ProgramCounter:X4}");
            if (test.Final.S != cpu.Registers.StackPointer) results.Add($"{test.Name}: StackPointer expected {test.Final.S:X2} actual {cpu.Registers.StackPointer:X2}");
            if (test.Final.A != cpu.Registers.A) results.Add($"{test.Name}: A expected {test.Final.A:X2} actual {cpu.Registers.A:X2}");
            if (test.Final.X != cpu.Registers.X) results.Add($"{test.Name}: X expected {test.Final.X:X2} actual {cpu.Registers.Y:X2}");
            if (test.Final.Y != cpu.Registers.Y) results.Add($"{test.Name}: Y expected {test.Final.Y:X2} actual {cpu.Registers.X:X2}");
            if (test.Final.P != cpu.Registers.ProcessorStatus) results.Add($"{test.Name}: ProcessorStatus expected {test.Final.P:X2} actual {cpu.Registers.ProcessorStatus:X2}");

            // verify memory
            (var ramMatches, var ramDiffersAtAddr, byte ramExpectedValue, byte ramActualValue) =
                memory.ValidateMemory(test.Final.Ram);
            if (ramMatches == false) results.Add($"{test.Name}: Expected memory at {ramDiffersAtAddr} to be {ramExpectedValue} but is {ramActualValue}");

            // verify bus accesses
            // if (test.BusAccesses.Count() != memory.BusAccesses.Count)
            // {
            //     results.Add($"{test.Name}: Expected {test.BusAccesses.Count()} memory accesses but got {memory.BusAccesses.Count} instead ");
            // }
            // else
            // {
            //     (var cyclesMatches, var cyclesDiffersAtAddr, var cyclesExpectedValue, var cyclesActualValue) =
            //         memory.ValidateCycles(test.BusAccesses);
            //     if (cyclesMatches == false) results.Add($"{test.Name}: Expected access at {cyclesDiffersAtAddr} to be {cyclesExpectedValue} but is {cyclesActualValue}");
            // }
        }

        protected static bool[] LoadIgnored(CpuClass cpuClass)
        {
            var ignored = new bool[256];

            for (var o = 0; o < 256; o++)
            {
                if (cpuClass == CpuClass.WDC6502)
                {
                    ignored[o] = CpuInstructions.OpCode6502[(byte)o].AddressingMode == AddressingMode.Unknown;
                }
                else
                {
                    ignored[o] = CpuInstructions.OpCode65C02[(byte)o].AddressingMode == AddressingMode.Unknown;
                }
            }

            return ignored;
        }
    }
}
