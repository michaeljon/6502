using System;
using System.Diagnostics;
using InnoWerks.Processors;

namespace InnoWerks.Simulators
{
    [DebuggerDisplay("{OpCode} {AddressingMode}")]
    public record OpCodeDefinition(
        OpCode OpCode,
        Action<Cpu, ushort, byte> Execute,
        Func<Cpu, ushort> DecodeOperand,
        AddressingMode AddressingMode,
        long PageCrossPenalty = 0)
    {
        /// <summary>
        ///
        /// </summary>
        public OpCode OpCode { get; init; } = OpCode;

        /// <summary>
        ///
        /// </summary>
        public Func<Cpu, ushort> DecodeOperand { get; init; } = DecodeOperand;

        /// <summary>
        ///
        /// </summary>
        public Action<Cpu, ushort, byte> Execute { get; init; } = Execute;

        /// <summary>
        ///
        /// </summary>
        public AddressingMode AddressingMode { get; init; } = AddressingMode;

        /// <summary>
        ///
        /// </summary>
        public long PageCrossPenalty { get; init; } = PageCrossPenalty;

        public override string ToString()
        {
            return OpCode.ToString();
        }
    }
}
