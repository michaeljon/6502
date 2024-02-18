using System;

namespace InnoWerks.Simulators
{
    public record OpCodeDefinition(
        string Nmemonic,
        Action<Cpu, ushort> Execute,
        Func<Cpu, ushort> DecodeOperand,
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
        public Action<Cpu, ushort> Execute { get; init; } = Execute;

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
