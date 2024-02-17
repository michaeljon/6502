namespace InnoWerks.Simulators
{
    public partial class Cpu
    {
        /// <summary>
        /// This register points the address from which the next instruction
        /// byte (opcode or parameter) will be fetched. Unlike other
        /// registers, this one is 16 bits in length. The low and high 8-bit
        /// halves of the register are called PCL and PCH, respectively. The
        /// Program Counter may be read by pushing its value on the stack.
        /// This can be done either by jumping to a subroutine or by causing
        /// an interrupt.
        /// </summary>
        public ushort ProgramCounter { get; set; }

        /// <summary>
        /// Accumulator
        /// </summary>
        public byte A { get; set; }

        /// <summary>
        /// X index register
        /// </summary>
        public byte X { get; set; }

        /// <summary>
        /// Y index register
        /// </summary>
        public byte Y { get; set; }

        /// <summary>
        /// Stack pointer
        /// </summary>
        public byte StackPointer { get; set; }

        /// <summary>
        /// Processor status register
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
    }
}
