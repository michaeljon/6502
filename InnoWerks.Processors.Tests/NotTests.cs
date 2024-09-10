using System;
using InnoWerks.Processors;

namespace InnoWerks.Disassemblers.Tests
{
    [TestClass]
    public class NotTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GenerateDasmTable()
        {
            (OpCode opCode, string instruction, AddressingMode addressingMode, CpuClass cpuClass)[] lookup =
                new (OpCode, string, AddressingMode, CpuClass)[256];

            foreach (var (k, v) in InstructionInformation.Instructions)
            {
                var instruction = k.opCode switch
                {
                    OpCode.ASL_A => "ASL A",
                    OpCode.DEA => "DEC A",
                    OpCode.INA => "INC A",
                    OpCode.ROL_A => "ROL A",
                    OpCode.ROR_A => "ROR A",
                    OpCode.LSR_A => "LSR A",

                    OpCode.Unknown => "???",

                    _ => k.opCode.ToString()
                };

                lookup[v.code] = (k.opCode, instruction, k.addressingMode, v.minCpuClass);
            }

            Console.WriteLine("[");
            for (var i = 0; i < 256; i++)
            {
                Console.WriteLine($"/* ${i:X2} */ new(OpCode.{lookup[i].opCode}, \"{lookup[i].instruction}\", AddressingMode.{lookup[i].addressingMode}, CpuClass.{lookup[i].cpuClass}), ");
            }
            Console.WriteLine("];");
        }
    }
}
