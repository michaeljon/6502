#pragma warning disable CA1822

namespace InnoWerks.Simulators
{
    public partial class Cpu
    {
        /// <summary>
        /// ADC - Add with Carry
        ///
        /// Flags affected: nv----zc
        ///
        /// A ← A + M + c
        ///
        /// n ← Most significant bit of result
        /// v ← Signed overflow of result
        /// z ← Set if the result is zero
        /// c ← Carry from ALU (bit 8/16 of result)
        ///
        /// </summary>
        public void ADC(ushort addr)
        {
            byte m = read(addr);
            int tmp = m + A + (IF_CARRY() ? 1 : 0);
            SET_ZERO_FROM_VALUE(tmp & 0xff);
            if (IF_DECIMAL())
            {
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
        }

        /// <summary>
        /// AND - And Accumulator with Memory
        ///
        /// Flags affected: n-----z-
        ///
        /// A ← A ^ M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </summary>
        public void AND(ushort addr)
        {
            byte m = read(addr);
            byte res = (byte)(m & A);
            SET_NEGATIVE_FROM_VALUE(res);
            SET_ZERO_FROM_VALUE(res);
            A = res;
        }

        /// <summary>
        /// ASL - Arithmetic Shift Left
        ///
        /// Flags affected: n-----zc
        ///
        /// M ← M + M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// c ← Most significant bit of original Memory
        /// </summary>
        public void ASL(ushort addr, bool accum)
        {
            byte m = accum ? A : read(addr);
            SET_CARRY_FROM_VALUE(m);
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
        }

        /// <summary>
        /// BBR - Branch on Bit Reset
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
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
        public void BBR(ushort addr, byte bit)
        {
            if ((addr & (0x01 << bit)) == 0)
            {
                sbyte offset = (sbyte)read(ProgramCounter++);
                ProgramCounter = (ushort)(ProgramCounter + offset);
            }
        }

        /// <summary>
        /// BBS - Branch on Bit Set
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
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
        public void BBS(ushort addr, byte bit)
        {
            if ((addr & (0x01 << bit)) != 0)
            {
                sbyte offset = (sbyte)read(ProgramCounter++);
                ProgramCounter = (ushort)(ProgramCounter + offset);
            }
        }

        /// <summary>
        /// BCC - Branch on Carry Clear
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </summary>
        public void BCC(ushort addr)
        {
            if (IF_CARRY() == false)
            {
                ProgramCounter = addr;
            }
        }

        /// <summary>
        /// BCS - Branch on Carry Set
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </summary>
        public void BCS(ushort addr)
        {
            if (IF_CARRY())
            {
                ProgramCounter = addr;
            }
        }

        /// <summary>
        /// BEQ - Branch on Result Zero
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </summary>
        public void BEQ(ushort addr)
        {
            if (IF_ZERO())
            {
                ProgramCounter = addr;
            }
        }

        /// <summary>
        /// BIT - Test Memory Bits against Accumulator
        ///
        /// Flags affected: nv----z-
        /// Flags affected (Immediate addressing mode only): ------z-
        ///
        /// A ^ M
        ///
        /// n ← Most significant bit of memory
        /// v ← Second most significant bit of memory
        /// z ← Set if logical AND of memory and Accumulator is zero
        /// </summary>
        public void BIT(ushort addr)
        {
            byte m = read(addr);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_OVERFLOW_FROM_VALUE(m);

            byte res = (byte)(m & A);
            SET_ZERO_FROM_VALUE(res);
        }

        /// <summary>
        /// BMI - Branch on Result Minus
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        ///
        /// Branch taken (BRL):
        /// PC ← PC + label
        /// </summary>
        public void BMI(ushort addr)
        {
            if (IF_NEGATIVE())
            {
                ProgramCounter = addr;
            }
        }

        /// <summary>
        /// BNE - Branch on Negative
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        ///
        /// Branch taken (BRL):
        /// PC ← PC + label
        /// </summary>
        public void BNE(ushort addr)
        {
            if (IF_ZERO() == false)
            {
                ProgramCounter = addr;
            }
        }

        /// <summary>
        /// BPL - Branch on Result Plus
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </summary>
        public void BPL(ushort addr)
        {
            if (IF_NEGATIVE() == false)
            {
                ProgramCounter = addr;
            }
        }

        /// <summary>
        /// BRA - Branch Always
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </summary>
        public void BRA(ushort addr)
        {
            ProgramCounter = addr;
        }

        /// <summary>
        /// BRL - Branch Long
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </summary>
        public void BRL(ushort addr)
        {
            ProgramCounter += addr;
        }

        /// <summary>
        /// BRK - Force Break
        ///
        /// Flags affected: ----di--
        ///
        /// S     ← S - 4
        /// [S+4] ← P
        /// [S+3] ← PC.h
        /// [S+2] ← PC.l
        /// [S+1] ← P
        /// d     ← 0
        /// i     ← 1
        /// P     ← 0
        /// PC    ← interrupt address
        /// </summary>
        public void BRK(ushort _)
        {
            ProgramCounter++;
            StackPush((byte)(ProgramCounter >> 8));
            StackPush((byte)(ProgramCounter & 0xff));
            StackPush((byte)(ProcessorStatus | (byte)ProcessorFlag.Unused | (byte)ProcessorFlag.BreakCommand));
            SET_INTERRUPT(true);
            ProgramCounter = (ushort)((read(IrqVectorH) << 8) + read(IrqVectorL));
        }

        /// <summary>
        /// BBR - Branch on Overflow Clear
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </summary>
        public void BVC(ushort addr)
        {
            if (IF_OVERFLOW() == false)
            {
                ProgramCounter = addr;
            }
        }

        /// <summary>
        /// BBR - Branch on Overflow Set
        ///
        /// Flags affected: --------
        /// Branch not taken:
        /// —
        ///
        /// Branch taken:
        /// PC ← PC + sign-extend(near)
        /// </summary>
        public void BVS(ushort addr)
        {
            if (IF_OVERFLOW())
            {
                ProgramCounter = addr;
            }
        }

        /// <summary>
        /// CLC - Clear Carry
        /// Flags affected (CLC): -------c
        /// c ← 0
        /// </summary>
        public void CLC(ushort _)
        {
            SET_CARRY(false);
        }

        /// <summary>
        /// CLC - Clear Decimal
        /// Flags affected (CLD): ----d---
        /// d ← 0
        /// </summary>
        public void CLD(ushort _)
        {
            SET_DECIMAL(false);
        }

        /// <summary>
        /// CLC - Clear Interrupt
        /// Flags affected (CLI): -----i--
        /// i ← 0
        /// </summary>
        public void CLI(ushort _)
        {
            SET_INTERRUPT(false);
        }

        /// <summary>
        /// CLC - Clear Overflow
        /// Flags affected (CLV): -v------
        /// v ← 0
        /// </summary>
        public void CLV(ushort _)
        {
            SET_OVERFLOW(false);
        }

        /// <summary>
        /// CMP - Compare Accumulator with Memory
        ///
        /// Flags affected: n-----zc
        /// A - M
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero (Set if A == M)
        /// c ← Carry from ALU (Set if A >= M)
        /// </summary>
        public void CMP(ushort addr)
        {
            int m = read(addr);
            int res = A - m;

            SET_CARRY(A >= (m & 0xff));
            SET_ZERO(A == (m & 0xff));
            SET_ZERO_FROM_VALUE(res & 0xff);
        }

        /// <summary>
        /// CPX - Compare Index Register X with Memory
        ///
        /// Flags affected: n-----zc
        ///
        /// X - M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero (Set if X == M)
        /// c ← Carry from ALU (Set if X >= M)
        /// </summary>
        public void CPX(ushort addr)
        {
            int m = read(addr);
            int res = X - m;

            SET_CARRY(X >= (m & 0xff));
            SET_ZERO(X == (m & 0xff));
            SET_ZERO_FROM_VALUE(res & 0xff);
        }

        /// <summary>
        /// CPY - Compare Index Register Y with Memory
        ///
        /// Flags affected: n-----zc
        ///
        /// Y - M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero (Set if Y == M)
        /// c ← Carry from ALU (Set if Y >= M)
        /// </summary>
        public void CPY(ushort addr)
        {
            int m = read(addr);
            int res = Y - m;

            SET_CARRY(Y >= (m & 0xff));
            SET_ZERO(Y == (m & 0xff));
            SET_ZERO_FROM_VALUE(res & 0xff);
        }

        /// <summary>
        /// DEC - Decrement
        ///
        /// Flags affected: n-----z-
        ///
        /// M ← M - 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </summary>
        public void DEC(ushort addr)
        {
            byte m = read(addr);
            m = (byte)((m - 1) & 0xff);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            write(addr, m);
        }

        /// <summary>
        /// DEX - Decrement Index Registers
        ///
        /// Flags affected: n-----z-
        ///
        /// R ← R - 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </summary>
        public void DEX(ushort _)
        {
            byte m = X;
            m = (byte)((m - 1) & 0xff);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            X = m;
        }

        /// <summary>
        /// DEY - Decrement Index Registers
        ///
        /// Flags affected: n-----z-
        ///
        /// R ← R - 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </summary>
        public void DEY(ushort _)
        {
            byte m = Y;
            m = (byte)((m - 1) & 0xff);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            Y = m;
        }

        /// <summary>
        /// EOR - Exclusive OR Accumulator with Memory
        ///
        /// Flags affected: n-----z-
        ///
        /// A ← A ^ M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </summary>
        public void EOR(ushort addr)
        {
            byte m = read(addr);
            m = (byte)(A ^ m);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            A = m;
        }

        /// <summary>
        /// INC - Increment
        ///
        /// Flags affected: n-----z-
        ///
        /// M ← M + 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </summary>
        public void INC(ushort addr)
        {
            byte m = read(addr);
            m = (byte)((m + 1) & 0xff);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            write(addr, m);
        }

        /// <summary>
        /// INX - Increment Index Registers
        ///
        /// Flags affected: n-----z-
        ///
        /// R ← R + 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </summary>
        public void INX(ushort _)
        {
            byte m = X;
            m = (byte)((m + 1) & 0xff);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            X = m;
        }

        /// <summary>
        /// INY - Increment Index Registers
        ///
        /// Flags affected: n-----z-
        ///
        /// R ← R + 1
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </summary>
        public void INY(ushort _)
        {
            byte m = Y;
            m = (byte)((m + 1) & 0xff);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            Y = m;
        }

        /// <summary>
        /// JMP - Jump
        ///
        /// Flags affected: --------
        ///
        /// JMP:
        /// PC     ← M
        /// </summary>
        public void JMP(ushort addr)
        {
            ProgramCounter = addr;
        }

        /// <summary>
        /// JSR - Jump to Subroutine
        /// Flags affected: --------
        ///
        /// JSR:
        /// PC     ← PC - 1
        /// S      ← S - 2
        /// [S+2]  ← PC.h
        /// [S+1]  ← PC.l
        /// PC     ← M
        /// </summary>
        public void JSR(ushort addr)
        {
            ProgramCounter--;
            StackPush((byte)((ProgramCounter >> 8) & 0xff));
            StackPush((byte)(ProgramCounter & 0xff));
            ProgramCounter = addr;
        }

        /// <summary>
        /// LDA - Load Accumulator from Memory
        ///
        ///         Flags affected: n-----z-
        ///
        /// A ← M
        ///
        /// n ← Most significant bit of Accumulator
        /// z ← Set if the Accumulator is zero
        /// </summary>
        public void LDA(ushort addr)
        {
            byte m = read(addr);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            A = m;
        }

        /// <summary>
        /// LDX - Load Index Register X from Memory
        ///
        /// Flags affected: n-----z-
        ///
        /// X ← M
        ///
        /// n ← Most significant bit of X
        /// z ← Set if the X is zero
        /// </summary>
        public void LDX(ushort addr)
        {
            byte m = read(addr);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            X = m;
        }

        /// <summary>
        /// LDY - Load Index Register Y from Memory
        ///
        /// Flags affected: n-----z-
        ///
        /// Y ← M
        ///
        /// n ← Most significant bit of Y
        /// z ← Set if the Y is zero
        /// </summary>
        public void LDY(ushort addr)
        {
            byte m = read(addr);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            Y = m;
        }

        /// <summary>
        /// LSR - Logical Shift Right
        ///
        /// Flags affected: n-----zc
        ///
        /// M ← M >> 1
        ///
        /// n ← cleared
        /// z ← Set if the result is zero
        /// c ← Bit 0 of original memory
        ///
        /// NOTE: This is an unsigned operation, the MSB of the result is always 0.
        /// </summary>
        public void LSR(ushort addr, bool accum)
        {
            byte m = accum ? A : read(addr);
            SET_CARRY_FROM_VALUE(m);
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
        }

        /// <summary>
        /// NOP - No Operation
        ///
        /// Flags affected: --------
        /// </summary>
        public void NOP(ushort _)
        {
        }

        /// <summary>
        /// ORA - OR Accumulator with Memory
        ///
        /// Flags affected: n-----z-
        ///
        /// A ← A | M
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// </summary>
        public void ORA(ushort addr)
        {
            byte m = read(addr);
            m = (byte)(A | m);
            SET_NEGATIVE_FROM_VALUE(m);
            SET_ZERO_FROM_VALUE(m);
            A = m;
        }

        /// <summary>
        /// PHA - Push to Stack
        ///
        /// Flags affected: --------
        ///
        /// 8 bit register:
        /// S     ← S - 1
        /// [S+1] ← R
        /// </summary>
        public void PHA(ushort _)
        {
            StackPush(A);
        }

        /// <summary>
        /// PHP - Push to Stack
        ///
        /// Flags affected: --------
        ///
        /// 8 bit register:
        /// S     ← S - 1
        /// [S+1] ← R
        /// </summary>
        public void PHP(ushort _)
        {
            StackPush(ProcessorStatus);
        }

        /// <summary>
        /// PHX - Push to Stack
        ///
        /// Flags affected: --------
        ///
        /// 8 bit register:
        /// S     ← S - 1
        /// [S+1] ← R
        /// </summary>
        public void PHX(ushort _)
        {
            StackPush(X);
        }

        /// <summary>
        /// PHY - Push to Stack
        ///
        /// Flags affected: --------
        ///
        /// 8 bit register:
        /// S     ← S - 1
        /// [S+1] ← R
        /// </summary>
        public void PHY(ushort _)
        {
            StackPush(Y);
        }

        /// <summary>
        /// PLA - Pull from Stack
        ///
        /// Flags affected: n-----z-
        /// Flags affected (PLP): nvmxdizc
        ///
        /// 8 bit register:
        /// R   ← [S+1]
        /// S   ← S + 1
        ///
        /// n   ← Most significant bit of register
        /// z   ← Set if the register is zero
        /// </summary>
        public void PLA(ushort _)
        {
            A = StackPop();
            SET_NEGATIVE_FROM_VALUE(A);
            SET_ZERO_FROM_VALUE(A);
        }

        /// <summary>
        /// PLP - Pull from Stack
        ///
        /// Flags affected (PLP): nvmxdizc
        ///
        /// 8 bit register:
        /// P   ← [S+1]
        /// S   ← S + 1
        /// </summary>
        public void PLP(ushort _)
        {
            ProcessorStatus = StackPop();
        }

        /// <summary>
        /// PLX - Pull from Stack
        ///
        /// Flags affected: n-----z-
        ///
        /// 8 bit register:
        /// R   ← [S+1]
        /// S   ← S + 1
        ///
        /// n   ← Most significant bit of register
        /// z   ← Set if the register is zero
        /// </summary>
        public void PLX(ushort _)
        {
            X = StackPop();
            SET_NEGATIVE_FROM_VALUE(X);
            SET_ZERO_FROM_VALUE(X);
        }

        /// <summary>
        /// PLY - Pull from Stack
        ///
        /// Flags affected: n-----z-
        ///
        /// 8 bit register:
        /// R   ← [S+1]
        /// S   ← S + 1
        ///
        /// n   ← Most significant bit of register
        /// z   ← Set if the register is zero
        /// </summary>
        public void PLY(ushort _)
        {
            Y = StackPop();
            SET_NEGATIVE_FROM_VALUE(Y);
            SET_ZERO_FROM_VALUE(Y);
        }

        /// <summary>
        /// RMB - Reset Memory Bit
        ///
        /// Flags affected: -------
        ///
        /// Clear the specified bit in the zero page memory location
        /// specified in the operand. The bit to clear is specified
        /// by a number (0 through 7) concatenated to the end of the
        /// mnemonic.
        /// </summary>
        public void RMB(ushort addr, byte bit)
        {
            byte flag = (byte)(0x01 << bit);
            byte value = read(addr);
            value &= unchecked((byte)~flag);
            write(addr, value);
        }

        /// <summary>
        /// ROL - Rotate Left
        ///
        /// Flags affected: n-----zc
        ///
        /// M ← M + M + c
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// c ← Most significant bit of original Memory
        /// </summary>
        public void ROL(ushort addr, bool accum)
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
        }

#pragma warning disable CS1570
        /// <summary>
        /// ROR - Rotate Right
        ///
        /// Flags affected: n-----zc
        ///
        /// M ← (c << (m ? 7 : 15)) | (M >> 1)
        ///
        /// n ← Most significant bit of result
        /// z ← Set if the result is zero
        /// c ← Bit 0 of original memory
        /// </summary>
#pragma warning restore CS1570
        public void ROR(ushort addr, bool accum)
        {
            ushort m = accum ? A : read(addr);
            if (IF_CARRY())
            {
                m |= 0x100;
            }
            SET_CARRY_FROM_VALUE(m);
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
        }

