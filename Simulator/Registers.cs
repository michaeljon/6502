namespace InnoWerks.Simulators
{
    public class Registers
    {
        public void Reset()
        {
            A = 0;
            Y = 0;
            X = 0;
            StackPointer = 0xff;
            ProcessorStatus = 0x00;
        }

        /// <summary>
        /// The accumulator is the main register for arithmetic and logic
        /// operations. Unlike the index registers X and Y, it has a direct
        /// connection to the Arithmetic and Logic Unit (ALU). This is why
        /// many operations are only available for the accumulator, not the
        /// index registers.
        /// </summary>
        public byte A { get; set; }

        /// <summary>
        /// This is the main register for addressing data with indices. It has
        /// a special addressing mode, indexed indirect, which lets you to
        /// have a vector table on the zero page.
        /// </summary>
        public byte X { get; set; }

        /// <summary>
        /// The Y register has the least operations available. On the other
        /// hand, only it has the indirect indexed addressing mode that
        /// enables access to any memory place without having to use
        /// self-modifying code.
        /// </summary>
        public byte Y { get; set; }

        /// <summary>
        /// <para>The NMOS 65xx processors have 256 bytes of stack memory, ranging
        ///  from $0100 to $01FF. The S register is a 8-bit offset to the stack
        ///  page. In other words, whenever anything is being pushed on the
        ///  stack, it will be stored to the address $0100+S.</para>
        ///
        ///  <para>The Stack pointer can be read and written by transfering its value
        ///  to or from the index register X (see below) with the TSX and TXS
        ///  instructions.</para>
        /// </summary>
        public byte StackPointer { get; set; }

        /// <summary>
        /// <para>This 8-bit register stores the state of the processor. The bits in
        /// this register are called flags. Most of the flags have something
        /// to do with arithmetic operations.</para>
        ///
        /// <para>The P register can be read by pushing it on the stack (with PHP or
        /// by causing an interrupt). If you only need to read one flag, you
        /// can use the branch instructions. Setting the flags is possible by
        /// pulling the P register from stack or by using the flag set or
        /// clear instructions.</para>
        /// </summary>
        public byte ProcessorStatus
        {
            get
            {
                byte ps = 0;

                ps |= (byte)(Negative ? ProcessorStatusBit.Negative : 0);
                ps |= (byte)(Overflow ? ProcessorStatusBit.Overflow : 0);
                ps |= (byte)(Break ? ProcessorStatusBit.BreakCommand : 0);
                ps |= (byte)(Decimal ? ProcessorStatusBit.DecimalMode : 0);
                ps |= (byte)(Interrupt ? ProcessorStatusBit.InterruptDisable : 0);
                ps |= (byte)(Zero ? ProcessorStatusBit.Zero : 0);
                ps |= (byte)(Carry ? ProcessorStatusBit.Carry : 0);

                return ps;
            }

            set
            {
                Negative = (byte)(value & (byte)ProcessorStatusBit.Negative) != 0;
                Overflow = (byte)(value & (byte)ProcessorStatusBit.Overflow) != 0;
                Break = (byte)(value & (byte)ProcessorStatusBit.BreakCommand) != 0;
                Decimal = (byte)(value & (byte)ProcessorStatusBit.DecimalMode) != 0;
                Interrupt = (byte)(value & (byte)ProcessorStatusBit.InterruptDisable) != 0;
                Zero = (byte)(value & (byte)ProcessorStatusBit.Zero) != 0;
                Carry = (byte)(value & (byte)ProcessorStatusBit.Carry) != 0;
            }
        }

        public string GetRegisterDisplay =>
             $"A:{A:X2} X:{X:X2} Y:{Y:X2} SP:{StackPointer:X2} PS:{ProcessorStatus:X2}";

        public string GetFlagsDisplay =>
            $"PS:{(Negative ? 1 : 0)}{(Overflow ? 1 : 0)}{1}{(Break ? 1 : 0)}{(Decimal ? 1 : 0)}{(Interrupt ? 1 : 0)}{(Zero ? 1 : 0)}{(Carry ? 1 : 0)}";

        /// <summary>
        /// direct access to the carry flag in the processor status register
        /// </summary>
        public bool Carry { get; set; }

        /// <summary>
        /// direct access to the zero flag in the processor status register
        /// </summary>
        public bool Zero { get; set; }

        /// <summary>
        /// direct access to the interrupt flag in the processor status register
        /// </summary>
        public bool Interrupt { get; set; }

        /// <summary>
        /// direct access to the decimal flag in the processor status register
        /// </summary>
#pragma warning disable CA1720
        public bool Decimal { get; set; }

        /// <summary>
        /// direct access to the BRK flag in the processor status register
        /// </summary>
        public bool Break { get; set; }

        /// <summary>
        /// direct access to the overflow flag in the processor status register
        /// </summary>
        public bool Overflow { get; set; }

        /// <summary>
        /// direct access to the negative flag in the processor status register
        /// </summary>
        public bool Negative { get; set; }
    }
}
