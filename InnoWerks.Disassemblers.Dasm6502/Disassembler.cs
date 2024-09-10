using System.Collections.Generic;
using System.IO;

#pragma warning disable CA1002

namespace InnoWerks.Disassemblers
{
    public class Disassembler
    {
        private readonly string filename;

        private readonly ushort origin;

        private static readonly Instructions instructionDecoding = new();

        public Disassembler(string filename, ushort origin = 0x0000)
        {
            this.filename = filename;
            this.origin = origin;
        }

        public void Disassemble()
        {
            var bytes = File.ReadAllBytes(filename);

            ushort pc = 0;

            while (pc < bytes.Length)
            {
                var op = bytes[pc];
                var decoding = instructionDecoding[op];

                switch (decoding.AddressingMode)
                {
                    case Processors.AddressingMode.Unknown:
                        Disassembly.Add($"    ???");
                        pc++;
                        break;

                    case Processors.AddressingMode.Implicit:
                        Disassembly.Add($"    {decoding.Instruction}");
                        pc++;
                        break;

                    case Processors.AddressingMode.Accumulator:
                        Disassembly.Add($"    {decoding.Instruction} A");
                        pc++;
                        break;

                    case Processors.AddressingMode.Immediate:
                        Disassembly.Add($"    {decoding.Instruction} #${bytes[pc + 1]:X2}");
                        pc += 2;
                        break;

                    case Processors.AddressingMode.Absolute:
                        Disassembly.Add($"    {decoding.Instruction} ${bytes[pc + 2]:X2}{bytes[pc + 1]:X2}");
                        pc += 3;
                        break;

                    case Processors.AddressingMode.ZeroPage:
                        Disassembly.Add($"    {decoding.Instruction} ${bytes[pc + 1]:X2}");
                        pc += 2;
                        break;

                    case Processors.AddressingMode.Stack:
                        Disassembly.Add($"    {decoding.Instruction}");
                        pc++;
                        break;

                    case Processors.AddressingMode.AbsoluteXIndexed:
                        Disassembly.Add($"    {decoding.Instruction} ${bytes[pc + 2]:X2}{bytes[pc + 1]:X2},X");
                        pc += 3;
                        break;

                    case Processors.AddressingMode.AbsoluteYIndexed:
                        Disassembly.Add($"    {decoding.Instruction} ${bytes[pc + 2]:X2}{bytes[pc + 1]:X2},Y");
                        pc += 3;
                        break;

                    case Processors.AddressingMode.ZeroPageXIndexed:
                        Disassembly.Add($"    {decoding.Instruction} (${bytes[pc + 1]:X2}),X");
                        pc += 2;
                        break;

                    case Processors.AddressingMode.ZeroPageYIndexed:
                        Disassembly.Add($"    {decoding.Instruction} (${bytes[pc + 1]:X2}),Y");
                        pc += 2;
                        break;
                    case Processors.AddressingMode.Relative:
                        Disassembly.Add($"    {decoding.Instruction} ${bytes[pc + 1]:X2}");
                        pc += 2;
                        break;

                    case Processors.AddressingMode.ZeroPageIndirect:
                        Disassembly.Add($"    {decoding.Instruction} (${bytes[pc + 1]:X2})");
                        pc += 2;
                        break;

                    case Processors.AddressingMode.AbsoluteIndexedIndirect:
                        Disassembly.Add($"    {decoding.Instruction} (${bytes[pc + 2]:X2}{bytes[pc + 1]:X2}),X");
                        pc += 3;
                        break;

                    case Processors.AddressingMode.XIndexedIndirect:
                        Disassembly.Add($"    {decoding.Instruction} ({bytes[pc + 1]:X2},X)");
                        pc += 2;
                        break;

                    case Processors.AddressingMode.IndirectYIndexed:
                        Disassembly.Add($"    {decoding.Instruction} ({bytes[pc + 1]:X2}),Y");
                        pc += 2;
                        break;

                    case Processors.AddressingMode.AbsoluteIndirect:
                        Disassembly.Add($"    {decoding.Instruction} (${bytes[pc + 2]:X2}{bytes[pc + 1]:X2})");
                        pc += 3;
                        break;
                }

            }
        }

        public List<string> Disassembly { get; } = [];
    }
}
