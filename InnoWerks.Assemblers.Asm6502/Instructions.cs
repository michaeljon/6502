using System.Collections.Generic;

using static InnoWerks.Assemblers.AddressingMode;
using static InnoWerks.Assemblers.OpCode;

namespace InnoWerks.Assemblers
{
    internal static class InstructionInformation
    {
        public static readonly ISet<AddressingMode> SingleByteAddressModes = new HashSet<AddressingMode>
        {
            Immediate,
            Relative,
            ZeroPage,
            ZeroPageXIndexed,
            ZeroPageYIndexed,
            ZeroPageIndirect,
            XIndexedIndirect,
            IndirectYIndexed
        };

        public static readonly ISet<AddressingMode> TwoByteAddressModes = new HashSet<AddressingMode>
        {
            Absolute,
            AbsoluteXIndexed,
            AbsoluteYIndexed,
            AbsoluteIndexedIndirect
        };

        public static readonly ISet<OpCode> BranchingOperations = new HashSet<OpCode>
        {
            BBR0,
            BBR1,
            BBR2,
            BBR3,
            BBR4,
            BBR5,
            BBR6,
            BBR7,
            BBS0,
            BBS1,
            BBS2,
            BBS3,
            BBS4,
            BBS5,
            BBS6,
            BBS7,
            BCC,
            BCS,
            BEQ,
            BMI,
            BPL,
            BNE,
            BRA,
            BVC,
            BVS,
        };

