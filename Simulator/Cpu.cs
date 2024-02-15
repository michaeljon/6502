using System;

namespace InnoWerks.Simulators
{
    public partial class Cpu
    {
        // IRQ, reset, NMI vectors
        public const ushort IrqVectorH = 0xFFFF;
        public const ushort IrqVectorL = 0xFFFE;

        public const ushort RstVectorH = 0xFFFD;
        public const ushort RstVectorL = 0xFFFC;

        public const ushort NmiVectorH = 0xFFFB;
        public const ushort NmiVectorL = 0xFFFA;

        private bool illegalOpCode;

        private readonly Func<ushort, byte> read;

        private readonly Action<ushort, byte> write;

        private readonly Action<Cpu> callback;

        public Cpu(Func<ushort, byte> read, Action<ushort, byte> write, Action<Cpu> callback)
        {
            this.read = read;
            this.write = write;
            this.callback = callback;

            A = 0;
            Y = 0;
            X = 0;
            StackPointer = 0xff;
            ProcessorStatus = 0x20;
        }

        public void Run(bool stopOnBreak, bool writeInstructions)
        {
            byte opcode;
            OpCodeDefinition opCodeDefinition;

            while (illegalOpCode == false)
            {
                opcode = read(ProgramCounter++);
                opCodeDefinition = OpCodes.OpCodeDefinitions[opcode];

                if (opCodeDefinition.Nmemonic == null)
                {
                    // illegal opcode encountered, should dump core here
                    illegalOpCode = true;
                    break;
                }

                Execute(opcode, opCodeDefinition, writeInstructions);

                if (opCodeDefinition.Nmemonic.Equals("BRK", StringComparison.Ordinal) && stopOnBreak)
                {
                    return;
                }
            }
        }

        public void Reset()
        {
            A = 0;
            Y = 0;
            X = 0;
            StackPointer = 0xff;
            ProcessorStatus = 0x20 | 0x10;

            // load PC from reset vector
            ushort pcl = read(RstVectorL);
            ushort pch = read(RstVectorH);
            ProgramCounter = (ushort)((pch << 8) + pcl);

            illegalOpCode = false;
        }

        public void NMI()
        {
            StackPush((byte)((ProgramCounter >> 8) & 0xff));
            StackPush((byte)(ProgramCounter & 0xff));
            StackPush((byte)((ProcessorStatus & 0xef) | 0x10));

            SET_INTERRUPT(true);

            // load PC from reset vector
            ushort pcl = read(NmiVectorL);
            ushort pch = read(NmiVectorH);

            ProgramCounter = (ushort)((pch << 8) + pcl);
        }

        public void IRQ()
        {
            if (IF_INTERRUPT())
            {
                StackPush((byte)((ProgramCounter >> 8) & 0xff));
                StackPush((byte)(ProgramCounter & 0xff));
                StackPush((byte)((ProcessorStatus & 0xef) | 0x10));

                SET_INTERRUPT(true);

                // load PC from reset vector
                ushort pcl = read(IrqVectorL);
                ushort pch = read(IrqVectorH);

                ProgramCounter = (ushort)((pch << 8) + pcl);
            }
        }

        public void PrintStatus()
        {
            Console.Write($"PC: 0x{ProgramCounter:X4} ");
            Console.Write($"A: 0x{A:X2} X: 0x{X:X2} Y: 0x{Y:X2} SP: 0x{StackPointer:X2} PS: 0x{ProcessorStatus:X2} ");
            Console.WriteLine($"PS: {(Negative ? 1 : 0)}{(Overflow ? 1 : 0)}{1}{(Break ? 1 : 0)}{(Decimal ? 1 : 0)}{(Interrupt ? 1 : 0)}{(Zero ? 1 : 0)}{(Carry ? 1 : 0)}");
        }

        private void Execute(byte opcode, OpCodeDefinition opCodeDefinition, bool writeInstructions)
        {
            // for display we need to 'reverse' the auto-increment
            ushort pc = (ushort)(ProgramCounter - 1);
            ushort src = opCodeDefinition.DecodeOperand(this);

            if (writeInstructions)
            {
                Console.WriteLine($"{pc:X4} {opCodeDefinition.Nmemonic} [{opcode:X2}] {src:X4}");
            }
            opCodeDefinition.Execute(this, src);
            callback?.Invoke(this);
        }

        private void StackPush(byte b)
        {
            write((ushort)(0x0100 + StackPointer), b);

            if (StackPointer == 0x00)
            {
                StackPointer = 0xff;
            }
            else
            {
                StackPointer--;
            }
        }

        private byte StackPop()
        {
            if (StackPointer == 0xff)
            {
                StackPointer = 0x00;
            }
            else
            {
                StackPointer++;
            }

            return read((ushort)(0x0100 + StackPointer));
        }
    }
}
