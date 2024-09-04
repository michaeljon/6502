using System;
using InnoWerks.Processors;

namespace InnoWerks.Simulators
{
    public partial class Cpu
    {
        public int Run(bool stopOnBreak = false, bool writeInstructions = false)
        {
            var instructionCount = 0;

            while (true)
            {
                instructionCount++;

                preExecutionCallback?.Invoke(this, Registers.ProgramCounter);

                // T0
                var operation = memory.Read(Registers.ProgramCounter);
                // this is a bad hard-coding right now...
                if (operation == 0x00)
                {
                    // BRK
                    break;
                }

                Dispatch(operation, writeInstructions);

                if (writeInstructions)
                {
                    Console.WriteLine($"  {Registers.GetRegisterDisplay}   {Registers.InternalGetFlagsDisplay,-8}");
                }

                postExecutionCallback?.Invoke(this);
            }

            return instructionCount;
        }

        public void Step(bool writeInstructions = false)
        {
            preExecutionCallback?.Invoke(this, Registers.ProgramCounter);

            // T0
            var operation = memory.Read(Registers.ProgramCounter);
            Dispatch(operation, writeInstructions);

            if (writeInstructions)
            {
                Console.WriteLine($"  {Registers.GetRegisterDisplay}   {Registers.InternalGetFlagsDisplay,-8}");
            }

            postExecutionCallback?.Invoke(this);
        }

