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
        CpuClass CpuClass { get; }

        Registers Registers { get; }

        void Reset();

        int Run(bool stopOnBreak = false, bool writeInstructions = false, int stepsPerSecond = 0);

        bool Step(bool writeInstructions = false, bool returnPriorToBreak = false);

        void StackPush(byte b);

        byte StackPop();

        void StackPushWord(ushort s);

        ushort StackPopWord();
    }
}
