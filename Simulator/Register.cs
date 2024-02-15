//
// See: https://github.com/eteran/pretendo/blob/master/doc/cpu/6502.txt
//
namespace InnoWerks.Simulators
{
    /// <summary>
    /// 8-bit registers
    /// </summary>
    public enum Register
    {
        /// <summary>
        /// The accumulator is the main register for arithmetic and logic
        /// operations. Unlike the index registers X and Y, it has a direct
        /// connection to the Arithmetic and Logic Unit (ALU). This is why
        /// many operations are only available for the accumulator, not the
        /// index registers.
        /// </summary>
        A,

        /// <summary>
        /// This 8-bit register stores the state of the processor. The bits in
        /// this register are called flags. Most of the flags have something
        /// to do with arithmetic operations.
        ///
        /// The P register can be read by pushing it on the stack (with PHP or
        /// by causing an interrupt). If you only need to read one flag, you
        /// can use the branch instructions. Setting the flags is possible by
        /// pulling the P register from stack or by using the flag set or
        /// clear instructions.
        /// </summary>
        P,

        /// <summary>
        /// The NMOS 65xx processors have 256 bytes of stack memory, ranging
        ///  from $0100 to $01FF. The S register is a 8-bit offset to the stack
        ///  page. In other words, whenever anything is being pushed on the
        ///  stack, it will be stored to the address $0100+S.
        ///
        ///  The Stack pointer can be read and written by transfering its value
        ///  to or from the index register X (see below) with the TSX and TXS
        ///  instructions.
        /// </summary>
        SP,

        /// <summary>
        /// This is the main register for addressing data with indices. It has
        /// a special addressing mode, indexed indirect, which lets you to
        /// have a vector table on the zero page.
        /// </summary>
        X,

        /// <summary>
        /// The Y register has the least operations available. On the other
        /// hand, only it has the indirect indexed addressing mode that
        /// enables access to any memory place without having to use
        /// self-modifying code.
        /// </summary>
        Y
    }
}
