using System;
using System.Threading.Tasks;

#pragma warning disable RCS1163, IDE0060

namespace InnoWerks.Simulators
{
    public class Cpu
    {
        // IRQ, reset, NMI vectors
        public const ushort IrqVectorH = 0xFFFF;
        public const ushort IrqVectorL = 0xFFFE;

        public const ushort RstVectorH = 0xFFFD;
        public const ushort RstVectorL = 0xFFFC;

        public const ushort NmiVectorH = 0xFFFB;
        public const ushort NmiVectorL = 0xFFFA;

        public const ushort StackBase = 0x0100;

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

        public Registers Registers { get; private set; }

        public string OperandDisplay { get; private set; }

        public CpuClass CpuClass { get; private set; }

        public Cpu(
            CpuClass cpuClass,
            Func<ushort, byte> read,
            Action<ushort, byte> write,
            Action<Cpu> callback,
            Func<Cpu, bool> stepHandler = null,
            Func<Cpu, bool> interruptHandler = null,
            Func<Cpu, bool> breakHandler = null)
        {
            CpuClass = cpuClass;

            Registers = new();

            this.read = read;
            this.write = write;
            this.callback = callback;

            this.stepHandler = stepHandler;
            this.interruptHandler = interruptHandler;
            this.breakHandler = breakHandler;

            Registers.Reset();
        }

