namespace InnoWerks.Simulators
{
    public partial class Cpu
    {
        /// <summary>
        /// <para>ADC - Add with Carry</para>
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
        public void ADC(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            byte m = read(addr);
            int tmp = m + A + (IF_CARRY() ? 1 : 0);
            SET_ZERO_FROM_VALUE(tmp & 0xff);
            if (IF_DECIMAL())
            {
                cycles++;

                if (((A & 0xF) + (m & 0xF) + (IF_CARRY() ? 1 : 0)) > 9)
                    tmp += 6;
                SET_NEGATIVE_FROM_VALUE(tmp);
                SET_OVERFLOW_FROM_A(m, tmp);
                if (tmp > 0x99)
                {
                    tmp += 96;
                }
                SET_CARRY(tmp > 0x99);
            }
            else
            {
                SET_NEGATIVE_FROM_VALUE(tmp);
                SET_OVERFLOW_FROM_A(m, tmp);
                SET_CARRY(tmp > 0xff);
            }

            A = (byte)(tmp & 0xff);

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
            byte m = read(addr);
            byte res = (byte)(m & A);
            SET_NEGATIVE_FROM_VALUE(res);
            SET_ZERO_FROM_VALUE(res);
            A = res;

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
        public void ASL(ushort addr, bool accum, long cycles, long pageCrossPenalty = 0)
        {
            byte m = accum ? A : read(addr);
            SET_CARRY_ON_SHIFT(m);
            m <<= 1;
            m &= 0xff;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            if (accum == true)
            {
                A = m;
            }
            else
            {
                write(addr, m);
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
        /// The specified bit in the zero page location specified in the
        /// operand is tested. If it is clear (reset), a branch is taken; if it is
        /// set, the instruction immediately following the two-byte BBRx instruction
        /// is executed. The bit is specified by a number (0 through 7)
        /// concatenated to the end of the mnemonic.
        ///
        /// If the branch is performed, the third byte of the instruction is used
        /// as a signed displacement from the program counter; that is, it is added
        /// to the program counter: a positive value(numbers less than or equal to
        /// $80; that is, numbers with the high-order bit clear) results in a branch
        /// to a higher location; a negative value(greater than $80, with the
        /// high-order bit set) results in a branch to a lower location.Once the branch
        /// address is calculated, the result is loaded into the program counter,
        /// transferring control to that location.
        /// </summary>
        public void BBR(ushort addr, byte bit, long cycles, long pageCrossPenalty = 0)
        {
            if ((addr & (0x01 << bit)) == 0)
            {
                ushort savePC = ProgramCounter;

                sbyte offset = (sbyte)read(ProgramCounter++);
                ProgramCounter = (ushort)(ProgramCounter + offset);

                if ((savePC & 0xff00) != (ProgramCounter & 0xff00))
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
        /// The specified bit in the zero page location specified in the
        /// operand is tested. If it is set, a branch is taken; if it is
        /// clear (reset), the instructions immediately following the
        /// two-byte BBSx instruction is executed. The bit is specified
        /// by a number (0 through 7) concatenated to the end of the mnemonic.
        ///
        /// If the branch is performed, the third byte of the instruction
        /// is used as a signed displacement from the program counter; that
        /// is, it is added to the program counter: a positive value (numbers
        /// less than or equal to $80; that is, numbers with the high order
        /// bit clear) results in a branch to a higher location; a negative
        /// value (greater than $80, with the high- order bit set) results in
        /// a branch to a lower location. Once the branch address is calculated,
        /// the result is loaded into the program counter, transferring control
        /// to that location.
        /// </summary>
        public void BBS(ushort addr, byte bit, long cycles, long pageCrossPenalty = 0)
        {
            if ((addr & (0x01 << bit)) != 0)
            {
                ushort savePC = ProgramCounter;

                sbyte offset = (sbyte)read(ProgramCounter++);
                ProgramCounter = (ushort)(ProgramCounter + offset);

                if ((savePC & 0xff00) != (ProgramCounter & 0xff00))
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
            if (IF_CARRY() == false)
            {
                if ((addr & 0xff00) != (ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                ProgramCounter = addr;
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
            if (IF_CARRY())
            {
                if ((addr & 0xff00) != (ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                ProgramCounter = addr;
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
            if (IF_ZERO())
            {
                if ((addr & 0xff00) != (ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                ProgramCounter = addr;
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
        public void BIT(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            byte m = read(addr);

            // TODO: pass flag indicating immediate mode
            SET_NEGATIVE_FROM_VALUE(m);
            SET_OVERFLOW_FROM_VALUE(m);

            byte res = (byte)(m & A);
            SET_ZERO_FROM_VALUE(res);

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
        ///
        /// Branch taken (BRL):
        /// PC ← PC + label
        /// </code>
        /// </summary>
        public void BMI(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if (IF_NEGATIVE())
            {
                if ((addr & 0xff00) != (ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                ProgramCounter = addr;
            }

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>BNE - Branch on Negative</para>
        /// <code>
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        ///
        /// Branch taken (BRL):
        /// PC ← PC + label
        /// </code>
        /// </summary>
        public void BNE(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            if (IF_ZERO() == false)
            {
                if ((addr & 0xff00) != (ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                ProgramCounter = addr;
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
            if (IF_NEGATIVE() == false)
            {
                if ((addr & 0xff00) != (ProgramCounter & 0xff00))
                {
                    cycles += pageCrossPenalty;
                }

                ProgramCounter = addr;
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
            if ((addr & 0xff00) != (ProgramCounter & 0xff00))
            {
                cycles += pageCrossPenalty;
            }

            ProgramCounter = addr;

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
            if ((addr & 0xff00) != (ProgramCounter & 0xff00))
            {
                cycles += pageCrossPenalty;
            }

            ProgramCounter += addr;

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
            ProgramCounter++;
            StackPush((byte)(ProgramCounter >> 8));
            StackPush((byte)(ProgramCounter & 0xff));
            StackPush((byte)(ProcessorStatus | (byte)ProcessorStatusBit.BreakCommand));
            SET_INTERRUPT(true);
            ProgramCounter = (ushort)((read(IrqVectorH) << 8) + read(IrqVectorL));

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
            if (IF_OVERFLOW() == false)
            {
                ProgramCounter = addr;
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
            if (IF_OVERFLOW())
            {
                ProgramCounter = addr;
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
            SET_CARRY(false);

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
            SET_DECIMAL(false);

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
            SET_INTERRUPT(false);

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
            SET_OVERFLOW(false);

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
            byte m = read(addr);

            SET_CARRY(A >= m);
            SET_ZERO(A == m);
            SET_NEGATIVE_FROM_VALUE(A - m);

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
            byte m = read(addr);

            SET_CARRY(X >= m);
            SET_ZERO(X == m);
            SET_NEGATIVE_FROM_VALUE(X - m);

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
            byte m = read(addr);

            SET_CARRY(Y >= m);
            SET_ZERO(Y == m);
            SET_NEGATIVE_FROM_VALUE(Y - m);

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
            byte m = read(addr);
            m--;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            write(addr, m);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>DEX - Decrement Index Registers</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// R ← R - 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void DEX(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            byte m = X;
            m--;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            X = m;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>DEY - Decrement Index Registers</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// R ← R - 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void DEY(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            byte m = Y;
            m--;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            Y = m;

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
            byte m = read(addr);
            m ^= A;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            A = m;

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
            byte m = read(addr);
            m++;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            write(addr, m);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>INX - Increment Index Registers</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// R ← R + 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void INX(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            byte m = X;
            m++;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            X = m;

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>INY - Increment Index Registers</para>
        /// <code>
        /// Flags affected: n-----z-
        ///
        /// R ← R + 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </code>
        /// </summary>
        public void INY(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            byte m = Y;
            m++;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            Y = m;

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
            if ((addr & 0xff00) != (ProgramCounter & 0xff00))
            {
                cycles += pageCrossPenalty;
            }

            ProgramCounter = addr;

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
            ushort savePC = ProgramCounter;

            ProgramCounter--;
            StackPush((byte)((ProgramCounter >> 8) & 0xff));
            StackPush((byte)(ProgramCounter & 0xff));
            ProgramCounter = addr;

            if ((savePC & 0xff00) != (ProgramCounter & 0xff00))
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
            byte m = read(addr);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            A = m;

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
            byte m = read(addr);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            X = m;

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
            byte m = read(addr);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            Y = m;

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
        public void LSR(ushort addr, bool accum, long cycles, long pageCrossPenalty = 0)
        {
            byte m = accum ? A : read(addr);
            SET_CARRY_ON_SHIFT(m);
            m >>= 1;
            SET_NEGATIVE(false);
            SET_ZERO_FROM_VALUE(m);
            if (accum == true)
            {
                A = m;
            }
            else
            {
                write(addr, m);
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
            byte m = read(addr);
            m |= A;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            A = m;

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
            StackPush(A);

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
            StackPush(ProcessorStatus);

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
            StackPush(X);

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
            StackPush(Y);

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
            A = StackPop();
            SET_NEGATIVE_FROM_VALUE(A);
            SET_ZERO_FROM_VALUE(A);

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
            ProcessorStatus = StackPop();

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
            X = StackPop();
            SET_NEGATIVE_FROM_VALUE(X);
            SET_ZERO_FROM_VALUE(X);

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
            Y = StackPop();
            SET_NEGATIVE_FROM_VALUE(Y);
            SET_ZERO_FROM_VALUE(Y);

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
            byte flag = (byte)(0x01 << bit);
            byte value = read(addr);
            value &= unchecked((byte)~flag);
            write(addr, value);

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
        public void ROL(ushort addr, bool accum, long cycles, long pageCrossPenalty = 0)
        {
            ushort m = accum ? A : read(addr);
            m <<= 1;
            if (IF_CARRY())
            {
                m |= 0x01;
            }
            SET_CARRY(m > 0xff);
            m &= 0xff;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            if (accum == true)
            {
                A = (byte)m;
            }
            else
            {
                write(addr, (byte)m);
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
        public void ROR(ushort addr, bool accum, long cycles, long pageCrossPenalty = 0)
        {
            ushort m = accum ? A : read(addr);
            if (IF_CARRY())
            {
                m |= 0x100;
            }
            SET_CARRY_ON_SHIFT(m);
            m >>= 1;
            m &= 0xff;
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            if (accum == true)
            {
                A = (byte)m;
            }
            else
            {
                write(addr, (byte)m);
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
            ProcessorStatus = StackPop();

            byte lo = StackPop();
            byte hi = StackPop();

            ProgramCounter = (ushort)((hi << 8) | lo);

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

            ProgramCounter = (ushort)(((hi << 8) | lo) + 1);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>SBC - Subtract with Borrow from Accumulator</para>
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
        public void SBC(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            byte m = read(addr);
            int tmp = A - m - (IF_CARRY() ? 0 : 1);

            SET_NEGATIVE_FROM_VALUE(tmp);
            SET_ZERO_FROM_VALUE(tmp & 0xff);
            SET_OVERFLOW_FROM_A(m, tmp);

            if (IF_DECIMAL())
            {
                cycles++;

                if (((A & 0x0F) - (IF_CARRY() ? 0 : 1)) < (m & 0x0F))
                    tmp -= 6;
                if (tmp > 0x99)
                {
                    tmp -= 0x60;
                }
            }

            SET_CARRY(tmp < 0x100);
            A = (byte)(tmp & 0xff);

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
            SET_CARRY(true);

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
            SET_DECIMAL(true);

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
            SET_INTERRUPT(true);

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
            byte flag = (byte)(0x01 << bit);
            byte value = read(addr);
            value |= flag;
            write(addr, value);

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
            write(addr, A);

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
            write(addr, X);

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
            write(addr, Y);

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
            X = A;
            SET_NEGATIVE_FROM_VALUE(X);
            SET_ZERO_FROM_VALUE(X);

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
            Y = A;
            SET_NEGATIVE_FROM_VALUE(Y);
            SET_ZERO_FROM_VALUE(Y);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>TRB - Test and Reset Memory Bits Against Accumulator</para>
        /// <code>
        /// Flags affected: ------z-
        /// </code>
        ///
        /// Logically AND together the complement of the value in the
        /// accumulator with the data at the effective address specified
        /// by the operand. Store the result at the memory location.
        ///
        /// This has the effect of clearing each memory bit for which the
        /// corresponding accumulator bit is set, while leaving unchanged
        /// all memory bits in which the corresponding accumulator bits are zeroes.
        ///
        /// The z zero flag is set based on a second and different operation
        /// the ANDing of the accumulator value (not its complement) with
        /// the memory value (the same way the BIT instruction affects the
        /// zero flag). The result of this second operation is not saved;
        /// only the zero flag is affected by it.
        /// </summary>
        public void TRB(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            byte value = read(addr);
            value = (byte)(value & (byte)~A);
            write(addr, value);

            SET_ZERO_FROM_VALUE(value & A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// <para>TSB - Test and Set Memory Bits Against Accumulator</para>
        /// <code>
        /// Flags affected: ------z-
        ///
        /// Logically OR together the value in the accumulator with the data
        /// at the effective address specified by the operand. Store the result
        /// at the memory location.
        ///
        /// This has the effect of setting each memory bit for which the
        /// corresponding accumulator bit is set, while leaving unchanged
        /// all memory bits in which the corresponding accumulator bits are
        /// zeroes.
        ///
        /// The z zero flag is set based on a second different operation,
        /// the ANDing of the accumulator value with the memory value (the
        /// same way the BIT instruction affects the zero flag). The result
        /// of this second operation is not saved; only the zero flag is
        /// affected by it.
        /// </code>
        /// </summary>
        public void TSB(ushort addr, long cycles, long pageCrossPenalty = 0)
        {
            byte value = read(addr);
            value = (byte)(value | A);
            write(addr, value);

            SET_ZERO_FROM_VALUE(value & A);

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
            X = StackPointer;
            SET_NEGATIVE_FROM_VALUE(StackPointer);
            SET_ZERO_FROM_VALUE(StackPointer);

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
            A = X;
            SET_NEGATIVE_FROM_VALUE(A);
            SET_ZERO_FROM_VALUE(A);

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
            StackPointer = X;

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
            A = Y;
            SET_NEGATIVE_FROM_VALUE(A);
            SET_ZERO_FROM_VALUE(A);

            WaitCycles(cycles);
        }

        /// <summary>
        /// Dummy operator, eats one opcode and increments the PC
        /// </summary>
        public void IllegalInstruction(ushort _, long cycles, long pageCrossPenalty = 0)
        {
            illegalOpCode = true;
        }
    }
}
