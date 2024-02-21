namespace InnoWerks.Simulators
{
#pragma warning disable CA1822
    public partial class Cpu
    {
        /// <summary>
        /// Accum - This form of addressing is represented with a
        /// one byte instruction, implying an operation on the accumulator.
        /// </summary>
        public ushort DecodeAccumulator()
        {
            OperandDisplay = "";

            return 0;
        }

        /// <summary>
        /// IMM - In immediate addressing, the second byte of the instruction
        /// contains the operand, with no further memory addressing required.
        /// </summary>
        public ushort DecodeImmediate()
        {
            OperandDisplay = $"#${read(ProgramCounter):X2}";

            return ProgramCounter++;
        }

        /// <summary>
        /// ABS - In absolute addressing, the second byte of the instruction
        /// specifies the eight low order bits of the effective address while
        /// the third byte specifies the eight high order bits. Thus the
        /// absolute addressing mode allows access to the entire 64k bytes
        /// of addressable memory.
        /// </summary>
        public ushort DecodeAbsolute()
        {
            OperandDisplay = $"${read((ushort)(ProgramCounter + 1)):X2}{read(ProgramCounter):X2}";

            ushort addrLo = read(ProgramCounter++);
            ushort addrHi = read(ProgramCounter++);

            return (ushort)((addrHi << 8) | addrLo);
        }

        /// <summary>
        /// ZP - The zero page instructions allow for shorter code and execution
        /// fetch times by fetching only the second byte of the instruction and
        /// assuming a zero high address byte. Careful of use the zero page can
        /// result in significant increase in code efficiency.
        /// </summary>
        public ushort DecodeZeroPage()
        {
            OperandDisplay = $"{{${read(ProgramCounter):X2}}}";

            return read(ProgramCounter++);
        }

        /// <summary>
        /// ZP,X (X indexing) - This form of address is used with the index
        /// register and is referred to as "Zero Page,X".
        /// The effective address is calculated by adding the second byte to the
        /// contents of the index register. Since this is a form of "Zero Page"
        /// addressing, the content of the second byte references a location
        /// in page zero. Additionally, due to the "Zero Page" addressing nature
        /// of this mode, no carry is added to the high order eight bits of
        /// memory and crossing page boundaries does not occur.
        /// </summary>
        public ushort DecodeZeroPageIndexedX()
        {
            OperandDisplay = $"{{${read(ProgramCounter):X2}}},X";

            return (ushort)((read(ProgramCounter++) + Registers.X) & 0x00ff);
        }

        /// <summary>
        /// ZP,Y (Y indexing) - This form of address is used with the index
        /// register and is referred to as "Zero Page,Y".
        /// The effective address is calculated by adding the second byte to the
        /// contents of the index register. Since this is a form of "Zero Page"
        /// addressing, the content of the second byte references a location
        /// in page zero. Additionally, due to the "Zero Page" addressing nature
        /// of this mode, no carry is added to the high order eight bits of
        /// memory and crossing page boundaries does not occur.
        /// </summary>
        public ushort DecodeZeroPageIndexedY()
        {
            OperandDisplay = $"{{${read(ProgramCounter):X2}}},Y";

            return (ushort)((read(ProgramCounter++) + Registers.Y) & 0xff);
        }

        /// <summary>
        /// ABS,X (X indexing) - This form of addressing is used in conjunction
        /// with X and Y index register and is referred to as "Absolute,X".
        /// The effective address is formed by adding the contents
        /// of X to the address contained in the second and third bytes of the
        /// instruction. This mode allows for the index register to contain the
        /// index or count value and the instruction to contain the base address.
        /// This type of indexing allows any location referencing and the index
        /// to modify fields, resulting in reducing coding and execution time.
        /// </summary>
        public ushort DecodeAbsoluteIndexedX()
        {
            OperandDisplay = $"${read((ushort)(ProgramCounter + 1)):X2}{read(ProgramCounter):X2},X";

            ushort addrL = read(ProgramCounter++);
            ushort addrH = read(ProgramCounter++);

            return (ushort)((addrH << 8) + addrL + Registers.X);
        }

        /// <summary>
        /// ABS,Y (Y indexing) - This form of addressing is used in conjunction
        /// with X and Y index register and is referred to as
        /// "Absolute,Y". The effective address is formed by adding the contents
        /// of Y to the address contained in the second and third bytes of the
        /// instruction. This mode allows for the index register to contain the
        /// index or count value and the instruction to contain the base address.
        /// This type of indexing allows any location referencing and the index
        /// to modify fields, resulting in reducing coding and execution time.
        /// </summary>
        public ushort DecodeAbsoluteIndexedY()
        {
            OperandDisplay = $"${read((ushort)(ProgramCounter + 1)):X2}{read(ProgramCounter):X2},Y";

            ushort addrL = read(ProgramCounter++);
            ushort addrH = read(ProgramCounter++);

            return (ushort)((addrH << 8) + addrL + Registers.Y);
        }

        /// <summary>
        /// (ABS,X) - The contents of the second and third instruction byte are
        /// added to the X register. The sixteen-bit result is a memory address
        /// containing the effective address (JMP (ABS,X) only).
        /// </summary>
        public ushort DecodeIndexedAbsolute()
        {
            OperandDisplay = $"(${read((ushort)(ProgramCounter + 1)):X2}{read(ProgramCounter):X2},X)";

            ushort addrL = read(ProgramCounter++);
            ushort addrH = read(ProgramCounter++);

            return read((ushort)((addrH << 8) + addrL + Registers.X));
        }

        /// <summary>
        /// Implied - In the implied addressing mode, the address containing
        /// the operand is implicitly stated in the operation code of the instruction.
        /// </summary>
        public ushort DecodeImplied()
        {
            OperandDisplay = "";

            return 0;
        }

        /// <summary>
        /// <para>Relative - Relative addressing is used only with branch instructions
        /// and establishes a destination for the conditional branch.</para>
        ///
        /// <para>The second byte of the instruction becomes the operand which is an
        /// "Offset" added to the contents of the lower eight bits of the program
        /// counter when the counter is set at the next instruction. The range
        /// of the offset is -128 to +127 bytes from the next instruction.</para>
        /// </summary>
        public ushort DecodeRelative()
        {
            OperandDisplay = $"${read(ProgramCounter):X2}";

            ushort offset = read(ProgramCounter++);

            if ((offset & 0x80) != 0) { offset |= 0xff00; }

            return (ushort)(ProgramCounter + offset);
        }

        /// <summary>
        /// (IND,X) - In indexed indirect addressing (referred to ad (Indirect,X)),
        /// the second byte of the instruction is added to the contents of the X
        /// register, discarding the carry. The result of this addition points to a
        /// memory location on page zero whose contents are the low order eight bits
        /// of the effective address. The next memory location in page zero contains
        /// the high order eight bits of the effective address. Both memory locations
        /// specifying the high and low order bytes of the effective address
        /// must be in page zero.
        /// </summary>
        public ushort DecodeIndexedIndirect()
        {
            OperandDisplay = $"({{${read(ProgramCounter):X2}}},X)";

            ushort zeroL = (ushort)((read(ProgramCounter++) + Registers.X) & 0xff);
            ushort zeroH = (ushort)((zeroL + 1) & 0xff);

            return (ushort)((read(zeroH) << 8) | read(zeroL));
        }

        /// <summary>
        /// (IND),Y - In indirect indexed addressing (referred to ad (Indirect),Y), the
        /// second byte of the instruction points to a memory location in page zero. The
        /// contents of this memory location are added to the contents of the Y index
        /// register, the result being the low order eight bits of the effective address.
        /// The carry from this addition is added to the contents of the next page
        /// zero memory loation, the result being the high order eight bits
        /// of the effective address.
        /// </summary>
        public ushort DecodeIndirectIndexed()
        {
            OperandDisplay = $"({{${read(ProgramCounter):X2}}}),Y";

            ushort zeroL = read(ProgramCounter++);
            ushort zeroH = (ushort)((zeroL + 1) & 0xff);

            return (ushort)(((read(zeroH) << 8) | read(zeroL)) + Registers.Y);
        }

        /// <summary>
        /// (ABS) - The second byte of the instruction contains the low order eight
        /// bits of a memory location. The high order eight bits of that memory
        /// location are contained in the third byte of the instruction. The contents
        /// of the fully specified memory location are the low order byte of the
        /// effective address. The next memory location contains the high order
        /// byte of the effective address which is loaded into the sixteen bits
        /// of the program counter (JMP (ABS) only).
        /// </summary>
        public ushort DecodeAbsoluteIndirect()
        {
            OperandDisplay = $"${read((ushort)(ProgramCounter + 1)):X2}{read(ProgramCounter):X2}";

            ushort addrL = read(ProgramCounter++);
            ushort addrH = read(ProgramCounter++);

            ushort abs = (ushort)((addrH << 8) | addrL);

            ushort effL = read(abs);

#if CMOS_INDIRECT_JMP_FIX
            ushort effH = read((ushort)(abs + 1));
#else
            ushort effH = read((ushort)((abs & 0xFF00) + ((abs + 1) & 0x00ff)));
#endif

            return (ushort)((0x100 * effH) + effL);
        }

        /// <summary>
        /// (IND) - The second byte of the instruction contains a zero page address
        /// serving as the indirect pointer.
        /// </summary>
        public ushort DecodeIndirect()
        {
            OperandDisplay = "";

            return 0;
        }

        public ushort DecodeUndefined()
        {
            illegalOpCode = true;

            OperandDisplay = "";

            return 0;
        }
    }
}