        /// <summary>
        /// RTI - Return From Interrupt
        ///
        /// Flags affected: nvmxdizc
        ///
        /// P    ← [S+1]
        /// PC.l ← [S+2]
        /// PC.h ← [S+3]
        /// S    ← S + 3
        /// </summary>
        public void RTI(ushort _)
        {
            ProcessorStatus = StackPop();

            byte lo = StackPop();
            byte hi = StackPop();

            ProgramCounter = (ushort)((hi << 8) | lo);
        }

        /// <summary>
        /// RTS  - Return From Subroutine
        ///
        /// Flags affected: --------
        ///
        /// PC.l ← [S+1]
        /// PC.h ← [S+2]
        /// S    ← S + 2
        /// PC   ← PC + 1
        /// </summary>
        public void RTS(ushort _)
        {
            byte lo = StackPop();
            byte hi = StackPop();

            ProgramCounter = (ushort)(((hi << 8) | lo) + 1);
        }

        /// <summary>
        /// SBC - Subtract with Borrow from Accumulator
        ///
        /// Flags affected: nv----zc
        ///
        /// A ← A + (~M) + c
        ///
        /// n ← Most significant bit of result
        /// v ← Signed overflow of result
        /// z ← Set if the Accumulator is zero
        /// c ← Carry from ALU(bit 8/16 of result) (set if borrow not required)
        /// </summary>
        public void SBC(ushort addr)
        {
            byte m = read(addr);
            int tmp = A - m - (IF_CARRY() ? 0 : 1);

            SET_NEGATIVE_FROM_VALUE(tmp);
            SET_ZERO_FROM_VALUE(tmp & 0xff);
            SET_OVERFLOW_FROM_A(m, tmp);

            if (IF_DECIMAL())
            {
                if (((A & 0x0F) - (IF_CARRY() ? 0 : 1)) < (m & 0x0F))
                    tmp -= 6;
                if (tmp > 0x99)
                {
                    tmp -= 0x60;
                }
            }

            SET_CARRY(tmp < 0x100);
            A = (byte)(tmp & 0xff);
        }