        public static readonly IDictionary<(OpCode opCode, AddressingMode addressingMode), byte> Instructions =
            new Dictionary<(OpCode opCode, AddressingMode addressingMode), byte>
            {
                {(BRK, Implied), 0x00 },
                {(ORA, XIndexedIndirect), 0x01 },
                // unassigned
                // unassigned
                {(TSB, ZeroPage), 0x04 },
                {(ORA, ZeroPage), 0x05 },
                {(ASL, ZeroPage), 0x06 },
                {(RMB0, ZeroPage), 0x07 },
                {(PHP, Implied), 0x08 },
                {(ORA, Immediate), 0x09 },
                {(ASL, Accumulator), 0x0a },
                // unassigned
                {(TSB, Absolute), 0x0c },
                {(ORA, Absolute), 0x0d },
                {(ASL, Absolute), 0x0e },
                {(BBR0, Relative), 0x0f },

                {(BPL, Relative), 0x10 },
                {(ORA, IndirectYIndexed), 0x11 },
                {(ORA, ZeroPageIndirect), 0x12 },
                // unassigned
                {(TRB, ZeroPage), 0x14 },
                {(ORA, ZeroPageXIndexed), 0x15 },
                {(ASL, ZeroPageXIndexed), 0x16 },
                {(RMB1, ZeroPage), 0x17 },
                {(CLC, Implied), 0x18 },
                {(ORA, AbsoluteYIndexed), 0x19 },
                {(INA, Accumulator), 0x1a },
                // unassigned
                {(TRB, Absolute), 0x1c },
                {(ORA, AbsoluteXIndexed), 0x1d },
                {(ASL, AbsoluteXIndexed), 0x1e },
                {(BBR1, Relative), 0x1f },

                {(JSR, Absolute), 0x20 },
                {(AND, XIndexedIndirect), 0x21 },
                // unassigned
                // unassigned
                {(BIT, ZeroPage), 0x24 },
                {(AND, ZeroPage), 0x25 },
                {(ROL, ZeroPage), 0x26 },
                {(RMB2, ZeroPage), 0x27 },
                {(PLP, Implied), 0x28 },
                {(AND, Immediate), 0x29 },
                {(ROL, Accumulator), 0x2a },
                // unassigned
                {(BIT, Absolute), 0x2c },
                {(AND, Absolute), 0x2d },
                {(ROL, Absolute), 0x2e },
                {(BBR2, Relative), 0x2f },

                {(BMI, Relative), 0x30 },
                {(AND, IndirectYIndexed), 0x31 },
                {(AND, ZeroPageIndirect), 0x32 },
                // unassigned
                {(BIT, ZeroPageXIndexed), 0x34 },
                {(AND, ZeroPageXIndexed), 0x35 },
                {(ROL, ZeroPageXIndexed), 0x36 },
                {(RMB3, ZeroPage), 0x37 },
                {(SEC, Implied), 0x38 },
                {(AND, AbsoluteYIndexed), 0x39 },
                {(DEA, Accumulator), 0x3a },
                // unassigned
                {(BIT, AbsoluteXIndexed), 0x3c },
                {(AND, AbsoluteXIndexed), 0x3d },
                {(ROL, AbsoluteXIndexed), 0x3e },
                {(BBR3, Relative), 0x3f },

                {(RTI, Implied), 0x40 },
                {(EOR, XIndexedIndirect), 0x41 },
                // unassigned
                // unassigned
                // unassigned
                {(EOR, ZeroPage), 0x45 },
                {(LSR, ZeroPage), 0x46 },
                {(RMB4, ZeroPage), 0x47 },
                {(PHA, Implied), 0x48 },
                {(EOR, Immediate), 0x49 },
                {(LSR, Accumulator), 0x4a },
                // unassigned
                {(JMP, Absolute), 0x4c },
                {(EOR, Absolute), 0x4d },
                {(LSR, Absolute), 0x4e },
                {(BBR4, Relative), 0x4f },

                {(BVC, Relative), 0x50 },
                {(EOR, IndirectYIndexed), 0x51 },
                {(EOR, ZeroPageIndirect), 0x52 },
                // unassigned
                // unassigned
                {(EOR, ZeroPageXIndexed), 0x55 },
                {(LSR, ZeroPageXIndexed), 0x56 },
                {(RMB5, ZeroPage), 0x57 },
                {(CLI, Implied), 0x58 },
                {(EOR, AbsoluteYIndexed), 0x59 },
                {(PHY, Accumulator), 0x5a },
                // unassigned
                // unassigned
                {(EOR, AbsoluteXIndexed), 0x5d },
                {(LSR, AbsoluteXIndexed), 0x5e },
                {(BBR5, Relative), 0x5f },

                {(RTS, Implied), 0x60 },
                {(ADC, XIndexedIndirect), 0x61 },
                // unassigned
                // unassigned
                {(STZ, ZeroPage), 0x64 },
                {(ADC, ZeroPage), 0x65 },
                {(ROR, ZeroPage), 0x66 },
                {(RMB6, ZeroPage), 0x67 },
                {(PLA, Implied), 0x68 },
                {(ADC, Immediate), 0x69 },
                {(ROR, Accumulator), 0x6a },
                // unassigned
                {(JMP, AbsoluteIndirect), 0x6c },
                {(ADC, Absolute), 0x6d },
                {(ROR, Absolute), 0x6e },
                {(BBR6, Relative), 0x6f },

                {(BVS, Relative), 0x70 },
                {(ADC, IndirectYIndexed), 0x71 },
                {(ADC, ZeroPageIndirect), 0x72 },
                // unassigned
                {(STZ, ZeroPageXIndexed), 0x74 },
                {(ADC, ZeroPageXIndexed), 0x75 },
                {(ROR, ZeroPageXIndexed), 0x76 },
                {(RMB7, ZeroPage), 0x77 },
                {(SEI, Implied), 0x78 },
                {(ADC, AbsoluteYIndexed), 0x79 },
                {(PLY, Implied), 0x7a },
                // unassigned
                {(JMP, AbsoluteIndexedIndirect), 0x7c },
                {(ADC, AbsoluteXIndexed), 0x7d },
                {(ROR, AbsoluteXIndexed), 0x7e },
                {(BBR7, Relative), 0x7f },

                {(BRA, Relative), 0x80 },
                {(STA, XIndexedIndirect), 0x81 },
                // unassigned
                // unassigned
                {(STY, ZeroPage), 0x84 },
                {(STA, ZeroPage), 0x85 },
                {(STX, ZeroPage), 0x86 },
                {(SMB0, ZeroPage), 0x87 },
                {(DEY, Implied), 0x88 },
                {(BIT, Immediate), 0x89 },
                {(TXA, Implied), 0x8a },
                // unassigned
                {(STY, Absolute), 0x8c },
                {(STA, Absolute), 0x8d },
                {(STX, Absolute), 0x8e },
                {(BBS0, Relative), 0x8f },

                {(BCC, Relative), 0x90 },
                {(STA, IndirectYIndexed), 0x91 },
                {(STA, ZeroPageIndirect), 0x92 },
                // unassigned
                {(STY, ZeroPageXIndexed), 0x94 },
                {(STA, ZeroPageXIndexed), 0x95 },
                {(STX, ZeroPageYIndexed), 0x96 },
                {(SMB1, ZeroPage), 0x97 },
                {(TYA, Implied), 0x98 },
                {(STA, AbsoluteYIndexed), 0x99 },
                {(TXS, Implied), 0x9a },
                // unassigned
                {(STZ, Absolute), 0x9c },
                {(STA, AbsoluteXIndexed), 0x9d },
                {(STZ, AbsoluteXIndexed), 0x9a },
                {(BBS1, Relative), 0x9f },

                {(LDY, Immediate), 0xa0 },
                {(LDA, XIndexedIndirect), 0xa1 },
                {(LDX, Immediate), 0xa2 },
                // unassigned
                {(LDY, ZeroPage), 0xa4 },
                {(LDA, ZeroPage), 0xa5 },
                {(LDX, ZeroPage), 0xa6 },
                {(SMB2, ZeroPage), 0xa7 },
                {(TAY, Implied), 0xa8 },
                {(LDA, Immediate), 0xa9 },
                {(TAX, Implied), 0xaa },
                // unassigned
                {(LDY, Absolute), 0xac },
                {(LDA, Absolute), 0xad },
                {(LDX, Absolute), 0xae },
                {(BBS2, Relative), 0xaf },

                {(BCS, Relative), 0xb0 },
                {(LDA, IndirectYIndexed), 0xb1 },
                {(LDA, ZeroPageIndirect), 0xb2 },
                // unassigned
                {(LDY, ZeroPageXIndexed), 0xb4 },
                {(LDA, ZeroPageXIndexed), 0xb5 },
                {(LDX, ZeroPageYIndexed), 0xb6 },
                {(SMB3, ZeroPage), 0xb7 },
                {(CLV, Implied), 0xb8 },
                {(LDA, AbsoluteYIndexed), 0xb9 },
                {(TSX, Implied), 0xba },
                // unassigned
                {(LDY, AbsoluteXIndexed), 0xbc },
                {(LDA, AbsoluteXIndexed), 0xbd },
                {(LDX, AbsoluteYIndexed), 0xbe },
                {(BBS3, Relative), 0xbf },

                {(CPY, Immediate), 0xc0 },
                {(CMP, XIndexedIndirect), 0xc1 },
                // unassigned
                // unassigned
                {(CPY, ZeroPage), 0xc4 },
                {(CMP, ZeroPage), 0xc5 },
                {(DEC, ZeroPage), 0xc6 },
                {(SMB4, ZeroPage), 0xc7 },
                {(INY, Implied), 0xc8 },
                {(CMP, Immediate), 0xc9 },
                {(DEX, Implied), 0xca },
                {(WAI, Implied), 0xcb },
                {(CPY, Absolute), 0xcc },
                {(CMP, Absolute), 0xcd },
                {(DEC, Absolute), 0xce },
                {(BBS4, Relative), 0xcf },

                {(BNE, Relative), 0xd0 },
                {(CMP, IndirectYIndexed), 0xd1 },
                {(CMP, ZeroPageIndirect), 0xd2 },
                // unassigned
                // unassigned
                {(CMP, ZeroPageXIndexed), 0xd5 },
                {(DEC, ZeroPageXIndexed), 0xd6 },
                {(SMB5, ZeroPage), 0xd7 },
                {(CLD, Implied), 0xd8 },
                {(CMP, AbsoluteYIndexed), 0xd9 },
                {(PHX, Implied), 0xda },
                {(STP, Implied), 0xdb },
                // unassigned
                {(CMP, AbsoluteXIndexed), 0xdd },
                {(DEC, AbsoluteXIndexed), 0xde },
                {(BBS5, Relative), 0xdf },

                {(CPX, Immediate), 0xe0 },
                {(SBC, XIndexedIndirect), 0xe1 },
                // unassigned
                // unassigned
                {(CPX, ZeroPage), 0xe4 },
                {(SBC, ZeroPage), 0xe5 },
                {(INC, ZeroPage), 0xe6 },
                {(SMB6, ZeroPage), 0xe7 },
                {(INX, Implied), 0xe8 },
                {(SBC, Immediate), 0xe9 },
                {(NOP, Implied), 0xea },
                // unassigned
                {(CPX, Absolute), 0xec },
                {(SBC, Absolute), 0xed },
                {(INC, Absolute), 0xee },
                {(BBS6, Relative), 0xef },

                {(BEQ, Relative), 0xf0 },
                {(SBC, IndirectYIndexed), 0xf1 },
                {(SBC, ZeroPageIndirect), 0xf2 },
                // unassigned
                // unassigned
                {(SBC, ZeroPageXIndexed), 0xf5 },
                {(INC, ZeroPageXIndexed), 0xf6 },
                {(SMB7, ZeroPage), 0xf7 },
                {(SED, Implied), 0xf8 },
                {(SBC, AbsoluteYIndexed), 0xf9 },
                {(PLX, Implied), 0xfa },
                // unassigned
                // unassigned
                {(SBC, AbsoluteXIndexed), 0xfd },
                {(INC, AbsoluteXIndexed), 0xfe },
                {(BBS7, Relative), 0xff },
        };
    }
}
