using System;
using System.Text;
using System.Threading.Tasks;
using InnoWerks.Processors;

//
// things to note: http://www.6502.org/tutorials/65c02opcodes.html
//                 https://xotmatrix.github.io/6502/6502-single-cycle-execution.html
//

#pragma warning disable RCS1163, IDE0060, CA1707, CA1822

namespace InnoWerks.Simulators
{
    public abstract class MosTechnologiesCpu : ICpu
    {
        // IRQ, reset, NMI vectors
        public const ushort IrqVectorH = 0xFFFF;
        public const ushort IrqVectorL = 0xFFFE;

        // Reset vectors
        public const ushort RstVectorH = 0xFFFD;
        public const ushort RstVectorL = 0xFFFC;

        // NMI vectors
        public const ushort NmiVectorH = 0xFFFB;
        public const ushort NmiVectorL = 0xFFFA;

        public const ushort StackBase = 0x0100;

        public Registers Registers { get; private set; }

        public string OperandDisplay { get; private set; }

        public abstract CpuClass CpuClass { get; }

        protected IBus bus { get; init; }

        protected Action<ICpu, ushort> preExecutionCallback { get; init; }

        protected Action<ICpu> postExecutionCallback { get; init; }

        protected bool illegalInstructionEncountered { get; set; }

        protected MosTechnologiesCpu(IBus bus,
                                     Action<ICpu, ushort> preExecutionCallback,
                                     Action<ICpu> postExecutionCallback)
        {
            this.bus = bus;

            Registers = new();

            this.preExecutionCallback = preExecutionCallback;
            this.postExecutionCallback = postExecutionCallback;

            Registers.Reset();
        }

        protected abstract void Dispatch(byte operation, bool writeInstructions = false);

        protected abstract OpCodeDefinition GetOpCodeDefinition(byte operation);

        public void Reset()
        {
            Registers.Reset();

            // this needs to reset the soft switches via the bus
            bus.Reset();

            // load PC from reset vector
            byte pcl = bus.Peek(RstVectorL);
            byte pch = bus.Peek(RstVectorH);

            Registers.ProgramCounter = RegisterMath.MakeShort(pch, pcl);
        }

        public void NMI()
        {
            StackPushWord(Registers.ProgramCounter);
            StackPush((byte)((Registers.ProcessorStatus & 0xef) | (byte)ProcessorStatusBit.Unused));

            // 65c02 clears the decimal flag, 6502 leaves is undefined
            if (CpuClass == CpuClass.WDC65C02)
            {
                Registers.Decimal = false;
            }

            Registers.Interrupt = true;

            // load PC from reset vector
            byte pcl = bus.Read(NmiVectorL);
            byte pch = bus.Read(NmiVectorH);

            Registers.ProgramCounter = RegisterMath.MakeShort(pch, pcl);
        }

        public void IRQ()
        {
            if (Registers.Interrupt == true)
            {
                NMI();
            }

            // todo: finish this case to support apple iie ref p. 151
        }

        public void PrintStatus()
        {
            Console.Write($"PC:{Registers.ProgramCounter:X4} {Registers.GetRegisterDisplay} ");
            Console.WriteLine(Registers.GetFlagsDisplay);
        }

        public void StackPush(byte b)
        {
            bus.Write((ushort)(StackBase + Registers.StackPointer), b);

            Registers.StackPointer = (byte)((Registers.StackPointer - 1) & 0xff);
        }

        public byte StackPop()
        {
            Registers.StackPointer = (byte)((Registers.StackPointer + 1) & 0xff);

            return bus.Read((ushort)(StackBase + Registers.StackPointer));
        }

        public void StackPushWord(ushort s)
        {
            StackPush((byte)(s >> 8));
            StackPush((byte)(s & 0xff));
        }

        public ushort StackPopWord()
        {
            return (ushort)((0xff & StackPop()) | (0xff00 & (StackPop() << 8)));
        }

        public ushort PeekWord(ushort address)
        {
            var lo = bus.Peek(address);
            var hi = bus.Peek((ushort)(address + 1));

            return (ushort)((hi << 8) | lo);
        }

        public ushort ReadWord(ushort address)
        {
            var lo = bus.Read(address);
            var hi = bus.Read((ushort)(address + 1));

            return (ushort)((hi << 8) | lo);
        }

