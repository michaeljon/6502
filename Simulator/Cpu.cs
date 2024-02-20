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

        private readonly Func<Cpu, bool> stepHandler;

        private readonly Func<Cpu, bool> interruptHandler;

        private readonly Func<Cpu, bool> breakHandler;

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

        public Registers Registers { get; private set; }

        public Cpu(
            Func<ushort, byte> read,
            Action<ushort, byte> write,
            Action<Cpu> callback,
            Func<Cpu, bool> stepHandler = null,
            Func<Cpu, bool> interruptHandler = null,
            Func<Cpu, bool> breakHandler = null)
        {
            Registers = new();

            this.read = read;
            this.write = write;
            this.callback = callback;

            this.stepHandler = stepHandler;
            this.interruptHandler = interruptHandler;
            this.breakHandler = breakHandler;

            Registers.Reset();
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

                if (opCodeDefinition.Nmemonic.Equals("BRK", StringComparison.Ordinal) && stopOnBreak)
                {
                    return;
                }

                Execute(opCodeDefinition, writeInstructions);

                instructionsProcessed++;
            }
        }

        public void Reset()
        {
            Registers.Reset();

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
            StackPush((byte)((Registers.ProcessorStatus & 0xef) | 0x10));

            Registers.Interrupt = true;
            Registers.Decimal = false;

            // load PC from reset vector
            ushort pcl = read(NmiVectorL);
            ushort pch = read(NmiVectorH);

            ProgramCounter = (ushort)((pch << 8) + pcl);
        }

        public void IRQ()
        {
            if (Registers.Interrupt == true)
            {
                StackPush((byte)((ProgramCounter >> 8) & 0xff));
                StackPush((byte)(ProgramCounter & 0xff));
                StackPush((byte)((Registers.ProcessorStatus & 0xef) | 0x10));

                Registers.Interrupt = true;
                Registers.Decimal = false;

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
            Console.Write($"PC:{ProgramCounter:X4} {Registers.GetRegisterDisplay} ");
            Console.WriteLine(Registers.GetFlagsDisplay);
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
            write((ushort)(0x0100 + Registers.StackPointer), b);
            Registers.StackPointer = (byte)((Registers.StackPointer == 0x00) ? 0xff : (Registers.StackPointer - 1));
        }

        private byte StackPop()
        {
            Registers.StackPointer = (byte)((Registers.StackPointer == 0xff) ? 0x00 : (Registers.StackPointer + 1));
            return read((ushort)(0x0100 + Registers.StackPointer));
        }
    }
}