        /// <summary>
        /// SEC - Set Carry
        ///
        /// Flags affected (SEC): -------c
        ///
        /// c ← 1
        /// </summary>
        public void SEC(ushort _)
        {
            SET_CARRY(true);
        }

        /// <summary>
        /// SED - Set Decimal
        ///
        /// Flags affected (SED): ----d---
        ///
        /// d ← 1
        /// </summary>
        public void SED(ushort _)
        {
            SET_DECIMAL(true);
        }

        /// <summary>
        /// SED - Set Interrupt
        ///
        /// Flags affected (SEI): -----i--
        ///
        /// i ← 1
        /// </summary>
        public void SEI(ushort _)
        {
            SET_INTERRUPT(true);
        }

        /// <summary>
        /// SMB - Set Memory Bit
        ///
        /// Flags affected: n------ ?
        ///
        /// Clear the specified bit in the zero page memory location
        /// specified in the operand. The bit to clear is specified
        /// by a number (0 through 7) concatenated to the end of the
        /// mnemonic.
        /// </summary>
        public void SMB(ushort addr, byte bit)
        {
            byte flag = (byte)(0x01 << bit);
            byte value = read(addr);
            value |= flag;
            write(addr, value);
        }

        /// <summary>
        /// STA - Store Accumulator to Memory
        ///
        /// Flags affected: --------
        ///
        /// M ← A
        /// </summary>
        public void STA(ushort addr)
        {
            write(addr, A);
        }

