// #define DUMP_TEST_DATA
#define POST_STEP_MEMORY
#define VALIDATE_BUS_ACCESSES

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;
using InnoWerks.Processors;
using System;
using System.Globalization;
using System.IO;

#if VALIDATE_BUS_ACCESSES
using System.Linq;
#endif

#pragma warning disable CA1002, CA1822, CA1508

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class HarteBase : TestBase
    {
        protected virtual string BasePath { get; }

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

        protected void RunNamedBatch(CpuClass cpuClass, string batch)
        {
            if (string.IsNullOrEmpty(batch))
            {
                Assert.Inconclusive("No batch name provided to RunNamedBatch");
                return;
            }

            List<string> results = [];

            var file = $"{BasePath}/{batch}.json";

            using (var fs = File.OpenRead(file))
            {
                if (fs.Length == 0)
                {
                    return;
                }

                foreach (var test in JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, SerializerOptions).ToList())
                {
                    RunIndividualTest(cpuClass, test, results);
                }

#if VERBOSE_BATCH_OUTPUT
                foreach (var result in results)
                {
                    TestContext.WriteLine(result);
                }
#endif

                Assert.IsTrue(results.Count == 0, $"Failed with {results.Count} messages");
            }
        }

        protected bool RunIndividualTest(CpuClass cpuClass, JsonHarteTestStructure test, List<string> results)
        {
            ArgumentNullException.ThrowIfNull(test);
            ArgumentNullException.ThrowIfNull(results);

            var batch = test.Name.Split(' ')[0];
            var ocd = cpuClass == CpuClass.WDC6502 ?
                CpuInstructions.OpCode6502[byte.Parse(batch, NumberStyles.HexNumber, CultureInfo.InvariantCulture)] :
                CpuInstructions.OpCode65C02[byte.Parse(batch, NumberStyles.HexNumber, CultureInfo.InvariantCulture)];

            var memory = new AccessCountingMemory();

            // set up initial memory state
            memory.Initialize(test.Initial.Ram);

            var cpu = new Cpu(
                cpuClass,
                memory,
                // (cpu, pc) => FlagsTraceCallback(cpu, pc, memory),
                // (cpu) => FlagsLoggerCallback(cpu, memory, 0))
                (cpu, pc) => DummyTraceCallback(cpu, pc, memory),
                (cpu) => DummyLoggerCallback(cpu, memory, 0));

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
            cpu.Step(writeInstructions: false);

            var finalRegisters = new Registers()
            {
                ProgramCounter = test.Final.ProgramCounter,
                StackPointer = test.Final.S,
                A = test.Final.A,
                X = test.Final.X,
                Y = test.Final.Y,
                ProcessorStatus = test.Final.P,
            };

            var testFailed = false;

            // verify results
            if (test.Final.ProgramCounter != cpu.Registers.ProgramCounter) { testFailed = true; results.Add($"{test.Name}: ProgramCounter expected {test.Final.ProgramCounter:X4} actual {cpu.Registers.ProgramCounter:X4}"); }

            // we can run these tests to this extent
            if (ocd.AddressingMode != AddressingMode.Unknown)
            {
                if (test.Final.S != cpu.Registers.StackPointer) { testFailed = true; results.Add($"{test.Name}: StackPointer expected {test.Final.S:X2} actual {cpu.Registers.StackPointer:X2}"); }
                if (test.Final.A != cpu.Registers.A) { testFailed = true; results.Add($"{test.Name}: A expected {test.Final.A:X2} actual {cpu.Registers.A:X2}"); }
                if (test.Final.X != cpu.Registers.X) { testFailed = true; results.Add($"{test.Name}: X expected {test.Final.X:X2} actual {cpu.Registers.Y:X2}"); }
                if (test.Final.Y != cpu.Registers.Y) { testFailed = true; results.Add($"{test.Name}: Y expected {test.Final.Y:X2} actual {cpu.Registers.X:X2}"); }
                if (test.Final.P != cpu.Registers.ProcessorStatus) { testFailed = true; results.Add($"{test.Name}: ProcessorStatus expected {test.Final.P:X2} actual {cpu.Registers.ProcessorStatus:X2}"); }

#if POST_STEP_MEMORY
                // verify memory
                (var ramMatches, var ramDiffersAtAddr, byte ramExpectedValue, byte ramActualValue) =
                    memory.ValidateMemory(test.Final.Ram);
                if (ramMatches == false) { testFailed = true; results.Add($"{test.Name}: Expected memory at {ramDiffersAtAddr} to be {ramExpectedValue} but is {ramActualValue}"); }
#endif

#if VALIDATE_BUS_ACCESSES
                // verify bus accesses
                if (test.BusAccesses.Count() != memory.BusAccesses.Count)
                {
                    { testFailed = true; results.Add($"{test.Name}: Expected {test.BusAccesses.Count()} memory accesses but got {memory.BusAccesses.Count} instead "); }
                }
                else
                {
                    (var cyclesMatches, var cyclesDiffersAtAddr, var cyclesExpectedValue, var cyclesActualValue) =
                        memory.ValidateCycles(test.BusAccesses);
                    if (cyclesMatches == false) { testFailed = true; results.Add($"{test.Name}: Expected access at {cyclesDiffersAtAddr} to be {cyclesExpectedValue} but is {cyclesActualValue}"); }
                }
#endif
            }

#if DUMP_TEST_DATA
            if (testFailed == true)
            {
                TestContext.WriteLine("");
                TestContext.WriteLine($"{((testFailed == true) ? "Failed" : "Passed")} TestName:     {test.Name}");
                TestContext.WriteLine($"OpCode:              ${batch} {ocd.OpCode} {ocd.AddressingMode}");
                TestContext.WriteLine($"Initial registers    {initialRegisters}");
                TestContext.WriteLine($"Expected registers   {finalRegisters}");
                TestContext.WriteLine($"Computed registers   {cpu.Registers}");

                TestContext.WriteLine("Expected bus accesses");
                var time = 0;
                foreach (var busAccess in test.BusAccesses)
                {
                    TestContext.WriteLine($"T{time++}: {busAccess}");
                }

                TestContext.WriteLine("Actual bus accesses");
                time = 0;
                foreach (var busAccess in memory.BusAccesses)
                {
                    TestContext.WriteLine($"T{time++}: {busAccess}");
                }
            }
#endif

            return testFailed;
        }

        protected static bool[] LoadIgnored(CpuClass cpuClass)
        {
            var ignored = new bool[256];

            if (cpuClass == CpuClass.WDC6502)
            {
                // from https://www.masswerk.at/nowgobang/2021/6502-illegal-opcodes
                foreach (var kill in (byte[])([0x12, 0x22, 0x32, 0x42, 0x52, 0x62, 0x72, 0x92, 0xB2, 0xD2, 0xF2]))
                {
                    ignored[kill] = true;
                }
            }

            return ignored;
        }
    }
}
