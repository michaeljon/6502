using System;
using InnoWerks.Processors.Common;

namespace InnoWerks.Simulators
{
    public record OpCodeDefinition(
        string Nmemonic,
        Action<Cpu, ushort, byte> Execute,
        Func<Cpu, ushort> DecodeOperand,
        AddressingMode AddressingMode,
        long PageCrossPenalty = 0)
    {
        /// <summary>
        ///
        /// </summary>
        public string Nmemonic { get; init; } = Nmemonic;

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
            return Nmemonic;
        }
    }
}