        /// <summary>
        /// STX - Store Index Register X to Memory
        ///
        /// Flags affected: --------
        ///
        /// M ← X
        /// </summary>
        public void STX(ushort addr)
        {
            write(addr, X);
        }

        /// <summary>
        /// STY - Store Index Register Y to Memory
        ///
        /// Flags affected: --------
        ///
        /// M ← Y
        /// </summary>
        public void STY(ushort addr)
        {
            write(addr, Y);
        }

        /// <summary>
        /// STZ - Store Zero to Memory
        ///
        /// Flags affected: --------
        ///
        /// M ← 0
        /// </summary>
        public void STZ(ushort addr)
        {
            write(addr, 0);
        }

        /// <summary>
        /// TAX - Transfer Accumulator to X
        ///
        /// Flags affected: n-----z-
        ///
        /// X ← A
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </summary>
        public void TAX(ushort _)
        {
            X = A;
            SET_NEGATIVE_FROM_VALUE(X);
            SET_ZERO_FROM_VALUE(X);
        }

        /// <summary>
        /// TAY - Transfer Accumulator to Y
        ///
        /// Flags affected: n-----z-
        ///
        /// Y ← A
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </summary>
        public void TAY(ushort _)
        {
            Y = A;
            SET_NEGATIVE_FROM_VALUE(Y);
            SET_ZERO_FROM_VALUE(Y);
        }

