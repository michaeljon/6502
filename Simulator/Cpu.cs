using System;
using System.Threading.Tasks;

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

        private const long TicksPerMicrosecond = 10;    // a tick is 100ns

        private long runningCycles;

        private long instructionsProcessed;

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
                opCodeDefinition = OpCodes.OpCode6502[opcode];

                if (opCodeDefinition.Nmemonic == null)
                {
                    // illegal opcode encountered, should dump core here
                    illegalOpCode = true;
                    break;
                }

                Execute(opCodeDefinition, writeInstructions);

                instructionsProcessed++;

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
            ProcessorStatus = 0x00;

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
            SET_DECIMAL(false);

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
                SET_DECIMAL(false);

                // load PC from reset vector
                ushort pcl = read(IrqVectorL);
                ushort pch = read(IrqVectorH);

                ProgramCounter = (ushort)((pch << 8) + pcl);
            }
        }

        public void PrintStatus()
        {
            var save = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write($"PC:{ProgramCounter:X4} A:{A:X2} X:{X:X2} Y:{Y:X2} SP:{StackPointer:X2} PS:{ProcessorStatus:X2} ");
            Console.WriteLine($"PS:{(Negative ? 1 : 0)}{(Overflow ? 1 : 0)}{1}{(Break ? 1 : 0)}{(Decimal ? 1 : 0)}{(Interrupt ? 1 : 0)}{(Zero ? 1 : 0)}{(Carry ? 1 : 0)}");
            Console.ForegroundColor = save;
        }

        public long Cycles => runningCycles;

        public long InstructionsProcessed => instructionsProcessed;

        public bool SkipTimingWait { get; set; }

        private void WaitCycles(long cycles)
        {
            runningCycles += cycles;

            if (SkipTimingWait == true)
            {
                return;
            }

            var t = Task.Run(async delegate
                          {
                              await Task.Delay(new TimeSpan(TicksPerMicrosecond * cycles));
                              return 0;
                          });
            t.Wait();
        }

        private void Execute(OpCodeDefinition opCodeDefinition, bool writeInstructions)
        {
            // we pulled one byte to decode the instruction, so we'll use that for display
            ushort savePC = (ushort)(ProgramCounter - 1);

            // decode the operand based on the opcode and addressind mode
            ushort src = opCodeDefinition.DecodeOperand(this);

            if (writeInstructions)
            {
                var save = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"{savePC:X4} {opCodeDefinition.Nmemonic} {src:X4}");
                Console.ForegroundColor = save;
            }

            opCodeDefinition.Execute(this, src);
            callback?.Invoke(this);
        }

        private void StackPush(byte b)
        {
            write((ushort)(0x0100 + StackPointer), b);
            StackPointer = (byte)((StackPointer == 0x00) ? 0xff : (StackPointer - 1));
        }

        private byte StackPop()
        {
            StackPointer = (byte)((StackPointer == 0xff) ? 0x00 : (StackPointer + 1));
            return read((ushort)(0x0100 + StackPointer));
        }
    }
}
