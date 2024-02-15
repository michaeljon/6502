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
        public ushort ProgramCounter { get; private set; }

        /// <summary>
        /// Accumulator
        /// </summary>
        public byte A { get; private set; }

        /// <summary>
        /// X index register
        /// </summary>
        public byte X { get; private set; }

        /// <summary>
        /// Y index register
        /// </summary>
        public byte Y { get; private set; }

        /// <summary>
        /// Stack pointer
        /// </summary>
        public byte StackPointer { get; private set; }

        /// <summary>
        /// Processor status register
        /// </summary>
        public byte ProcessorStatus { get; private set; }
    }
}