        /// <summary>
        /// TRB - Test and Reset Memory Bits Against Accumulator
        ///
        /// Flags affected: ------z-
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
        public void TRB(ushort addr)
        {
            byte value = read(addr);
            value = (byte)(value & (byte)~A);
            write(addr, value);

            SET_ZERO_FROM_VALUE(value & A);
        }

        /// <summary>
        /// TSB - Test and Set Memory Bits Against Accumulator
        ///
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
        /// </summary>
        public void TSB(ushort addr)
        {
            byte value = read(addr);
            value = (byte)(value | A);
            write(addr, value);

            SET_ZERO_FROM_VALUE(value & A);
        }

        /// <summary>
        /// TSX - Transfer Stack Pointer to X
        ///
        /// Flags affected: n-----z-
        ///
        /// X ← S
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </summary>
        public void TSX(ushort _)
        {
            X = StackPointer;
            SET_NEGATIVE_FROM_VALUE(StackPointer);
            SET_ZERO_FROM_VALUE(StackPointer);
        }

        /// <summary>
        /// TSX - Transfer X to Accumulator
        ///
        /// Flags affected: n-----z-
        ///
        /// A ← X
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </summary>
        public void TXA(ushort _)
        {
            A = X;
            SET_NEGATIVE_FROM_VALUE(A);
            SET_ZERO_FROM_VALUE(A);
        }

        /// <summary>
        /// TXS - Transfer X to Stack Pointer
        ///
        /// Flags affected: n-----z-
        ///
        /// S ← X
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </summary>
        public void TXS(ushort _)
        {
            StackPointer = X;
        }

        /// <summary>
        /// TYA - Transfer Y to Accumulator
        ///
        /// Flags affected: n-----z-
        ///
        /// A ← Y
        /// n ← Most significant bit of the transferred value
        /// z ← Set if the transferred value is zero
        /// </summary>
        public void TYA(ushort _)
        {
            A = Y;
            SET_NEGATIVE_FROM_VALUE(A);
            SET_ZERO_FROM_VALUE(A);
        }

        /// <summary>
        /// Dummy operator, eats one opcode and increments the PC
        /// </summary>
        public void IllegalInstruction(ushort _)
        {
            illegalOpCode = true;
        }
    }
}
