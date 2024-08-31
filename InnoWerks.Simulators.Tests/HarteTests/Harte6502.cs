using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using InnoWerks.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable RCS1213

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class Harte6502 : TestBase
    {
        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = false,
            IgnoreReadOnlyFields = false,
            AllowTrailingCommas = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new HarteCycleConverter(),
                new HarteRamConverter()
            }
        };

        [TestMethod]
        public void RunAll6502Tests()
        {
            List<string> results = [];

            var files = Directory
                .GetFiles("/Users/michaeljon/src/6502/working/65x02/6502/v1", "*.json")
                .OrderBy(f => f);

            var ignored = LoadIgnored();

            foreach (var file in files)
            {
                using (var fs = File.OpenRead(file))
                {
                    var index = byte.Parse(Path.GetFileNameWithoutExtension(file), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                    if (fs.Length == 0)
                    {
                        Console.WriteLine($"Empty batch {index:X2}");
                        continue;
                    }

                    if (ignored[index] == false)
                    {
                        Console.WriteLine($"Running batch {index:X2}");
                        foreach (var test in JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, serializerOptions))
                        {
                            RunIndividualTest(test, results);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Ignoring batch {index:X2}");
                    }
                }
            }

            foreach (var result in results)
            {
                Console.WriteLine(result);
            }

            Assert.AreEqual(0, results.Count, $"Failed some tests");
        }

        private static void RunIndividualTest(JsonHarteTestStructure test, List<string> results)
        {
            var memory = new AccessCountingMemory();

            // set up initial memory state
            memory.Initialize(test.Initial.Ram);

            var cpu = new Cpu(
                CpuClass.WDC6502,
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

            // run test
            Console.WriteLine();
            cpu.Step(stopOnBreak: true, writeInstructions: false);

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
            // (var cyclesMatches, var cyclesDiffersAtAddr, var cyclesExpectedValue, var cyclesActualValue) =
            //     memory.ValidateCycles(test.BusAccesses);
            // if (cyclesMatches == false) results.Add($"{test.Name}: Expected cycles at {cyclesDiffersAtAddr} to be {cyclesExpectedValue} but is {cyclesActualValue}");
        }

        private static bool[] LoadIgnored()
        {
            var ignored = new bool[256];

            for (var o = 0; o < 256; o++)
            {
                ignored[o] = CpuInstructions.OpCode6502[(byte)o].AddressingMode == AddressingMode.Unknown;
            }

            return ignored;
        }
    }
}
