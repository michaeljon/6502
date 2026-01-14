using System;
using System.Diagnostics;
using InnoWerks.Processors;

namespace InnoWerks.Simulators
{
    [DebuggerDisplay("{OpCode} {AddressingMode}")]
    public record OpCodeDefinition(
        OpCode OpCode,
        Action<MosTechnologiesCpu, ushort, byte> Execute,
        Func<MosTechnologiesCpu, bool> DecodeOperand,
        AddressingMode AddressingMode)
    {
        /// <summary>
        ///
        /// </summary>
        public OpCode OpCode { get; init; } = OpCode;

        /// <summary>
        ///
        /// </summary>
        public Func<MosTechnologiesCpu, bool> DecodeOperand { get; init; } = DecodeOperand;

        /// <summary>
        ///
        /// </summary>
        public Action<MosTechnologiesCpu, ushort, byte> Execute { get; init; } = Execute;

        /// <summary>
        ///
        /// </summary>
        public AddressingMode AddressingMode { get; init; } = AddressingMode;

        public override string ToString()
        {
            return $"{OpCode} {AddressingMode}";
        }
    }
}
