using System;
using System.Diagnostics;
using InnoWerks.Processors;

namespace InnoWerks.Simulators
{
    [DebuggerDisplay("{OpCode} {AddressingMode}")]
    public record OpCodeDefinition(
        OpCode OpCode,
        Action<Cpu, ushort, byte> Execute,
        Func<Cpu, bool> DecodeOperand,
        AddressingMode AddressingMode)
    {
        /// <summary>
        ///
        /// </summary>
        public OpCode OpCode { get; init; } = OpCode;

        /// <summary>
        ///
        /// </summary>
        public Func<Cpu, bool> DecodeOperand { get; init; } = DecodeOperand;

        /// <summary>
        ///
        /// </summary>
        public Action<Cpu, ushort, byte> Execute { get; init; } = Execute;

        /// <summary>
        ///
        /// </summary>
        public AddressingMode AddressingMode { get; init; } = AddressingMode;

        public override string ToString()
        {
            return OpCode.ToString();
        }
    }
}
