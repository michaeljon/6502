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
    public class Harte65C02 : TestBase
    {
        private static readonly JsonSerializerOptions serializerOptions = new()
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

        [TestMethod]
        public void RunAll65C02Tests()
        {
            List<string> results = [];

            var files = Directory
                .GetFiles("/Users/michaeljon/src/6502/working/65x02/wdc65c02/v1", "*.json")
                .OrderBy(f => f);

            var ignored = LoadIgnored();

            foreach (var file in files)
            {
                using (var fs = File.OpenRead(file))
                {
                    var index = byte.Parse(Path.GetFileNameWithoutExtension(file), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                    if (fs.Length == 0)
                    {
                        continue;
                    }

                    if (ignored[index] == false)
                    {
                        foreach (var test in JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, serializerOptions).Take(10))
                        {
                            RunIndividualTest(test, results);
                        }
                    }
                }
            }

            foreach (var result in results)
            {
                Console.WriteLine(result);
            }

            Assert.AreEqual(0, results.Count, $"Failed some tests");
        }

        [DataTestMethod]
        [DataRow("40")]
        public void RunNamed65C02Batch(string batch)
        {
            if (string.IsNullOrEmpty(batch))
            {
                Assert.Inconclusive("No batch name provided to RunNamed65C02Batch");
                return;
            }

            List<string> results = [];

            var file = $"/Users/michaeljon/src/6502/working/65x02/wdc65c02/v1/{batch}.json";

            Console.WriteLine($"Running batch {batch}");

            using (var fs = File.OpenRead(file))
            {
                var tests = JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, serializerOptions);
                foreach (var test in tests)
                {
                    RunIndividualTest(test, results);
                }

                // foreach (var result in results)
                // {
                //     Console.WriteLine(result);
                // }

                Assert.AreEqual(0, results.Count, $"Failed some {results.Count} tests out of an expected {tests.Count}");
            }
        }

        [DataTestMethod]
        [DataRow("7c 1b e8")]
        public void RunNamed65C02Test(string testName)
        {
            if (string.IsNullOrEmpty(testName))
            {
                Assert.Inconclusive("No test name provided to RunNamed65C02Test");
                return;
            }

            List<string> results = [];

            var batch = testName.Split(' ')[0];
            var file = $"/Users/michaeljon/src/6502/working/65x02/wdc65c02/v1/{batch}.json";

            Console.WriteLine($"Running test {testName}");
            Console.WriteLine($"OpCode: ${batch} {CpuInstructions.OpCode65C02[byte.Parse(batch, NumberStyles.HexNumber, CultureInfo.InvariantCulture)].OpCode}");
            Console.WriteLine($"AddressingMode: {CpuInstructions.OpCode65C02[byte.Parse(batch, NumberStyles.HexNumber, CultureInfo.InvariantCulture)].AddressingMode}");

            using (var fs = File.OpenRead(file))
            {
                var tests = JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, serializerOptions);
                var test = tests.FirstOrDefault(t => t.Name == testName);

                if (test == null)
                {
                    Assert.Inconclusive($"Unable to locate test {testName}");
                    return;
                }

                var json = JsonSerializer.Serialize(test.Clone(), serializerOptions);
                File.WriteAllText("foo.json", json);

                RunIndividualTest(test, results);
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
                CpuClass.WDC65C02,
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
                ignored[o] = CpuInstructions.OpCode65C02[(byte)o].AddressingMode == AddressingMode.Unknown;
            }

            return ignored;
        }
    }
}