        public void WriteWord(ushort address, ushort value)
        {
            bus.Write(address, (byte)(value & 0x00ff));
            bus.Write((ushort)(address + 1), (byte)((value >> 8) & 0xff));
        }

        public (int intructionCount, int cycleCount) Run(bool stopOnBreak = false, bool writeInstructions = false, int stepsPerSecond = 0)
        {
            var instructionCount = 0;

            bus.BeginTransaction();

            while (true)
            {
                instructionCount++;

                preExecutionCallback?.Invoke(this, Registers.ProgramCounter);

                // T0
                var operation = bus.Read(Registers.ProgramCounter);
                // this is a bad hard-coding right now...
                if (operation == 0x00)
                {
                    // BRK
                    break;
                }

                Dispatch(operation, writeInstructions);

                if (writeInstructions)
                {
                    Console.Error.WriteLine($"  {Registers.GetRegisterDisplay}   {Registers.InternalGetFlagsDisplay,-8}");
                }

                postExecutionCallback?.Invoke(this);

                if (stepsPerSecond > 0)
                {
                    var t = Task.Run(async delegate
                                {
                                    await Task.Delay(new TimeSpan((long)(1.0 / stepsPerSecond * 1000) * TimeSpan.TicksPerMillisecond));
                                    return 0;
                                });
                    t.Wait();
                }
            }

            int cycleCount = bus.EndTransaction();

            return (instructionCount, cycleCount);
        }

        public int Step(bool writeInstructions = false, bool returnPriorToBreak = false)
        {
            bus.BeginTransaction();

            // for debugging we're just going to peek at the next
            // instruction, and if it's both a BRK and we've been asked
            // to NOT execute, then we'll return
            var operation = bus.Peek(Registers.ProgramCounter);
            if (operation == 0x00 && returnPriorToBreak == true)
            {
                return 0;
            }

            preExecutionCallback?.Invoke(this, Registers.ProgramCounter);

            // T0
            operation = bus.Read(Registers.ProgramCounter);

            // rest of memory cycles
            Dispatch(operation, writeInstructions);

            postExecutionCallback?.Invoke(this);

            // hard-coded BRK, if hit we'll tell the caller that we saw and executed a break
            return bus.EndTransaction();
        }