        private void Dispatch(byte operation, bool writeInstructions = false)
        {
            OpCodeDefinition opCodeDefinition = CpuClass == CpuClass.WDC6502 ?
                CpuInstructions.OpCode6502[operation] :
                CpuInstructions.OpCode65C02[operation];

            // decode the operand based on the opcode and addressing mode
            if (opCodeDefinition.DecodeOperand(this) == false)
            {
                // this is an illegal operation that should jam
                throw new IllegalOpCodeException(Registers.ProgramCounter, operation);
            }

            var stepToExecute = $"{Registers.ProgramCounter:X4} {opCodeDefinition.OpCode}   {OperandDisplay,-10}";
            if (writeInstructions)
            {
                Console.Write(stepToExecute);
            }

            switch (opCodeDefinition.OpCode)
            {
                // A. 1. SINGLE-BYTE INSTRUCTIONS
                // These single-byte instructions require two cycles to execute. During the second
                // cycle the address of the next instruction in program sequence will be placed on
                // the address bus. However, the OP CODE which appears on the data bus during the
                // second cycle will be ignored. This same instruction will be fetched on the following
                // cycle, at which time it will be decoded and executed. The ASL, LSR, ROL and ROR
                // instructions apply to the accumulator mode of address.

                case OpCode.ASL_A:
                case OpCode.CLC:
                case OpCode.CLD:
                case OpCode.CLI:
                case OpCode.CLV:
                case OpCode.DEA:
                case OpCode.DEX:
                case OpCode.DEY:
                case OpCode.INA:
                case OpCode.INX:
                case OpCode.INY:
                case OpCode.LSR_A:
                case OpCode.NOP:
                case OpCode.ROL_A:
                case OpCode.ROR_A:
                case OpCode.SEC:
                case OpCode.SED:
                case OpCode.SEI:
                case OpCode.TAX:
                case OpCode.TAY:
                case OpCode.TSX:
                case OpCode.TXA:
                case OpCode.TXS:
                case OpCode.TYA:
                    // A. 1.1 Implied Addressing (2 Cycles)
                    {
                        // T1
                        /* var discarded = */
                        memory.Read((ushort)(Registers.ProgramCounter + 1));
                        opCodeDefinition.Execute(this, 0, 0);

                        Registers.ProgramCounter++;
                    }
                    break;

                // A. 2. INTERNAL EXECUTION ON MEMORY DATA
                // The instructions listed above will execute by performing operations inside the microprocessor
                // using data fetched from the effective address. This total operation requires three steps. The
                // first step (one cycle) is the OP CODE fetch. The second (zero to four cycles) Is the calculation
                // of an effective address. The final step is the fetching of the data from the effective address.
                // Execution of the instruction takes place during the fetching and decoding of the next instruction.
                case OpCode.ADC:
                case OpCode.AND:
                case OpCode.BIT:
                case OpCode.CMP:
                case OpCode.CPX:
                case OpCode.CPY:
                case OpCode.EOR:
                case OpCode.LDA:
                case OpCode.LDX:
                case OpCode.LDY:
                case OpCode.ORA:
                case OpCode.SBC:
                    switch (opCodeDefinition.AddressingMode)
                    {
                        // A. 1.1 Implied Addressing (2 Cycles)
                        case AddressingMode.Accumulator:
                            {
                                // T1
                                /* var discarded = */
                                memory.Read((ushort)(Registers.ProgramCounter + 1));
                                opCodeDefinition.Execute(this, 0, 0);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        // A. 2.1. Immediate Addressing (2 Cycles)
                        case AddressingMode.Immediate:
                            {
                                // T1
                                var data = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                opCodeDefinition.Execute(this, 0, data);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        // A. 2.2. Zero Page Addressing (3 Cycles)
                        case AddressingMode.ZeroPage:
                            {
                                // T1
                                var adl = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var data = memory.Read(adl);
                                opCodeDefinition.Execute(this, 0, data);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        // A. 2.3. Absolute Addressing (4 Cycles)
                        case AddressingMode.Absolute:
                            {
                                // T1
                                var adl = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var adh = memory.Read((ushort)(Registers.ProgramCounter + 2));
                                // T3
                                var ad = (ushort)((adh << 8) | adl);
                                var data = memory.Read(ad);

                                opCodeDefinition.Execute(this, ad, data);

                                Registers.ProgramCounter += 3;
                            }
                            break;

                        // A. 2.4. Indirect, X Addressing (6 Cycles)
                        case AddressingMode.XIndexedIndirect:
                            {
                                // T1
                                var bal = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                /* var discarded = */
                                memory.Read(bal);
                                // T3
                                var adl = memory.Read((ushort)((bal + Registers.X) & 0xff));
                                // T4
                                var adh = memory.Read((ushort)((bal + Registers.X + 1) & 0xff));
                                // T5
                                var ad = (ushort)((adh << 8) | adl);
                                var data = memory.Read(ad);

                                opCodeDefinition.Execute(this, ad, data);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        // A. 2.5. Absolute, X or Absolute, Y Addressing (4 or 5 Cycles)
                        case AddressingMode.AbsoluteXIndexed:
                        case AddressingMode.AbsoluteYIndexed:
                            {
                                // T1
                                var bal = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var bah = memory.Read((ushort)(Registers.ProgramCounter + 2));
                                // T3
                                var index = opCodeDefinition.AddressingMode == AddressingMode.AbsoluteXIndexed ?
                                    Registers.X :
                                    Registers.Y;

                                var adl = (bal + index) & 0xff;     // Fetch Data (No Page Crossing)
                                var adh = bah + 0;                  // Carry is 0 or 1 as Required from Previous Add Operation
                                var ad = (ushort)((adh << 8) + adl);
                                var data = memory.Read(ad);

                                // T4
                                var adWithIndex = (ushort)((adh << 8) + bal + index);
                                var adWithoutIndex = (ushort)((adh << 8) + bal);

                                if ((adWithIndex & 0xff00) != (adWithoutIndex & 0xff00))
                                {
                                    data = memory.Read((ushort)((bah << 8) + bal + index));
                                }

                                opCodeDefinition.Execute(this, 0, data);

                                Registers.ProgramCounter += 3;
                            }
                            break;

                        // A. 2.6. Zero Page, X or Zero Page, Y Addressing Modes (4 Cycles)
                        case AddressingMode.ZeroPageXIndexed:
                        case AddressingMode.ZeroPageYIndexed:
                            {
                                // T1
                                var bal = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                /* var discarded = */
                                memory.Read(bal);
                                // T3
                                var index = opCodeDefinition.AddressingMode == AddressingMode.ZeroPageXIndexed ?
                                    Registers.X :
                                    Registers.Y;
                                var data = memory.Read((ushort)((bal + index) & 0xff));

                                opCodeDefinition.Execute(this, 0, data);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        // A. 2.7. Indirect, Y Addressing Mode (5 or 6 Cycles)
                        case AddressingMode.IndirectYIndexed:
                            {
                                // T1
                                var ial = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var bal = memory.Read((ushort)(ial));
                                // T3
                                var bah = memory.Read((ushort)((ial + 1) & 0xff));

                                // T4
                                var adl = (bal + Registers.Y) & 0xff;     // Fetch Data (No Page Crossing)
                                var adh = bah + 0;                  // Carry is 0 or 1 as Required from Previous Add Operation
                                var ad = (ushort)((adh << 8) + adl);
                                var data = memory.Read(ad);

                                // T5
                                var adWithIndex = (ushort)((adh << 8) + bal + Registers.Y);
                                var adWithoutIndex = (ushort)((adh << 8) + bal);

                                if ((adWithIndex & 0xff00) != (adWithoutIndex & 0xff00))
                                {
                                    data = memory.Read((ushort)((bah << 8) + bal + Registers.Y));
                                }

                                opCodeDefinition.Execute(this, 0, data);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        default:
                            throw new UnhandledAddressingModeException(Registers.ProgramCounter, operation, opCodeDefinition.OpCode, opCodeDefinition.AddressingMode);
                    }
                    break;

                // A. 3. STORE OPERATIONS
                // The specific steps taken in the Store Operations are very similar to those taken in the
                // previous group (internal execution on memory data). However, in the Store Operation, the
                // fetch of data is replaced by a WRITE (R/W = 0) cycle. No overlapping occurs and no
                // shortening of the instruction time occurs on indexing operations.
                case OpCode.STA:
                case OpCode.STX:
                case OpCode.STY:
                case OpCode.STZ:
                    byte val = opCodeDefinition.OpCode switch
                    {
                        OpCode.STA => Registers.A,
                        OpCode.STX => Registers.X,
                        OpCode.STY => Registers.Y,
                        OpCode.STZ => 0,

                        _ => throw new IllegalOpCodeException("OpCode doesn't map to A, X, or Y")
                    };

                    switch (opCodeDefinition.AddressingMode)
                    {
                        // A. 3.1. Zero Page Addressing (3 Cycles)
                        case AddressingMode.ZeroPage:
                            {
                                // T1
                                var adl = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                opCodeDefinition.Execute(this, adl, val);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        // A. 3.2. Absolute Addressing (4 Cycles)
                        case AddressingMode.Absolute:
                            {
                                // T1
                                var adl = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var adh = memory.Read((ushort)(Registers.ProgramCounter + 2));
                                // T3
                                var ad = (ushort)((adh << 8) | adl);
                                opCodeDefinition.Execute(this, ad, val);

                                Registers.ProgramCounter += 3;
                            }
                            break;

                        // A. 3.3. Indirect, X Addressing (6 Cycles)
                        case AddressingMode.XIndexedIndirect:
                            {
                                // T1
                                var bal = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                /* var discarded = */
                                memory.Read(bal);
                                // T3
                                var adl = memory.Read((ushort)((bal + Registers.X) & 0xff));
                                // T4
                                var adh = memory.Read((ushort)((bal + Registers.X + 1) & 0xff));
                                // T5
                                var ad = (ushort)((adh << 8) | adl);
                                opCodeDefinition.Execute(this, ad, val);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        // A. 3.4. Absolute, X or Absolute, Y Addressing (5 Cycles)
                        case AddressingMode.AbsoluteXIndexed:
                        case AddressingMode.AbsoluteYIndexed:
                            {
                                // T1
                                var bal = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var bah = memory.Read((ushort)(Registers.ProgramCounter + 2));
                                // T3
                                var index = opCodeDefinition.AddressingMode == AddressingMode.AbsoluteXIndexed ?
                                    Registers.X :
                                    Registers.Y;

                                var adl = bal + index;
                                /* var discarded = */
                                memory.Read((ushort)((bah << 8) + (adl & 0xff)));

                                // T4
                                opCodeDefinition.Execute(this, (ushort)((bah << 8) + adl), val);

                                Registers.ProgramCounter += 3;
                            }
                            break;

                        // A. 3.5. Zero Page, X or Zero Page, Y Addressing Modes (4 Cycles)
                        case AddressingMode.ZeroPageXIndexed:
                        case AddressingMode.ZeroPageYIndexed:
                            {
                                // T1
                                var bal = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                /* var discarded = */
                                memory.Read(bal);
                                // T3
                                var index = opCodeDefinition.AddressingMode == AddressingMode.ZeroPageXIndexed ?
                                    Registers.X :
                                    Registers.Y;
                                var adl = (ushort)((bal + index) & 0xff);

                                // T4
                                opCodeDefinition.Execute(this, adl, val);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        // A. 3.6. Indirect, Y Addressing Mode (6 Cycles)
                        case AddressingMode.IndirectYIndexed:
                            {
                                // T1
                                var ial = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var bal = memory.Read((ushort)(ial));
                                // T3
                                var bah = memory.Read((ushort)((ial + 1) & 0xff));
                                // T4
                                var adl = bal + Registers.Y;
                                /* var discarded = */
                                memory.Read((ushort)((bah << 8) + (adl & 0xff)));

                                // T5
                                opCodeDefinition.Execute(this, (ushort)((bah << 8) + adl), val);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        default:
                            throw new UnhandledAddressingModeException(Registers.ProgramCounter, operation, opCodeDefinition.OpCode, opCodeDefinition.AddressingMode);
                    }
                    break;

                // A. 4. READ -- MODIFY -- WRITE OPERATIONS
                // The Read -- Modify -- Write operations involve the loading of operands from the
                // operand address, modification of the operand and the resulting modified data being
                // stored in the original location.
                case OpCode.ASL:
                case OpCode.LSR:
                case OpCode.DEC:
                case OpCode.INC:
                case OpCode.ROL:
                case OpCode.ROR:
                    switch (opCodeDefinition.AddressingMode)
                    {
                        // A. 4.1. Zero Page Addressing (5 Cycles)
                        case AddressingMode.ZeroPage:
                            {
                                // T1
                                var adl = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var data = memory.Read(adl);
                                // T3
                                memory.Write(adl, data);
                                // T4
                                opCodeDefinition.Execute(this, adl, data);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        // A. 4.2. Absolute Addressing (6 Cycles)
                        case AddressingMode.Absolute:
                            {
                                // T1
                                var adl = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var adh = memory.Read((ushort)(Registers.ProgramCounter + 2));
                                // T3
                                var ad = (ushort)((adh << 8) | adl);
                                var data = memory.Read(ad);
                                // T4
                                memory.Write(ad, data);
                                // T3
                                opCodeDefinition.Execute(this, ad, data);

                                Registers.ProgramCounter += 3;
                            }
                            break;

                        // A. 4.3. Zero Page, X Addressing (6 Cycles)
                        case AddressingMode.ZeroPageXIndexed:
                            {
                                // T1
                                var bal = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                /* var discarded = */
                                memory.Read(bal);
                                // T3
                                var ad = (ushort)((bal + Registers.X) & 0xff);
                                var data = memory.Read(ad);
                                // T4
                                memory.Write(ad, data);
                                // T5
                                opCodeDefinition.Execute(this, ad, data);

                                Registers.ProgramCounter += 2;
                            }
                            break;

                        // A. 4.4. Absolute, X Addressing (7 Cycles)
                        case AddressingMode.AbsoluteXIndexed:
                            {
                                // T1
                                var bal = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var bah = memory.Read((ushort)(Registers.ProgramCounter + 2));
                                // T3
                                /* var discarded = */
                                memory.Read((ushort)((bah << 8) | ((bal + Registers.X) & 0xff)));
                                // T4
                                var ad = (ushort)((bah << 8) + bal + Registers.X);
                                var data = memory.Read(ad);
                                // T5
                                memory.Write(ad, data);
                                // T6
                                opCodeDefinition.Execute(this, ad, data);

                                Registers.ProgramCounter += 3;
                            }
                            break;

                        default:
                            throw new UnhandledAddressingModeException(Registers.ProgramCounter, operation, opCodeDefinition.OpCode, opCodeDefinition.AddressingMode);
                    }
                    break;

                // A. 5.4. Break Operation -- (Hardware Interrupt)-BRK (7 Cycles)
                case OpCode.BRK:
                    {
                        // T1
                        /* var discarded = */
                        memory.Read((ushort)(Registers.ProgramCounter + 1));
                        // T2
                        StackPush((byte)(((Registers.ProgramCounter + 2) & 0xff00) >> 8));
                        // T3
                        StackPush((byte)((Registers.ProgramCounter + 2) & 0x00ff));
                        // T4
                        StackPush((byte)(Registers.ProcessorStatus | (byte)ProcessorStatusBit.BreakCommand));
                        // T5
                        var adl = memory.Read(IrqVectorL);
                        // T6
                        var adh = memory.Read(IrqVectorH);

                        opCodeDefinition.Execute(this, RegisterMath.MakeShort(adh, adl), 0);
                    }
                    break;

                // A. 5.6. Jump Operation -- JMP
                case OpCode.JMP:
                    switch (opCodeDefinition.AddressingMode)
                    {
                        // A. 5.6.1 Absolute Addressing Mode (3 Cycles)
                        case AddressingMode.Absolute:
                            {
                                // T1
                                var adl = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var adh = memory.Read((ushort)(Registers.ProgramCounter + 2));
                                var ad = (ushort)((adh << 8) | adl);

                                opCodeDefinition.Execute(this, ad, 0);
                            }
                            break;

                        // A. 5.6.2 Indirect Addressing Mode (5 Cycles)
                        case AddressingMode.AbsoluteIndirect:
                            {
                                // T1
                                var ial = memory.Read((ushort)(Registers.ProgramCounter + 1));
                                // T2
                                var iah = memory.Read((ushort)(Registers.ProgramCounter + 2));
                                // T3
                                var ad = RegisterMath.MakeShort(iah, ial);
                                var adl = memory.Read(ad);
                                // T4
                                ad = RegisterMath.MakeShort(iah, (byte)((ial + 1) & 0xff));
                                var adh = memory.Read(ad);

                                opCodeDefinition.Execute(this, RegisterMath.MakeShort(adh, adl), 0);
                            }
                            break;

                        default:
                            throw new UnhandledAddressingModeException(Registers.ProgramCounter, operation, opCodeDefinition.OpCode, opCodeDefinition.AddressingMode);
                    }
                    break;

                // A. 5.3. Jump to Subroutine -- JSR (6 Cycles)
                case OpCode.JSR:
                    {
                        // T1
                        var adl = memory.Read((ushort)(Registers.ProgramCounter + 1));
                        // T2
                        /* var discarded = */
                        memory.Read((ushort)(StackBase + Registers.StackPointer));
                        // T3
                        StackPush((byte)(((Registers.ProgramCounter + 2) & 0xff00) >> 8));
                        // T4
                        StackPush((byte)((Registers.ProgramCounter + 2) & 0x00ff));
                        // T5
                        var adh = memory.Read((ushort)(Registers.ProgramCounter + 2));

                        opCodeDefinition.Execute(this, RegisterMath.MakeShort(adh, adl), 0);
                    }
                    break;

                // A. 5.1. Push Operations -- PHP, PHA (3 Cycles)
                case OpCode.PHA:
                case OpCode.PHP:
                    {
                        // T1
                        /* var discarded = */
                        memory.Read((ushort)(Registers.ProgramCounter + 1));
                        // T2
                        opCodeDefinition.Execute(this, 0, 0);

                        Registers.ProgramCounter++;
                    }
                    break;

                // A. 5.2. Pull Operations -- PLP, PLA (4 Cycles)
                case OpCode.PLA:
                case OpCode.PLP:
                    {
                        // T1
                        /* var discarded = */
                        memory.Read((ushort)(Registers.ProgramCounter + 1));
                        // T2
                        /* var discarded = */
                        memory.Read((ushort)(StackBase + Registers.StackPointer));
                        // T3
                        opCodeDefinition.Execute(this, 0, 0);

                        Registers.ProgramCounter++;
                    }
                    break;

                // A. 5.5. Return from Interrupt -- RTI (6 Cycles)
                case OpCode.RTI:
                    {
                        // T1
                        /* var discarded = */
                        memory.Read((ushort)(Registers.ProgramCounter + 1));
                        // T2
                        memory.Read((ushort)(StackBase + Registers.StackPointer));
                        // T3 - T5
                        opCodeDefinition.Execute(this, 0, 0);
                    }
                    break;

                // A. 5.7. Return from Subroutine -- RTS (6 Cycles)
                case OpCode.RTS:
                    {
                        // T1
                        memory.Read((ushort)(Registers.ProgramCounter + 1));
                        // T2
                        memory.Read((ushort)(StackBase + Registers.StackPointer));
                        // T3 - T5
                        opCodeDefinition.Execute(this, 0, 0);
                    }
                    break;

                // A. 5.8. Branch Operation -- BCC, BCS, BEQ, BMI, BNE, BPL, BVC, BVS (2, 3, or 4 Cycles)
                case OpCode.BRA:

                case OpCode.BBR0:
                case OpCode.BBR1:
                case OpCode.BBR2:
                case OpCode.BBR3:
                case OpCode.BBR4:
                case OpCode.BBR5:
                case OpCode.BBR6:
                case OpCode.BBR7:

                case OpCode.BBS0:
                case OpCode.BBS1:
                case OpCode.BBS2:
                case OpCode.BBS3:
                case OpCode.BBS4:
                case OpCode.BBS5:
                case OpCode.BBS6:
                case OpCode.BBS7:

                case OpCode.BCC:
                case OpCode.BCS:
                case OpCode.BEQ:
                case OpCode.BMI:
                case OpCode.BNE:
                case OpCode.BPL:
                case OpCode.BVC:
                case OpCode.BVS:
                    {
                        // T1
                        var offset = memory.Read((ushort)(Registers.ProgramCounter + 1));

                        // T2 - T3
                        var addr = (ushort)(Registers.ProgramCounter + 2 + ((sbyte)offset < 0 ? (sbyte)offset : offset));
                        opCodeDefinition.Execute(this, addr, offset);
                    }
                    break;

                default:
                    // this is unexpected...
                    throw new IllegalOpCodeException(Registers.ProgramCounter, operation);
            }
        }

    }
}