using InnoWerks.Processors;

//
// things to note: http://www.6502.org/tutorials/65c02opcodes.html
//                 https://xotmatrix.github.io/6502/6502-single-cycle-execution.html
//

#pragma warning disable RCS1163, IDE0060, CA1707, CA1822, CA1716

namespace InnoWerks.Simulators
{
    public interface ICpu
    {
        /// <summary>
        /// Describes the type of CPU: 6502, WDC 65C02, Synertek 65SC02, etc
        /// </summary>
        CpuClass CpuClass { get; }

        /// <summary>
        /// Provides access to the CPU's register set
        /// </summary>
        Registers Registers { get; }

        /// <summary>
        /// Resets the CPU state to it's cold boot state
        /// </summary>
        void Reset();

        /// <summary>
        /// Puts the CPU in a free-run state where it simply executes a
        /// complete program. This is only used for test scenarios.
        /// </summary>
        /// <param name="stopOnBreak"></param>
        /// <param name="writeInstructions"></param>
        /// <param name="stepsPerSecond"></param>
        /// <returns></returns>
        (int intructionCount, int cycleCount) Run(bool stopOnBreak = false, bool writeInstructions = false, int stepsPerSecond = 0);

        /// <summary>
        /// Runs a single cycle-accurate CPU instruction
        /// </summary>
        /// <param name="writeInstructions"></param>
        /// <param name="returnPriorToBreak"></param>
        /// <returns>true if the CPU encounters a BRK instruction</returns>
        int Step(bool writeInstructions = false, bool returnPriorToBreak = false);

        /// <summary>
        /// Pushes a byte onto the stack at the current value of SP
        /// and adjusts SP accordingly.
        /// </summary>
        /// <param name="b"></param>
        void StackPush(byte b);

        /// <summary>
        /// Pops a byte from the stack at the current value of SP
        /// and adjusts SP accordingly.
        /// </summary>
        /// <returns></returns>
        byte StackPop();

        /// <summary>
        /// Pushes a word to the stack at the current value of SP
        /// and adjusts the stack accordingly. Pushes the high-order
        /// byte first, then the low-order byte.
        /// </summary>
        /// <param name="s"></param>
        void StackPushWord(ushort s);

        /// <summary>
        /// Pops a word from the stack at the current value of SP
        /// and adjusts the stack accordingly. Pops the low-order
        /// byte first, followed by the high-order byte.
        /// </summary>
        /// <returns></returns>
        ushort StackPopWord();
    }
}
