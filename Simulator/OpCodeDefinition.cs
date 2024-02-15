using System;

//
// See: https://github.com/eteran/pretendo/blob/master/doc/cpu/6502.txt
//
namespace InnoWerks.Simulators
{
    public record OpCodeDefinition(
        string Nmemonic,
        Action<Cpu, ushort> Execute,
        Func<Cpu, ushort> DecodeOperand,
        int Cycles)
    {
        public string Nmemonic { get; init; } = Nmemonic;

        public Func<Cpu, ushort> DecodeOperand { get; init; } = DecodeOperand;

        public Action<Cpu, ushort> Execute { get; init; } = Execute;

        public int Cycles { get; init; } = Cycles;
    }
}