        #region Execution
        public void Run(bool stopOnBreak, bool writeInstructions)
        {
            byte opcode;
            OpCodeDefinition opCodeDefinition;

            while (illegalOpCode == false)
            {
                opcode = read(Registers.ProgramCounter++);

                if (CpuClass == CpuClass.WDC6502)
                {
                    opCodeDefinition = OpCodes.OpCode6502[opcode];
                }
                else
                {
                    opCodeDefinition = OpCodes.OpCode65C02[opcode];
                }

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
            Registers.Reset();

            // load PC from reset vector
            byte pcl = read(RstVectorL);
            byte pch = read(RstVectorH);

            Registers.ProgramCounter = RegisterMath.MakeShort(pch, pcl);

            illegalOpCode = false;
        }

        public void NMI()
        {
            StackPush(RegisterMath.HighByte(Registers.ProgramCounter));
            StackPush(RegisterMath.LowByte(Registers.ProgramCounter));
            StackPush((byte)((Registers.ProcessorStatus & 0xef) | (byte)ProcessorStatusBit.Unused));

            Registers.Interrupt = true;
            Registers.Decimal = false;

            // load PC from reset vector
            byte pcl = read(NmiVectorL);
            byte pch = read(NmiVectorH);

            Registers.ProgramCounter = RegisterMath.MakeShort(pch, pcl);
        }

        public void IRQ()
        {
            if (Registers.Interrupt == true)
            {
                NMI();
            }
        }

        public void PrintStatus()
        {
            // var save = Console.ForegroundColor;
            // Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write($"PC:{Registers.ProgramCounter:X4} {Registers.GetRegisterDisplay} ");
            Console.WriteLine(Registers.GetFlagsDisplay);
            // Console.ForegroundColor = save;
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
            ushort savePC = (ushort)(Registers.ProgramCounter - 1);

            // decode the operand based on the opcode and addressing mode
            ushort src = opCodeDefinition.DecodeOperand(this);

            // var save = Console.ForegroundColor;
            if (writeInstructions)
            {
                // Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write($"{savePC:X4} {opCodeDefinition.Nmemonic}   {OperandDisplay,-10}");
            }

            opCodeDefinition.Execute(this, src);

            if (writeInstructions)
            {
                Console.Write($"  {Registers.GetRegisterDisplay} ");
                Console.WriteLine($"  {Registers.InternalGetFlagsDisplay,-8}");
                // Console.ForegroundColor = save;
            }

            callback?.Invoke(this);
        }

        private void StackPush(byte b)
        {
            write((ushort)(StackBase + Registers.StackPointer), b);
            Registers.StackPointer--;
        }

        private byte StackPop()
        {
            Registers.StackPointer++;
            return read((ushort)(StackBase + Registers.StackPointer));
        }
        #endregion

        //
        // CPU Instructions
        //
        #region InstructionDefinitions
        /// <summary>
        /// <para>ADC - Add with Carry 65C02</para>
        /// <code>
        /// Flags affected: nv----zc
        ///
        /// A ← A + M + c
        ///
        /// n ← Most significant bit of result
        /// v ← Signed overflow of result
        /// z ← Set if the result is zero
        /// c ← Carry from ALU (bit 8/16 of result)
        /// </code>
        /// </summary>
        public void ADCCMOS(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            int val = 0;
            int temp = read(addr);
            Registers.Overflow = ((Registers.A ^ temp) & 0x80) == 0;

            if (Registers.Decimal == true)
            {
                cycles++;

                val = (Registers.A & 0x0f) + (temp & 0x0f) + (Registers.Carry ? 0x01 : 0x00);
                if (val >= 0x0a)
                {
                    val = 0x10 | ((val + 0x06) & 0x0f);
                }
                val += (Registers.A & 0xf0) + (temp & 0xf0);
                if (val >= 0xa0)
                {
                    Registers.Carry = true;
                    if (val >= 0x180)
                    {
                        Registers.Overflow = false;
                    }
                    val += 0x60;
                }
                else
                {
                    Registers.Carry = false;
                    if (val < 0x80)
                    {
                        Registers.Overflow = false;
                    }
                }
            }
            else
            {
                val = Registers.A + temp + (Registers.Carry ? 0x01 : 0x00);
                if (val >= 0x100)
                {
                    Registers.Carry = true;
                    if (val >= 0x180)
                    {
                        Registers.Overflow = false;
                    }
                }
                else
                {
                    Registers.Carry = false;
                    if (val < 0x80)
                    {
                        Registers.Overflow = false;
                    }
                }
            }

            Registers.A = RegisterMath.TruncateToByte(val);
            Registers.SetNZ(Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>ADC - Add with Carry 6502</para>
        /// <code>
        /// Flags affected: nv----zc
        ///
        /// A ← A + M + c
        ///
        /// n ← Most significant bit of result
        /// v ← Signed overflow of result
        /// z ← Set if the result is zero
        /// c ← Carry from ALU (bit 8/16 of result)
        /// </code>
        /// </summary>
        public void ADCNMOS(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            int temp = read(addr);

            if (Registers.Decimal == true)
            {
                cycles++;

                int val = (Registers.A & 0x0f) + (temp & 0x0f) + (Registers.Carry ? 0x01 : 0x00);
                if (val > 0x09)
                {
                    val += 0x06;
                }
                if (val <= 0x0f)
                {
                    val = (val & 0x0f) + (Registers.A & 0xf0) + (temp & 0xf0);
                }
                else
                {
                    val = (val & 0x0f) + (Registers.A & 0xf0) + (temp & 0xf0) + 0x10;
                }

                Registers.Zero = RegisterMath.IsNonZero((Registers.A + temp + (Registers.Carry ? 0x01 : 0x00)) & 0xff);
                Registers.Negative = RegisterMath.IsHighBitSet(val);
                Registers.Overflow = RegisterMath.IsHighBitSet(Registers.A ^ val) && RegisterMath.IsHighBitClear(Registers.A ^ temp);

                if ((val & 0x1f0) > 0x90)
                {
                    val += 0x60;
                }

                Registers.Carry = (val & 0xff0) > 0xf0;
                Registers.A = RegisterMath.TruncateToByte(val);
            }
            else
            {
                int val = Registers.A + temp + (Registers.Carry ? 0x01 : 0x00);

                Registers.Carry = val > 0xff;
                Registers.Overflow = (((Registers.A & 0x80) == (temp & 0x80)) &&
                                      ((Registers.A & 0x80) != (val & 0x80)));
                Registers.A = RegisterMath.TruncateToByte(val);
                Registers.SetNZ(Registers.A);
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>AND - And Accumulator with Memory</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// A ← A &amp; M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void AND(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            var value = read(addr);
            var intermediate = Registers.A & value;
            Registers.A = RegisterMath.TruncateToByte(intermediate);

            Registers.SetNZ(Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>ASL - Arithmetic Shift Left</para>
        /// <code>
        /// Flags affected: n-----zc
        ///
        /// M ← M + M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// c ← Most significant bit of original Memory
        /// </code>
        /// </summary>
        public void ASL(ushort addr, bool accumulator, long cycles, long pageCrossPenalty = 0)
        {
            int value = accumulator ? Registers.A : read(addr);

            Registers.Carry = ((byte)(value & 0x80)) == 0x80;

            value = (byte)(0xfe & (value << 1));

            Registers.SetNZ(value);

            if (accumulator == true)
            {
                Registers.A = RegisterMath.TruncateToByte(value);
            }
            else
            {
                write(addr, RegisterMath.TruncateToByte(value));
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BBR - Branch on Bit Reset</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        ///
        /// <para>The specified bit in the zero page location specified in the
        /// operand is tested. If it is clear (reset), a branch is taken; if it is
        /// set, the instruction immediately following the two-byte BBRx instruction
        /// is executed. The bit is specified by a number (0 through 7)
        /// concatenated to the end of the mnemonic.</para>
        ///
        /// <para>If the branch is performed, the third byte of the instruction is used
        /// as a signed displacement from the program counter; that is, it is added
        /// to the program counter: a positive value(numbers less than or equal to
        /// $80; that is, numbers with the high-order bit clear) results in a branch
        /// to a higher location; a negative value(greater than $80, with the
        /// high-order bit set) results in a branch to a lower location.Once the branch
        /// address is calculated, the result is loaded into the program counter,
        /// transferring control to that location.</para>
        /// </summary>
        public void BBR(ushort addr, byte bit, long cycles, long pageCrossPenalty = 0)
        {
            if ((addr & (0x01 << bit)) == 0)
            {
                ushort savePC = Registers.ProgramCounter;

                sbyte offset = (sbyte)read(Registers.ProgramCounter++);
                Registers.ProgramCounter = (ushort)(Registers.ProgramCounter + offset);

                if ((savePC & 0xff00) != (Registers.ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BBS - Branch on Bit Set</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        ///
        /// <para>The specified bit in the zero page location specified in the
        /// operand is tested. If it is set, a branch is taken; if it is
        /// clear (reset), the instructions immediately following the
        /// two-byte BBSx instruction is executed. The bit is specified
        /// by a number (0 through 7) concatenated to the end of the mnemonic.</para>
        ///
        /// <para>If the branch is performed, the third byte of the instruction
        /// is used as a signed displacement from the program counter; that
        /// is, it is added to the program counter: a positive value (numbers
        /// less than or equal to $80; that is, numbers with the high order
        /// bit clear) results in a branch to a higher location; a negative
        /// value (greater than $80, with the high- order bit set) results in
        /// a branch to a lower location. Once the branch address is calculated,
        /// the result is loaded into the program counter, transferring control
        /// to that location.</para>
        /// </summary>
        public void BBS(ushort addr, byte bit, long cycles, long pageCrossPenalty = 0)
        {
            if ((addr & (0x01 << bit)) != 0)
            {
                ushort savePC = Registers.ProgramCounter;

                sbyte offset = (sbyte)read(Registers.ProgramCounter++);
                Registers.ProgramCounter = (ushort)(Registers.ProgramCounter + offset);

                if ((savePC & 0xff00) != (Registers.ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BCC - Branch on Carry Clear</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        /// </summary>
        public void BCC(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if (Registers.Carry == false)
            {
                if ((addr & 0xff00) != (Registers.ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                Registers.ProgramCounter = addr;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BCS - Branch on Carry Set</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        /// </summary>
        public void BCS(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if (Registers.Carry == true)
            {
                if ((addr & 0xff00) != (Registers.ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                Registers.ProgramCounter = addr;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BEQ - Branch on Result Zero</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        /// </summary>
        public void BEQ(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if (Registers.Zero == true)
            {
                if ((addr & 0xff00) != (Registers.ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                Registers.ProgramCounter = addr;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BIT - Test Memory Bits against Accumulator</para>
        /// <code>
        /// Flags affected: nv----z-
        /// Flags affected (Immediate addressing mode only): ------z-
        ///
        /// A &amp; M
        ///
        /// n ← Most significant bit of memory
        /// v ← Second most significant bit of memory
        /// z ← Set if logical AND of memory and Accumulator is zero
        /// </code>
        /// </summary>
        public void BIT(ushort addr, long cycles, bool immediateMode, long pageCrossPenalty = 0)
        {
            if (immediateMode == true)
            {
                // see http://www.6502.org/tutorials/vflag.html
                Registers.Zero = RegisterMath.IsZero(Registers.A & read(addr));
            }
            else
            {
                byte val = read(addr);
                Registers.Zero = RegisterMath.IsZero(Registers.A & val);
                Registers.Negative = ((byte)(val & 0x80)) == 0x80;
                Registers.Overflow = ((byte)(val & 0x40)) == 0x40;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BMI - Branch on Result Minus</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        /// </summary>
        public void BMI(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if (Registers.Negative == true)
            {
                if ((addr & 0xff00) != (Registers.ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                Registers.ProgramCounter = addr;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BNE - Branch on Not Equal</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        /// </summary>
        public void BNE(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if (Registers.Zero == false)
            {
                if ((addr & 0xff00) != (Registers.ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                Registers.ProgramCounter = addr;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BPL - Branch on Result Plus</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        /// </summary>
        public void BPL(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if (Registers.Negative == false)
            {
                if ((addr & 0xff00) != (Registers.ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                Registers.ProgramCounter = addr;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BRA - Branch Always</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        /// </summary>
        public void BRA(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if ((addr & 0xff00) != (Registers.ProgramCounter & 0xff00))
            {
                cycles += pageCrossPenalty;
            }

            Registers.ProgramCounter = addr;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BRL - Branch Long</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        /// </summary>
        public void BRL(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if ((addr & 0xff00) != (Registers.ProgramCounter & 0xff00))
            {
                cycles += pageCrossPenalty;
            }

            Registers.ProgramCounter += addr;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BRK - Force Break</para>
        /// <code>
        /// Flags affected: ----di--
        ///
        /// S     ← S - 3
        /// [S+3] ← PC.h
        /// [S+2] ← PC.l
        /// [S+1] ← P
        /// d     ← 0
        /// i     ← 1
        /// P     ← 0
        /// PC    ← interrupt address
        /// </code>
        /// </summary>
        public void BRK(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            // this should probably be delegated "up" so it can
            // be reported out to the caller

            Registers.ProgramCounter++;
            StackPush(RegisterMath.HighByte(Registers.ProgramCounter));
            StackPush(RegisterMath.LowByte(Registers.ProgramCounter));
            StackPush(Registers.ProcessorStatus);

            Registers.Decimal = false;
            Registers.Break = true;
            Registers.Interrupt = true;

            Registers.ProgramCounter = RegisterMath.MakeShort(read(IrqVectorH), read(IrqVectorL));

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BBR - Branch on Overflow Clear</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        /// </summary>
        public void BVC(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if (Registers.Overflow == false)
            {
                Registers.ProgramCounter = addr;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BBR - Branch on Overflow Set</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </code>
        /// </summary>
        public void BVS(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if (Registers.Overflow == true)
            {
                Registers.ProgramCounter = addr;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>CLC - Clear Carry</para>
        /// <code>
        /// Flags affected: -------c
        ///
        /// c ← 0
        /// </code>
        /// </summary>
        public void CLC(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Carry = false;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>CLC - Clear Decimal</para>
        /// <code>
        /// Flags affected: ----d---
        ///
        /// d ← 0
        /// </code>
        /// </summary>
        public void CLD(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Decimal = false;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>CLC - Clear Interrupt</para>
        /// <code>
        /// Flags affected: -----i--
        ///
        /// i ← 0
        /// </code>
        /// </summary>
        public void CLI(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Interrupt = false;

            // this should probably be reported "up" so that a
            // subsequent interrupt is handled properly

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>CLV - Clear Overflow</para>
        /// <code>
        /// Flags affected: -v------
        ///
        /// v ← 0
        /// </code>
        /// </summary>
        public void CLV(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Overflow = false;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>CMP - Compare Accumulator with Memory</para>
        /// <code>
        /// Flags affected: n-----zc
        ///
        /// A - M
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero (Set if A == M)
        /// c ← Carry from ALU (Set if A >= M)
        /// </code>
        /// </summary>
        public void CMP(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            int val = read(addr);

            Registers.Carry = Registers.A >= val;
            val = Registers.A - val;

            Registers.SetNZ(val);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>CPX - Compare Index Register X with Memory</para>
        /// <code>
        /// Flags affected: n-----zc
        ///
        /// X - M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero (Set if X == M)
        /// c ← Carry from ALU (Set if X >= M)
        /// </code>
        /// </summary>
        public void CPX(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            int val = read(addr);

            Registers.Carry = Registers.X >= val;
            val = Registers.X - val;

            Registers.SetNZ(val);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>CPY - Compare Index Register Y with Memory</para>
        /// <code>
        /// Flags affected: n-----zc
        ///
        /// Y - M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero (Set if Y == M)
        /// c ← Carry from ALU (Set if Y >= M)
        /// </code>
        /// </summary>
        public void CPY(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            int val = read(addr);

            Registers.Carry = Registers.Y >= val;
            val = Registers.Y - val;

            Registers.SetNZ(val);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>DEC - Decrement</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// M ← M - 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void DEC(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            byte value = RegisterMath.Dec(read(addr));
            Registers.SetNZ(value);
            write(addr, value);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>DEX - Decrement Index Registers</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// X ← X - 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void DEX(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.X = RegisterMath.Dec(Registers.X);
            Registers.SetNZ(Registers.X);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>DEY - Decrement Index Registers</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// Y ← Y - 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void DEY(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Y = RegisterMath.Dec(Registers.Y);
            Registers.SetNZ(Registers.Y);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>EOR - Exclusive OR Accumulator with Memory</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// A ← A ^ M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void EOR(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            var value = read(addr);
            var intermediate = Registers.A ^ value;
            Registers.A = RegisterMath.TruncateToByte(intermediate);

            Registers.SetNZ(Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>INC - Increment</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// M ← M + 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void INC(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            byte val = RegisterMath.Inc(read(addr));
            Registers.SetNZ(val);
            write(addr, val);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>INX - Increment Index Registers</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// X ← X + 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void INX(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.X = RegisterMath.Inc(Registers.X);
            Registers.SetNZ(Registers.X);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>INY - Increment Index Registers</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// Y ← Y + 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void INY(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Y = RegisterMath.Inc(Registers.Y);
            Registers.SetNZ(Registers.Y);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>JMP - Jump</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// JMP:
        /// PC     ← M
        /// </code>
        /// </summary>
        public void JMP(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if ((addr & 0xff00) != (Registers.ProgramCounter & 0xff00))
            {
                cycles += pageCrossPenalty;
            }

            Registers.ProgramCounter = addr;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>JSR - Jump to Subroutine</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// JSR:
        /// PC     ← PC - 1
        /// S      ← S - 2
        /// [S+2]  ← PC.h
        /// [S+1]  ← PC.l
        /// PC     ← M
        /// </code>
        /// </summary>
        public void JSR(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            ushort savePC = Registers.ProgramCounter;

            Registers.ProgramCounter--;

            StackPush(RegisterMath.HighByte(Registers.ProgramCounter));
            StackPush(RegisterMath.LowByte(Registers.ProgramCounter));

            Registers.ProgramCounter = addr;

            if ((savePC & 0xff00) != (Registers.ProgramCounter & 0xff00))
            {
                cycles += pageCrossPenalty;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>LDA - Load Accumulator from Memory</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// A ← M
        ///
        /// n ← Most significant bit of Accumulator
        /// z ← Set if the Accumulator is zero
        /// </code>
        /// </summary>
        public void LDA(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            Registers.A = read(addr);
            Registers.SetNZ(Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>LDX - Load Index Register X from Memory</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// X ← M
        ///
        /// n ← Most significant bit of X
        /// z ← Set if the X is zero
        /// </code>
        /// </summary>
        public void LDX(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            Registers.X = read(addr);
            Registers.SetNZ(Registers.X);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>LDY - Load Index Register Y from Memory</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// Y ← M
        ///
        /// n ← Most significant bit of Y
        /// z ← Set if the Y is zero
        /// </code>
        /// </summary>
        public void LDY(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Y = read(addr);
            Registers.SetNZ(Registers.Y);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>LSR - Logical Shift Right</para>
        /// <code>
        /// Flags affected: n-----zc
        ///
        /// M ← M >> 1
        ///
        /// n ← cleared
        /// z ← Set if the result is zero
        /// c ← Bit 0 of original memory
        /// </code>
        ///
        /// NOTE: This is an unsigned operation, the MSB of the result is always 0.
        /// </summary>
        public void LSR(ushort addr, bool accumulator, long cycles, long pageCrossPenalty = 0)
        {
            if (accumulator == false)
            {
                int val = read(addr);
                Registers.Carry = (val & 0x01) != 0;
                Registers.Negative = false;
                val >>= 1;
                Registers.SetNZ(val);
                write(addr, RegisterMath.TruncateToByte(val));
            }
            else
            {
                Registers.Carry = (Registers.A & 0x01) != 0;
                Registers.Negative = false;
                Registers.A >>= 1;
                Registers.SetNZ(Registers.A);
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>NOP - No Operation</para>
        /// <code>
        /// Flags affected: --------
        /// </code>
        /// </summary>
        public void NOP(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>ORA - OR Accumulator with Memory</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// A ← A | M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void ORA(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            var value = read(addr);
            var intermediate = Registers.A | value;
            Registers.A = RegisterMath.TruncateToByte(intermediate);

            Registers.SetNZ(Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>PHA - Push A to Stack</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// 8 bit register:
        /// S     ← S - 1
        /// [S+1] ← R
        /// </code>
        /// </summary>
        public void PHA(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            StackPush(Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>PHP - Push PS to Stack</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// 8 bit register:
        /// S     ← S - 1
        /// [S+1] ← R
        /// </code>
        /// </summary>
        public void PHP(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            StackPush(Registers.ProcessorStatus);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>PHX - Push X to Stack</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// 8 bit register:
        /// S     ← S - 1
        /// [S+1] ← R
        /// </code>
        /// </summary>
        public void PHX(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            StackPush(Registers.X);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>PHY - Push Y to Stack</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// S     ← S - 1
        /// [S+1] ← R
        /// </code>
        /// </summary>
        public void PHY(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            StackPush(Registers.Y);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>PLA - Pull A from Stack</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// A   ← [S+1]
        /// S   ← S + 1
        ///
        /// n   ← Most significant bit of register
        /// z   ← Set if the register is zero
        /// </code>
        /// </summary>
        public void PLA(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.A = StackPop();
            Registers.SetNZ(Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>PLP - Pull PS from Stack</para>
        /// <code>
        /// Flags affected (PLP): nvmxdizc
        ///
        /// P   ← [S+1]
        /// S   ← S + 1
        /// </code>
        /// </summary>
        public void PLP(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.ProcessorStatus = StackPop();

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>PLX - Pull X from Stack</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// X   ← [S+1]
        /// S   ← S + 1
        ///
        /// n   ← Most significant bit of register
        /// z   ← Set if the register is zero
        /// </code>
        /// </summary>
        public void PLX(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.X = StackPop();
            Registers.SetNZ(Registers.X);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>PLY - Pull Y from Stack</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// Y   ← [S+1]
        /// S   ← S + 1
        ///
        /// n   ← Most significant bit of register
        /// z   ← Set if the register is zero
        /// </code>
        /// </summary>
        public void PLY(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Y = StackPop();
            Registers.SetNZ(Registers.Y);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>RMB - Reset Memory Bit</para>
        /// <code>
        /// Flags affected: -------
        ///
        /// Clear the specified bit in the zero page memory location
        /// specified in the operand. The bit to clear is specified
        /// by a number (0 through 7) concatenated to the end of the
        /// mnemonic.
        /// </code>
        /// </summary>
        public void RMB(ushort addr, byte bit, long cycles, long pageCrossPenalty = 0)
        {
            int flag = 0x01 << bit;
            int value = read(addr);
            value &= ~flag;
            write(addr, RegisterMath.TruncateToByte(value));

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>ROL - Rotate Left</para>
        /// <code>
        /// Flags affected: n-----zc
        ///
        /// M ← M + M + c
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// c ← Most significant bit of original Memory
        /// </code>
        /// </summary>
        public void ROL(ushort addr, bool accumulator, long cycles, long pageCrossPenalty = 0)
        {
            // ROL
            if (accumulator == false)
            {
                int val = read(addr) << 1 | (Registers.Carry ? 0x01 : 0x00);
                Registers.Carry = val > 0xff;

                Registers.SetNZ(val);
                write(addr, RegisterMath.TruncateToByte(val));
            }
            else
            {
                int val = Registers.A << 1 | (Registers.Carry ? 0x01 : 0x00);
                Registers.Carry = val > 0xff;

                Registers.A = RegisterMath.TruncateToByte(val);
                Registers.SetNZ(val);
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>ROR - Rotate Right</para>
        /// <code>
        /// Flags affected: n-----zc
        ///
        /// M ← (c &lt;&lt; (m ? 7 : 15)) | (M &gt;&gt; 1)
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// c ← Bit 0 of original memory
        /// </code>
        /// </summary>
        public void ROR(ushort addr, bool accumulator, long cycles, long pageCrossPenalty = 0)
        {
            // ROR
            if (accumulator == false)
            {
                int temp = read(addr);
                int val = (temp >> 1) | (Registers.Carry ? 0x80 : 0x00);
                Registers.Carry = (temp & 0x01) != 0;

                Registers.SetNZ(val);
                write(addr, RegisterMath.TruncateToByte(val));
            }
            // ROA
            else
            {
                int val = (Registers.A >> 1) | (Registers.Carry ? 0x80 : 0x00);
                Registers.Carry = (Registers.A & 0x01) != 0;
                Registers.A = RegisterMath.TruncateToByte(val);
                Registers.SetNZ(val);
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>RTI - Return From Interrupt</para>
        /// <code>
        /// Flags affected: nvmxdizc
        ///
        /// P    ← [S+1]
        /// PC.l ← [S+2]
        /// PC.h ← [S+3]
        /// S    ← S + 3
        /// </code>
        /// </summary>
        public void RTI(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            // this should probably be delegated "up" so that
            // the cpu can notify the caller

            Registers.ProcessorStatus = StackPop();

            byte lo = StackPop();
            byte hi = StackPop();

            Registers.ProgramCounter = RegisterMath.MakeShort(hi, lo);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>RTS  - Return From Subroutine</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// PC.l ← [S+1]
        /// PC.h ← [S+2]
        /// S    ← S + 2
        /// PC   ← PC + 1
        /// </code>
        /// </summary>
        public void RTS(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            byte lo = StackPop();
            byte hi = StackPop();

            Registers.ProgramCounter = RegisterMath.MakeShort(hi, lo);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>SBC - Subtract with Borrow from Accumulator 6502</para>
        /// <code>
        /// Flags affected: nv----zc
        ///
        /// A ← A + (~M) + c
        ///
        /// n ← Most significant bit of result
        /// v ← Signed overflow of result
        /// z ← Set if the Accumulator is zero
        /// c ← Carry from ALU(bit 8/16 of result) (set if borrow not required)
        /// </code>
        /// </summary>
        public void SBCNMOS(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            int temp = read(addr);
            int temp2 = Registers.A - temp - (Registers.Carry ? 0x00 : 0x01);

            if (Registers.Decimal == true)
            {
                cycles++;

                int val = (Registers.A & 0x0f) - (temp & 0x0f) - (Registers.Carry ? 0x00 : 0x01);
                if ((val & 0x10) != 0)
                {
                    val = ((val - 0x06) & 0x0f) | ((Registers.A & 0xf0) - (temp & 0xf0) - 0x10);
                }
                else
                {
                    val = (val & 0x0f) | ((Registers.A & 0xf0) - (temp & 0xf0));
                }
                if ((val & 0x100) != 0)
                {
                    val -= 0x60;
                }

                Registers.Carry = temp2 < 0x100;
                Registers.SetNZ(temp2 & 0xff);
                Registers.Overflow = RegisterMath.IsHighBitSet(Registers.A ^ temp2) &&
                                     RegisterMath.IsHighBitSet(Registers.A ^ temp);
                Registers.A = RegisterMath.TruncateToByte(val);
            }
            else
            {
                int val = temp2;

                Registers.Carry = val < 0x100;
                Registers.Overflow = (((Registers.A & 0x80) != (temp & 0x80)) &&
                                      ((Registers.A & 0x80) != (val & 0x80)));
                Registers.A = RegisterMath.TruncateToByte(val);
                Registers.SetNZ(Registers.A);
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>SBC - Subtract with Borrow from Accumulator 65C02</para>
        /// <code>
        /// Flags affected: nv----zc
        ///
        /// A ← A + (~M) + c
        ///
        /// n ← Most significant bit of result
        /// v ← Signed overflow of result
        /// z ← Set if the Accumulator is zero
        /// c ← Carry from ALU(bit 8/16 of result) (set if borrow not required)
        /// </code>
        /// </summary>
        public void SBCCMOS(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            int val = 0;
            int temp = read(addr);
            Registers.Overflow = ((Registers.A ^ temp) & 0x80) != 0;

            if (Registers.Decimal == true)
            {
                cycles++;

                int temp2 = 0x0f + (Registers.A & 0x0f) - (temp & 0x0f) + (Registers.Carry ? 0x01 : 0x00);
                if (temp2 < 0x10)
                {
                    val = 0;
                    temp2 -= 0x06;
                }
                else
                {
                    val = 0x10;
                    temp2 -= 0x10;
                }

                val += 0xf0 + (Registers.A & 0xf0) - (temp & 0xf0);

                if (val < 0x100)
                {
                    Registers.Carry = false;
                    if (val < 0x80)
                    {
                        Registers.Overflow = false;
                    }
                    val -= 0x60;
                }
                else
                {
                    Registers.Carry = true;
                    if (val >= 0x180)
                    {
                        Registers.Overflow = false;
                    }
                }
                val += temp2;
            }
            else
            {
                val = 0xff + Registers.A - temp + (Registers.Carry ? 0x01 : 0x00);
                if (val < 0x100)
                {
                    Registers.Carry = false;
                    if (val < 0x80)
                    {
                        Registers.Overflow = false;
                    }
                }
                else
                {
                    Registers.Carry = true;
                    if (val >= 0x180)
                    {
                        Registers.Overflow = false;
                    }
                }
            }

            Registers.A = RegisterMath.TruncateToByte(val);
            Registers.SetNZ(Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>SEC - Set Carry</para>
        /// <code>
        /// Flags affected (SEC): -------c
        ///
        /// c ← 1
        /// </code>
        /// </summary>
        public void SEC(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Carry = true;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>SED - Set Decimal</para>
        /// <code>
        /// Flags affected (SED): ----d---
        ///
        /// d ← 1
        /// </code>
        /// </summary>
        public void SED(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Decimal = true;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>SEI - Set Interrupt</para>
        /// <code>
        /// Flags affected (SEI): -----i--
        ///
        /// i ← 1
        /// </code>
        /// </summary>
        public void SEI(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Interrupt = true;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>SMB - Set Memory Bit</para>
        /// <code>
        /// Flags affected: n------ ?
        /// </code>
        ///
        /// Clear the specified bit in the zero page memory location
        /// specified in the operand. The bit to clear is specified
        /// by a number (0 through 7) concatenated to the end of the
        /// mnemonic.
        /// </summary>
        public void SMB(ushort addr, byte bit, long cycles, long pageCrossPenalty = 0)
        {
            int flag = 0x01 << bit;
            int value = read(addr);
            value |= flag;
            write(addr, RegisterMath.TruncateToByte(value));

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>STA - Store Accumulator to Memory</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// M ← A
        /// </code>
        /// </summary>
        public void STA(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            write(addr, Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>STX - Store Index Register X to Memory</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// M ← X
        /// </code>
        /// </summary>
        public void STX(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            write(addr, Registers.X);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>STY - Store Index Register Y to Memory</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// M ← Y
        /// </code>
        /// </summary>
        public void STY(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            write(addr, Registers.Y);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>STZ - Store Zero to Memory</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// M ← 0
        /// </code>
        /// </summary>
        public void STZ(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            write(addr, 0);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>TAX - Transfer Accumulator to X</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// X ← A
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </code>
        /// </summary>
        public void TAX(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.X = Registers.A;
            Registers.SetNZ(Registers.X);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>TAY - Transfer Accumulator to Y</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// Y ← A
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </code>
        /// </summary>
        public void TAY(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.Y = Registers.A;
            Registers.SetNZ(Registers.Y);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>TRB - Test and Reset Memory Bits Against Accumulator</para>
        /// <code>
        /// Flags affected: ------z-
        /// </code>
        ///
        /// <para>Logically AND together the complement of the value in the
        /// accumulator with the data at the effective address specified
        /// by the operand. Store the result at the memory location.</para>
        ///
        /// <para>This has the effect of clearing each memory bit for which the
        /// corresponding accumulator bit is set, while leaving unchanged
        /// all memory bits in which the corresponding accumulator bits are zeroes.</para>
        ///
        /// <para>The z zero flag is set based on a second and different operation
        /// the ANDing of the accumulator value (not its complement) with
        /// the memory value (the same way the BIT instruction affects the
        /// zero flag). The result of this second operation is not saved;
        /// only the zero flag is affected by it.</para>
        /// </summary>
        public void TRB(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            int value = read(addr);

            Registers.Zero = RegisterMath.IsZero(Registers.A & value);
            value &= ~Registers.A;
            write(addr, RegisterMath.TruncateToByte(value));

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>TSB - Test and Set Memory Bits Against Accumulator</para>
        /// <code>
        /// Flags affected: ------z-
        /// </code>
        ///
        /// <para>Logically OR together the value in the accumulator with the data
        /// at the effective address specified by the operand. Store the result
        /// at the memory location.</para>
        ///
        /// <para>This has the effect of setting each memory bit for which the
        /// corresponding accumulator bit is set, while leaving unchanged
        /// all memory bits in which the corresponding accumulator bits are
        /// zeroes.</para>
        ///
        /// <para>The z zero flag is set based on a second different operation,
        /// the ANDing of the accumulator value with the memory value (the
        /// same way the BIT instruction affects the zero flag). The result
        /// of this second operation is not saved; only the zero flag is
        /// affected by it.</para>
        /// </summary>
        public void TSB(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            int value = read(addr);
            Registers.Zero = RegisterMath.IsZero(Registers.A & value);

            value |= Registers.A;
            write(addr, RegisterMath.TruncateToByte(value));

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>TSX - Transfer Stack Pointer to X</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// X ← S
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </code>
        /// </summary>
        public void TSX(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.X = Registers.StackPointer;
            Registers.SetNZ(Registers.X);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>TXA - Transfer X to Accumulator</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// A ← X
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </code>
        /// </summary>
        public void TXA(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.A = Registers.X;
            Registers.SetNZ(Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>TXS - Transfer X to Stack Pointer</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// S ← X
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </code>
        /// </summary>
        public void TXS(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.StackPointer = Registers.X;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>TYA - Transfer Y to Accumulator</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// A ← Y
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </code>
        /// </summary>
        public void TYA(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            Registers.A = Registers.Y;
            Registers.SetNZ(Registers.A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// Dummy operator, eats one opcode and increments the PC
        /// </summary>
        public void IllegalInstruction(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            illegalOpCode = true;
        }
        #endregion

        #region InstructionDecoders
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
            OperandDisplay = $"#${read(Registers.ProgramCounter):X2}";

            return Registers.ProgramCounter++;
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
            OperandDisplay = $"${read((ushort)(Registers.ProgramCounter + 1)):X2}{read(Registers.ProgramCounter):X2}";

            byte addrLo = read(Registers.ProgramCounter++);
            byte addrHi = read(Registers.ProgramCounter++);

            return RegisterMath.MakeShort(addrHi, addrLo);
        }

        /// <summary>
        /// ZP - The zero page instructions allow for shorter code and execution
        /// fetch times by fetching only the second byte of the instruction and
        /// assuming a zero high address byte. Careful of use the zero page can
        /// result in significant increase in code efficiency.
        /// </summary>
        public ushort DecodeZeroPage()
        {
            OperandDisplay = $"{{${read(Registers.ProgramCounter):X2}}}";

            return read(Registers.ProgramCounter++);
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
            OperandDisplay = $"{{${read(Registers.ProgramCounter):X2}}},X";

            byte zpAddr = read(Registers.ProgramCounter);
            Registers.ProgramCounter++;

            return (ushort)((zpAddr + Registers.X) & 0x00ff);
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
            OperandDisplay = $"{{${read(Registers.ProgramCounter):X2}}},Y";

            byte zpAddr = read(Registers.ProgramCounter);
            Registers.ProgramCounter++;

            return (ushort)((zpAddr + Registers.Y) & 0x00ff);
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
            OperandDisplay = $"${read((ushort)(Registers.ProgramCounter + 1)):X2}{read(Registers.ProgramCounter):X2},X";

            ushort addrL = read(Registers.ProgramCounter++);
            ushort addrH = read(Registers.ProgramCounter++);

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
            OperandDisplay = $"${read((ushort)(Registers.ProgramCounter + 1)):X2}{read(Registers.ProgramCounter):X2},Y";

            ushort addrL = read(Registers.ProgramCounter++);
            ushort addrH = read(Registers.ProgramCounter++);

            return (ushort)((addrH << 8) + addrL + Registers.Y);
        }

        /// <summary>
        /// (ABS,X) - The contents of the second and third instruction byte are
        /// added to the X register. The sixteen-bit result is a memory address
        /// containing the effective address (JMP (ABS,X) only).
        /// </summary>
        public ushort DecodeIndexedAbsolute()
        {
            OperandDisplay = $"(${read((ushort)(Registers.ProgramCounter + 1)):X2}{read(Registers.ProgramCounter):X2},X)";

            ushort addrL = read(Registers.ProgramCounter++);
            ushort addrH = read(Registers.ProgramCounter++);

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
            OperandDisplay = $"${read(Registers.ProgramCounter):X2}";

            ushort offset = read(Registers.ProgramCounter++);

            if ((offset & 0x80) != 0) { offset |= 0xff00; }

            return (ushort)(Registers.ProgramCounter + offset);
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
            OperandDisplay = $"({{${read(Registers.ProgramCounter):X2}}},X)";

            ushort zeroL = (ushort)((read(Registers.ProgramCounter++) + Registers.X) & 0xff);
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
            OperandDisplay = $"({{${read(Registers.ProgramCounter):X2}}}),Y";

            ushort zeroL = read(Registers.ProgramCounter++);
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
            OperandDisplay = $"${read((ushort)(Registers.ProgramCounter + 1)):X2}{read(Registers.ProgramCounter):X2}";

            ushort addrL = read(Registers.ProgramCounter++);
            ushort addrH = read(Registers.ProgramCounter++);

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
        #endregion
    }
}
