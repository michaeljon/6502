namespace InnoWerks.Simulators
{
    public enum AddressingMode
    {
        Unknown,

        // 0 byte instruction, no argument provided
        Implied,

        // 0 byte instruction, no argument (for "A" operands) or an explicit A
        Accumulator,

        // 1 byte #-prefixed value of the form #$12
        Immediate,

        // 2 byte address of the form $ABCD
        Absolute,

        // 1 byte address
        ZeroPage,

        // 0 byte instruction, stack
        Stack,

        // 2 byte address like $ABCD,X
        AbsoluteXIndexed,

        // 2 byte address like $ABCD,Y
        AbsoluteYIndexed,

        // 1 byte address like $ABCD,X
        ZeroPageXIndexed,

        // 1 byte address like $ABCD,Y
        ZeroPageYIndexed,

        // typically a label, must ref a loc < [-127, + 128] bytes away
        Relative,

        // 1 byte address of form ($AB,X) (also called indirect X)
        ZeroPageIndirect,

        // 2 byte address of the form ($ABCD,X) only with JMP
        AbsoluteIndexedIndirect,

        // 1 byte address of form ($AB,X)
        XIndexedIndirect,

        // 1 byte address of the form $(AB),Y
        IndirectYIndexed,

        // 2 byte address of the form ($ABCD) only with JMP
        AbsoluteIndirect
    }
}
