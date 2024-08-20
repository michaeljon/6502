namespace Asm6502
{
    public enum AddressingMode
    {
        Unknown,

        Implicit,

        Accumulator,

        Immediate,

        Relative,

        Absolute,

        AbsoluteXIndexed,

        AbsoluteYIndexed,

        ZeroPage,

        ZeroPageIndirect,

        ZeroPageXIndexed,

        ZeroPageYIndexed,

        Indirect,

        XIndexedIndirect,

        IndirectYIndexed
    }
}