        public (OpCodeDefinition opCodeDefinition, string decode) PeekInstruction()
        {
            var operation = bus.Peek(Registers.ProgramCounter);

            OpCodeDefinition opCodeDefinition =
                CpuInstructions.OpCode65C02[operation];

            // decode the operand based on the opcode and addressing mode
            if (opCodeDefinition.DecodeOperand(this) == false)
            {
                if (illegalInstructionEncountered == true)
                {
                    // This is a JAM / KIL
                    throw new IllegalOpCodeException(Registers.ProgramCounter, operation);
                }

                return (null, $"Invalid operation {operation:X2}");
            }

            var stepToExecute = $"{Registers.ProgramCounter:X4} {opCodeDefinition.OpCode}   {OperandDisplay,-10}";

            return (opCodeDefinition, stepToExecute);
        }

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
        public void ADC65C02(ushort addr, byte value)
        {
            int adjustment = Registers.Carry ? 0x01 : 0x00;
            int w;

            Registers.Overflow = ((Registers.A ^ value) & 0x80) == 0;

            if (Registers.Decimal == true)
            {
                w = (Registers.A & 0x0f) + (value & 0x0f) + adjustment;
                if (w >= 0x0a)
                {
                    w = 0x10 | ((w + 0x06) & 0x0f);
                }
                w += (Registers.A & 0xf0) + (value & 0xf0);
                if (w >= 0xa0)
                {
                    Registers.Carry = true;
                    Registers.Overflow &= w < 0x180;
                    w += 0x60;
                }
                else
                {
                    Registers.Carry = false;
                    Registers.Overflow &= w >= 0x80;
                }
            }
            else
            {
                w = Registers.A + value + adjustment;
                Registers.Overflow = ((Registers.A ^ w) & (value ^ w) & 0x80) != 0;
                Registers.Carry = w >= 0x100;
            }

            Registers.A = RegisterMath.TruncateToByte(w);
            Registers.SetNZ(Registers.A);
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
        public void ADC6502(ushort addr, byte value)
        {
            int adjustment = Registers.Carry ? 0x01 : 0x00;

            if (Registers.Decimal == true)
            {
                int w = (Registers.A & 0x0f) + (value & 0x0f) + adjustment;
                if (w > 0x09)
                {
                    w += 0x06;
                }
                if (w <= 0x0f)
                {
                    w = (w & 0x0f) + (Registers.A & 0xf0) + (value & 0xf0);
                }
                else
                {
                    w = (w & 0x0f) + (Registers.A & 0xf0) + (value & 0xf0) + 0x10;
                }

                Registers.Zero = RegisterMath.IsZero((Registers.A + value + adjustment) & 0xff);
                Registers.Negative = RegisterMath.IsHighBitSet(w);
                Registers.Overflow = ((Registers.A ^ w) & 0x80) != 0 && ((Registers.A ^ value) & 0x80) == 0;

                if ((w & 0x1f0) > 0x90)
                {
                    w += 0x60;
                }

                Registers.Carry = (w & 0xff0) > 0xf0;
                Registers.A = RegisterMath.TruncateToByte(w);
            }
            else
            {
                int w = Registers.A + value + adjustment;

                Registers.Carry = w > 0xff;
                Registers.Overflow = ((Registers.A & 0x80) == (value & 0x80)) && ((Registers.A & 0x80) != (w & 0x80));
                Registers.A = RegisterMath.TruncateToByte(w);
                Registers.SetNZ(Registers.A);
            }
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
        public void AND(ushort addr, byte value)
        {
            value &= Registers.A;

            Registers.A = RegisterMath.TruncateToByte(value);
            Registers.SetNZ(Registers.A);
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
        public void ASL(ushort addr, byte value)
        {
            Registers.Carry = ((byte)(value & 0x80)) != 0x00;
            value = RegisterMath.TruncateToByte(0xfe & (value << 1));
            Registers.SetNZ(value);

            // todo: deal with http://forum.6502.org/viewtopic.php?f=4&t=1617&view=previous
            bus.Write(addr, value);
        }

        public void ASL_A(ushort addr, byte value)
        {
            Registers.Carry = ((Registers.A >> 7) & 0x01) != 0;
            Registers.A = RegisterMath.TruncateToByte(0xfe & (Registers.A << 1));
            Registers.SetNZ(Registers.A);
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
        public void BBR(ushort _, byte value, byte bit)
        {
            // T3
            var offset = bus.Read((ushort)(Registers.ProgramCounter + 2));

            // T2 - T3
            var addr = (ushort)(Registers.ProgramCounter + 3 + ((sbyte)offset < 0 ? (sbyte)offset : offset));

            DoBranch65C02((value & (0x01 << bit)) == 0, addr, 0);
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
        public void BBS(ushort _, byte value, byte bit)
        {
            // T3
            var offset = bus.Read((ushort)(Registers.ProgramCounter + 2));

            // T2 - T3
            var addr = (ushort)(Registers.ProgramCounter + 3 + ((sbyte)offset < 0 ? (sbyte)offset : offset));

            DoBranch65C02((value & (0x01 << bit)) != 0, addr, 0);
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
        public void BCC(ushort addr, byte value)
        {
            DoBranch(Registers.Carry == false, addr, value);
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
        public void BCS(ushort addr, byte value)
        {
            DoBranch(Registers.Carry == true, addr, value);
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
        public void BEQ(ushort addr, byte value)
        {
            DoBranch(Registers.Zero == true, addr, value);
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
        public void BIT(ushort addr, byte value, bool immediateMode)
        {
            int result = Registers.A & value;
            Registers.Zero = RegisterMath.IsZero(result);

            if (immediateMode == false)
            {
                Registers.Negative = ((byte)(value & 0x80)) != 0x00;
                Registers.Overflow = ((byte)(value & 0x40)) != 0x00;
            }
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
        public void BMI(ushort addr, byte value)
        {
            DoBranch(Registers.Negative == true, addr, value);
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
        public void BNE(ushort addr, byte value)
        {
            DoBranch(Registers.Zero == false, addr, value);
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
        public void BPL(ushort addr, byte value)
        {
            DoBranch(Registers.Negative == false, addr, value);
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
        public void BRA(ushort addr, byte value)
        {
            DoBranch(true, addr, value);
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
        public void BRK(ushort addr, byte _2)
        {
            // 65c02 clears the decimal flag, 6502 leaves is undefined
            if (CpuClass == CpuClass.WDC65C02)
            {
                Registers.Decimal = false;
            }

            Registers.Interrupt = true;
            Registers.ProgramCounter = addr;
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
        public void BVC(ushort addr, byte value)
        {
            DoBranch(Registers.Overflow == false, addr, value);
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
        public void BVS(ushort addr, byte value)
        {
            DoBranch(Registers.Overflow == true, addr, value);
        }

        /// <summary>
        /// <para>CLC - Clear Carry</para>
        /// <code>
        /// Flags affected: -------c
        ///
        /// c ← 0
        /// </code>
        /// </summary>
        public void CLC(ushort _1, byte _2)
        {
            Registers.Carry = false;
        }

        /// <summary>
        /// <para>CLC - Clear Decimal</para>
        /// <code>
        /// Flags affected: ----d---
        ///
        /// d ← 0
        /// </code>
        /// </summary>
        public void CLD(ushort _1, byte _2)
        {
            Registers.Decimal = false;
        }

        /// <summary>
        /// <para>CLC - Clear Interrupt</para>
        /// <code>
        /// Flags affected: -----i--
        ///
        /// i ← 0
        /// </code>
        /// </summary>
        public void CLI(ushort _1, byte _2)
        {
            Registers.Interrupt = false;
        }

        /// <summary>
        /// <para>CLV - Clear Overflow</para>
        /// <code>
        /// Flags affected: -v------
        ///
        /// v ← 0
        /// </code>
        /// </summary>
        public void CLV(ushort _1, byte _2)
        {
            Registers.Overflow = false;
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
        public void CMP(ushort addr, byte value)
        {
            int val = Registers.A - value;

            Registers.Carry = Registers.A >= value;
            Registers.SetNZ(val);
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
        public void CPX(ushort addr, byte value)
        {
            int val = Registers.X - value;

            Registers.Carry = Registers.X >= value;
            Registers.SetNZ(val);
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
        public void CPY(ushort addr, byte value)
        {
            int val = Registers.Y - value;

            Registers.Carry = Registers.Y >= value;
            Registers.SetNZ(val);
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
        public void DEC(ushort addr, byte value)
        {
            value = RegisterMath.Dec(value);
            bus.Write(addr, value);

            Registers.SetNZ(value);
        }

        /// <summary>
        /// <para>DEA - Decrement Accumulator</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// A ← A - 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void DEA(ushort _1, byte _2)
        {
            Registers.A = RegisterMath.Dec(Registers.A);
            Registers.SetNZ(Registers.A);
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
        public void DEX(ushort _1, byte _2)
        {
            Registers.X = RegisterMath.Dec(Registers.X);
            Registers.SetNZ(Registers.X);
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
        public void DEY(ushort _1, byte _2)
        {
            Registers.Y = RegisterMath.Dec(Registers.Y);
            Registers.SetNZ(Registers.Y);
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
        public void EOR(ushort addr, byte value)
        {
            value ^= Registers.A;

            Registers.A = RegisterMath.TruncateToByte(value);
            Registers.SetNZ(Registers.A);
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
        public void INC(ushort addr, byte value)
        {
            byte val = RegisterMath.Inc(value);
            Registers.SetNZ(val);

            bus.Write(addr, val);
        }

        /// <summary>
        /// <para>INX - Increment Accumulator</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// A ← A + 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void INA(ushort _1, byte _2)
        {
            Registers.A = RegisterMath.Inc(Registers.A);
            Registers.SetNZ(Registers.A);
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
        public void INX(ushort _1, byte _2)
        {
            Registers.X = RegisterMath.Inc(Registers.X);
            Registers.SetNZ(Registers.X);
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
        public void INY(ushort _1, byte _2)
        {
            Registers.Y = RegisterMath.Inc(Registers.Y);
            Registers.SetNZ(Registers.Y);
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
        public void JMP(ushort addr, byte value)
        {
            Registers.ProgramCounter = addr;
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
        public void JSR(ushort addr, byte value)
        {
            Registers.ProgramCounter = addr;
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
        public void LDA(ushort addr, byte value)
        {
            Registers.A = value;
            Registers.SetNZ(Registers.A);
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
        public void LDX(ushort addr, byte value)
        {
            Registers.X = value;
            Registers.SetNZ(Registers.X);
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
        public void LDY(ushort addr, byte value)
        {
            Registers.Y = value;
            Registers.SetNZ(Registers.Y);
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
        public void LSR(ushort addr, byte value)
        {
            Registers.Carry = (value & 0x01) != 0;
            Registers.Negative = false;

            value >>= 1;
            Registers.Zero = RegisterMath.IsZero(value);

            bus.Write(addr, value);
        }

        public void LSR_A(ushort addr, byte value)
        {
            Registers.Carry = (Registers.A & 0x01) != 0;
            Registers.Negative = false;

            Registers.A >>= 1;
            Registers.Zero = RegisterMath.IsZero(Registers.A);
        }

        /// <summary>
        /// <para>NOP - No Operation</para>
        /// <code>
        /// Flags affected: --------
        /// </code>
        /// </summary>
        public void NOP(ushort _1, byte _2)
        {
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
        public void ORA(ushort addr, byte value)
        {
            value |= Registers.A;

            Registers.A = RegisterMath.TruncateToByte(value);
            Registers.SetNZ(Registers.A);
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
        public void PHA(ushort _1, byte _2)
        {
            StackPush(Registers.A);
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
        public void PHP(ushort _1, byte _2)
        {
            StackPush((byte)(Registers.ProcessorStatus | (byte)ProcessorStatusBit.BreakCommand));
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
        public void PHX(ushort _1, byte _2)
        {
            StackPush(Registers.X);
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
        public void PHY(ushort _1, byte _2)
        {
            StackPush(Registers.Y);
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
        public void PLA(ushort _1, byte _2)
        {
            Registers.A = StackPop();
            Registers.SetNZ(Registers.A);
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
        public void PLP(ushort _1, byte _2)
        {
            Registers.ProcessorStatus = StackPop();
            Registers.Break = false;
            Registers.Unused = true;
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
        public void PLX(ushort _1, byte _2)
        {
            Registers.X = StackPop();
            Registers.SetNZ(Registers.X);
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
        public void PLY(ushort _1, byte _2)
        {
            Registers.Y = StackPop();
            Registers.SetNZ(Registers.Y);
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
        public void RMB(ushort addr, byte value, byte bit)
        {
            int flag = 0x01 << bit;
            value &= (byte)~flag;

            bus.Write(addr, value);
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
        public void ROL(ushort addr, byte value)
        {
            var adjustment = Registers.Carry ? 0x01 : 0x00;
            Registers.Carry = RegisterMath.IsHighBitSet(value);
            value = RegisterMath.TruncateToByte((value << 1) | adjustment);
            Registers.SetNZ(value);

            bus.Write(addr, value);
        }

        public void ROL_A(ushort addr, byte value)
        {
            var adjustment = Registers.Carry ? 0x01 : 0x00;
            Registers.Carry = (Registers.A >> 7) != 0;
            Registers.A = RegisterMath.TruncateToByte((Registers.A << 1) | adjustment);
            Registers.SetNZ(Registers.A);
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
        public void ROR(ushort addr, byte value)
        {
            var adjustment = (Registers.Carry ? 0x01 : 0x00) << 7;
            Registers.Carry = (value & 1) != 0;
            value = RegisterMath.TruncateToByte((value >> 1) | adjustment);
            Registers.SetNZ(value);

            bus.Write(addr, value);
        }

        public void ROR_A(ushort addr, byte value)
        {
            var adjustment = (Registers.Carry ? 0x01 : 0x00) << 7;
            Registers.Carry = (Registers.A & 1) != 0;
            Registers.A = RegisterMath.TruncateToByte((Registers.A >> 1) | adjustment);
            Registers.SetNZ(Registers.A);
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
        public void RTI(ushort _1, byte _2)
        {
            // T3
            Registers.ProcessorStatus = StackPop();
            Registers.Break = false;
            Registers.Unused = true;

            // T4 - T5
            Registers.ProgramCounter = StackPopWord();
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
        public void RTS(ushort _1, byte _2)
        {
            // T3
            var pcl = StackPop();

            // T4
            var pch = StackPop();

            // T5
            bus.Read((ushort)((pch << 8) | pcl));

            Registers.ProgramCounter = (ushort)(((pch << 8) | pcl) + 1);
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
        public void SBC6502(ushort addr, byte value)
        {
            int adjustment = Registers.Carry ? 0x00 : 0x01;
            int result = Registers.A - value - adjustment;

            bool borrowNeeded = false;
            if (result < 0)
            {
                borrowNeeded = true;
            }

            if (Registers.Decimal == true)
            {
                int val = (Registers.A & 0x0f) - (value & 0x0f) - adjustment;
                if ((val & 0x10) != 0)
                {
                    val = ((val - 0x06) & 0x0f) | ((Registers.A & 0xf0) - (value & 0xf0) - 0x10);
                }
                else
                {
                    val = (val & 0x0f) | ((Registers.A & 0xf0) - (value & 0xf0));
                }
                if ((val & 0x100) != 0)
                {
                    val -= 0x60;
                }

                // Registers.Carry = result < 0x100;
                Registers.Carry = !borrowNeeded;
                Registers.SetNZ(result);
                Registers.Overflow = ((Registers.A ^ result) & 0x80) != 0 && ((Registers.A ^ value) & 0x80) != 0;
                Registers.A = RegisterMath.TruncateToByte(val);
            }
            else
            {
                // Registers.Carry = result < 0x100;
                Registers.Carry = !borrowNeeded;
                Registers.Overflow = ((Registers.A & 0x80) != (value & 0x80)) && ((Registers.A & 0x80) != (result & 0x80));
                Registers.A = RegisterMath.TruncateToByte(result);
                Registers.SetNZ(Registers.A);
            }
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
        public void SBC65C02(ushort addr, byte value)
        {
            if (Registers.Decimal == false)
            {
                ADC65C02(addr, RegisterMath.TruncateToByte(~value));
            }
            else
            {
                int adjustment = Registers.Carry ? 0x01 : 0x00;
                Registers.Overflow = ((Registers.A ^ value) & 0x80) != 0;

                int w = 0x0f + (Registers.A & 0x0f) - (value & 0x0f) + adjustment;
                int val = 0;

                if (w < 0x10)
                {
                    w -= 0x06;
                }
                else
                {
                    val = 0x10;
                    w -= 0x10;
                }

                val += 0xf0 + (Registers.A & 0xf0) - (value & 0xf0);

                if (val < 0x100)
                {
                    Registers.Carry = false;
                    Registers.Overflow &= val >= 0x80;
                    val -= 0x60;
                }
                else
                {
                    Registers.Carry = true;
                    Registers.Overflow &= val < 0x180;
                }

                val += w;
                Registers.A = RegisterMath.TruncateToByte(val);
                Registers.SetNZ(Registers.A);
            }
        }

        /// <summary>
        /// <para>SEC - Set Carry</para>
        /// <code>
        /// Flags affected (SEC): -------c
        ///
        /// c ← 1
        /// </code>
        /// </summary>
        public void SEC(ushort _1, byte _2)
        {
            Registers.Carry = true;
        }

        /// <summary>
        /// <para>SED - Set Decimal</para>
        /// <code>
        /// Flags affected (SED): ----d---
        ///
        /// d ← 1
        /// </code>
        /// </summary>
        public void SED(ushort _1, byte _2)
        {
            Registers.Decimal = true;
        }

        /// <summary>
        /// <para>SEI - Set Interrupt</para>
        /// <code>
        /// Flags affected (SEI): -----i--
        ///
        /// i ← 1
        /// </code>
        /// </summary>
        public void SEI(ushort _1, byte _2)
        {
            Registers.Interrupt = true;
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
        public void SMB(ushort addr, byte value, byte bit)
        {
            int flag = 0x01 << bit;
            value |= (byte)flag;

            bus.Write(addr, value);
        }

        /// <summary>
        /// <para>STA - Store Accumulator to Memory</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// M ← A
        /// </code>
        /// </summary>
        public void STA(ushort addr, byte value)
        {
            bus.Write(addr, Registers.A);
        }

        /// <summary>
        /// <para>STX - Store Index Register X to Memory</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// M ← X
        /// </code>
        /// </summary>
        public void STX(ushort addr, byte value)
        {
            bus.Write(addr, Registers.X);
        }

        /// <summary>
        /// <para>STY - Store Index Register Y to Memory</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// M ← Y
        /// </code>
        /// </summary>
        public void STY(ushort addr, byte value)
        {
            bus.Write(addr, Registers.Y);
        }

        /// <summary>
        /// <para>STZ - Store Zero to Memory</para>
        /// <code>
        /// Flags affected: --------
        ///
        /// M ← 0
        /// </code>
        /// </summary>
        public void STZ(ushort addr, byte value)
        {
            bus.Write(addr, 0);
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
        public void TAX(ushort _1, byte _2)
        {
            Registers.X = Registers.A;
            Registers.SetNZ(Registers.X);
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
        public void TAY(ushort _1, byte _2)
        {
            Registers.Y = Registers.A;
            Registers.SetNZ(Registers.Y);
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
        public void TRB(ushort addr, byte value)
        {
            Registers.Zero = RegisterMath.IsZero(Registers.A & value);
            value &= (byte)~Registers.A;

            bus.Write(addr, RegisterMath.TruncateToByte(value));
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
        public void TSB(ushort addr, byte value)
        {
            Registers.Zero = RegisterMath.IsZero(Registers.A & value);
            value |= Registers.A;

            bus.Write(addr, RegisterMath.TruncateToByte(value));
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
        public void TSX(ushort _1, byte _2)
        {
            Registers.X = Registers.StackPointer;
            Registers.SetNZ(Registers.X);
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
        public void TXA(ushort _1, byte _2)
        {
            Registers.A = Registers.X;
            Registers.SetNZ(Registers.A);
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
        public void TXS(ushort _1, byte _2)
        {
            Registers.StackPointer = Registers.X;
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
        public void TYA(ushort _1, byte _2)
        {
            Registers.A = Registers.Y;
            Registers.SetNZ(Registers.A);
        }

        /// <summary>
        /// Dummy operator, eats one opcode and increments the PC
        /// </summary>
        public void IllegalInstruction(ushort _1, byte _2)
        {
        }

        public void WAI(ushort _1, byte _2)
        {
            throw new NotImplementedException();
        }

        public void STP(ushort _1, byte _2)
        {
            throw new NotImplementedException();
        }

        private void DoBranch(bool condition, ushort addr, byte offset)
        {
            ushort next = 2;
            ushort pc = (ushort)(Registers.ProgramCounter + next);

            if (condition == false)
            {
                Registers.ProgramCounter += next;
            }
            else
            {
                /* discarded */
                bus.Read(pc);

                if ((addr & 0xff00) != (pc & 0xff00))
                {
                    var lo = addr & 0x00ff;
                    var hi = ((sbyte)offset < 0) ? ((addr & 0xff00) >> 8) + 1 : ((addr & 0xff00) >> 8) - 1;

                    /* discarded */
                    bus.Read((ushort)((hi << 8) | lo));
                }

                Registers.ProgramCounter = addr;
            }
        }

        private void DoBranch65C02(bool condition, ushort addr, byte offset)
        {
            ushort next = 3;
            ushort pc = (ushort)(Registers.ProgramCounter + next);

            if (condition == false)
            {
                Registers.ProgramCounter += next;
            }
            else
            {
                /* discarded */
                bus.Read(pc);

                if ((addr & 0xff00) != (pc & 0xff00))
                {
                    bus.Read(pc);
                }

                Registers.ProgramCounter = addr;
            }
        }
        #endregion

        #region InstructionDecoders
        public bool DecodeUndefined(int bytes, int cycles)
        {
            OperandDisplay = "<illegal>";

            if (bytes == 0)
            {
                illegalInstructionEncountered = true;
                return false;
            }

            // all we can do is move the PC
            Registers.ProgramCounter = (ushort)(Registers.ProgramCounter + bytes);
            return false;
        }

        /// <summary>
        /// Implied - In the implied addressing mode, the address containing
        /// the operand is implicitly stated in the operation code of the instruction.
        /// </summary>
        public bool DecodeImplicit()
        {
            OperandDisplay = "";
            return true;
        }

        /// <summary>
        /// Implied - In the implied addressing mode, the address containing
        /// the operand is implicitly stated in the operation code of the instruction.
        /// </summary>
        public bool DecodeStack()
        {
            OperandDisplay = "";
            return true;
        }

        /// <summary>
        /// Accum - This form of addressing is represented with a
        /// one byte instruction, implying an operation on the accumulator.
        /// </summary>
        public bool DecodeAccumulator()
        {
            OperandDisplay = "";
            return true;
        }

        /// <summary>
        /// IMM - In immediate addressing, the second byte of the instruction
        /// contains the operand, with no further memory addressing required.
        /// </summary>
        public bool DecodeImmediate()
        {
            OperandDisplay = $"#${bus.Peek((ushort)(Registers.ProgramCounter + 1)):X2}";
            return true;
        }

        /// <summary>
        /// ABS - In absolute addressing, the second byte of the instruction
        /// specifies the eight low order bits of the effective address while
        /// the third byte specifies the eight high order bits. Thus the
        /// absolute addressing mode allows access to the entire 64k bytes
        /// of addressable memory.
        /// </summary>
        public bool DecodeAbsolute()
        {
            OperandDisplay = $"${PeekWord((ushort)(Registers.ProgramCounter + 1)):X4}";
            return true;
        }

        /// <summary>
        /// ZP - The zero page instructions allow for shorter code and execution
        /// fetch times by fetching only the second byte of the instruction and
        /// assuming a zero high address byte. Careful of use the zero page can
        /// result in significant increase in code efficiency.
        /// </summary>
        public bool DecodeZeroPage()
        {
            OperandDisplay = $"${bus.Peek((ushort)(Registers.ProgramCounter + 1)):X2}";
            return true;
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
        public bool DecodeAbsoluteXIndexed()
        {
            OperandDisplay = $"${PeekWord((ushort)(Registers.ProgramCounter + 1)):X4},X";
            return true;
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
        public bool DecodeAbsoluteYIndexed()
        {
            OperandDisplay = $"${PeekWord((ushort)(Registers.ProgramCounter + 1)):X4},Y";
            return true;
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
        public bool DecodeZeroPageXIndexed()
        {
            OperandDisplay = $"(${bus.Peek((ushort)(Registers.ProgramCounter + 1)):X2},X)";
            return true;
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
        public bool DecodeZeroPageYIndexed()
        {
            OperandDisplay = $"(${bus.Peek((ushort)(Registers.ProgramCounter + 1)):X2},Y";
            return true;
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
        public bool DecodeRelative()
        {
            OperandDisplay = $"${bus.Peek((ushort)(Registers.ProgramCounter + 1)):X2}";
            return true;
        }

        /// <summary>
        /// (IND) - The second byte of the instruction contains a zero page address
        /// serving as the indirect pointer.
        /// </summary>
        public bool DecodeZeroPageIndirect()
        {
            OperandDisplay = $"(${bus.Peek((ushort)(Registers.ProgramCounter + 1)):X2})";
            return true;
        }

        /// <summary>
        /// (ABS,X) - The contents of the second and third instruction byte are
        /// added to the X register. The sixteen-bit result is a memory address
        /// containing the effective address (JMP (ABS,X) only).
        /// </summary>
        public bool DecodeAbsoluteIndexedIndirect()
        {
            OperandDisplay = $"(${PeekWord((ushort)(Registers.ProgramCounter + 1)):X4},X)";
            return true;
        }

        /// <summary>
        /// (IND,X) - In indexed indirect addressing (referred to as (Indirect,X)),
        /// the second byte of the instruction is added to the contents of the X
        /// register, discarding the carry. The result of this addition points to a
        /// memory location on page zero whose contents are the low order eight bits
        /// of the effective address. The next memory location in page zero contains
        /// the high order eight bits of the effective address. Both memory locations
        /// specifying the high and low order bytes of the effective address
        /// must be in page zero.
        /// </summary>
        public bool DecodeXIndexedIndirect()
        {
            OperandDisplay = $"(${bus.Peek((ushort)(Registers.ProgramCounter + 1)):X2},X)";
            return true;
        }

        /// <summary>
        /// (IND),Y - In indirect indexed addressing (referred to as (Indirect),Y), the
        /// second byte of the instruction points to a memory location in page zero. The
        /// contents of this memory location are added to the contents of the Y index
        /// register, the result being the low order eight bits of the effective address.
        /// The carry from this addition is added to the contents of the next page
        /// zero memory location, the result being the high order eight bits
        /// of the effective address.
        /// </summary>
        public bool DecodeIndirectYIndexed()
        {
            OperandDisplay = $"(${bus.Peek((ushort)(Registers.ProgramCounter + 1)):X2}),Y";
            return true;
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
        public bool DecodeAbsoluteIndirect()
        {
            OperandDisplay = $"(${PeekWord((ushort)(Registers.ProgramCounter + 1)):X4})";
            return true;
        }
        #endregion
    }

}
