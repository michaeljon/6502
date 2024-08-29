using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                new HarteCycleConverter()
            }
        };

        [TestMethod]
        public void RunAll6502Tests()
        {
            var files = Directory
                .GetFiles("/Users/michaeljon/src/6502/working/65x02/6502/v1", "*.json")
                .OrderBy(f => f)
                .Skip(3)
                .Take(1);

            foreach (var file in files)
            {
                using (var fs = File.OpenRead(file))
                {
                    var batch = Path.GetFileNameWithoutExtension(file);

                    foreach (var test in JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, serializerOptions))
                    {
                        RunIndividualTest(batch, test);
                    }
                }
            }
        }

        private static void RunIndividualTest(string batch, JsonHarteTestStructure test)
        {
            Console.WriteLine($"Running batch {batch} test {test.Name}");

            var memory = new AccessCountingMemory();

            // set up initial memory state
            memory.Initialize(test.Initial.Ram);

            var cpu = new Cpu(
                Processors.CpuClass.WDC6502,
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
            Assert.AreEqual(test.Final.ProgramCounter, cpu.Registers.ProgramCounter, "ProgramCounter");
            Assert.AreEqual(test.Final.S, cpu.Registers.StackPointer, "StackPointer");
            Assert.AreEqual(test.Final.A, cpu.Registers.A, "A");
            Assert.AreEqual(test.Final.X, cpu.Registers.X, "X");
            Assert.AreEqual(test.Final.Y, cpu.Registers.Y, "Y");
            Assert.AreEqual(test.Final.P, cpu.Registers.ProcessorStatus, "ProcessorStatus");

            // verify memory
            (var ramMatches, var ramDiffersAtAddr, byte ramExpectedValue, byte ramActualValue) =
                memory.ValidateMemory(test.Final.Ram);
            Assert.IsTrue(ramMatches, $"Expected memory at {ramDiffersAtAddr} to be {ramExpectedValue} but is {ramActualValue}");

            // verify timing
            (var cyclesMatches, var cyclesDiffersAtAddr, var cyclesExpectedValue, var cyclesActualValue) =
                memory.ValidateCycles(test.BusAccesses);
            Assert.IsTrue(ramMatches, $"Expected memory at {cyclesDiffersAtAddr} to be {cyclesExpectedValue} but is {cyclesActualValue}");
        }
    }
}
