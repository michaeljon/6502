namespace InnoWerks.Simulators
{
    public partial class Cpu
    {
        private void SET_CARRY_ON_SHIFT(int x) { Carry = ((byte)x & 0x01) != 0; }
        private void SET_CARRY(bool v) { Carry = v; }
        private void SET_ZERO_FROM_VALUE(int x) { Zero = x == 0; }
        private void SET_ZERO(bool v) { Zero = v; }
        private void SET_INTERRUPT(bool x) { Interrupt = x; }
        private void SET_DECIMAL(bool x) { Decimal = x; }
        private void SET_OVERFLOW_FROM_A(int a, int b) { Overflow = ((A ^ (byte)a) & 0x80) != 0 && ((A ^ (byte)b) & 0x80) != 0; }
        private void SET_OVERFLOW_FROM_VALUE(int v) { Overflow = ((byte)v & 0x40) != 0; }
        private void SET_OVERFLOW(bool x) { Overflow = x; }
        private void SET_NEGATIVE_FROM_VALUE(int x) { Negative = ((byte)x & 0x80) != 0; }
        private void SET_NEGATIVE(bool v) { Negative = v; }

        private bool IF_CARRY() { return Carry; }
        private bool IF_ZERO() { return Zero; }
        private bool IF_INTERRUPT() { return Interrupt; }
        private bool IF_DECIMAL() { return Decimal; }
        private bool IF_OVERFLOW() { return Overflow; }
        private bool IF_NEGATIVE() { return Negative; }

        /// <summary>
        /// direct access to the carry flag in the processor status register
        /// </summary>
        public bool Carry
        {
            get => (ProcessorStatus & (byte)ProcessorStatusBit.Carry) != 0;

            private set
            {
                if (value == false)
                {
                    ProcessorStatus &= unchecked((byte)~ProcessorStatusBit.Carry);
                }
                else
                {
                    ProcessorStatus |= (byte)ProcessorStatusBit.Carry;
                }
            }
        }

        /// <summary>
        /// direct access to the zero flag in the processor status register
        /// </summary>
        public bool Zero
        {
            get => (ProcessorStatus & (byte)ProcessorStatusBit.Zero) != 0;

            private set
            {
                if (value == false)
                {
                    ProcessorStatus &= unchecked((byte)~ProcessorStatusBit.Zero);
                }
                else
                {
                    ProcessorStatus |= (byte)ProcessorStatusBit.Zero;
                }
            }
        }

        /// <summary>
        /// direct access to the interrupt flag in the processor status register
        /// </summary>
        public bool Interrupt
        {
            get => (ProcessorStatus & (byte)ProcessorStatusBit.InterruptDisable) != 0;

            private set
            {
                if (value == false)
                {
                    ProcessorStatus &= unchecked((byte)~ProcessorStatusBit.InterruptDisable);
                }
                else
                {
                    ProcessorStatus |= (byte)ProcessorStatusBit.InterruptDisable;
                }
            }
        }

        /// <summary>
        /// direct access to the decimal flag in the processor status register
        /// </summary>
#pragma warning disable CA1720
        public bool Decimal
        {
            get => (ProcessorStatus & (byte)ProcessorStatusBit.DecimalMode) != 0;

            private set
            {
                if (value == false)
                {
                    ProcessorStatus &= unchecked((byte)~ProcessorStatusBit.DecimalMode);
                }
                else
                {
                    ProcessorStatus |= (byte)ProcessorStatusBit.DecimalMode;
                }
            }
        }

        /// <summary>
        /// direct access to the BRK flag in the processor status register
        /// </summary>
        public bool Break
        {
            get => (ProcessorStatus & (byte)ProcessorStatusBit.BreakCommand) != 0;

            private set
            {
                if (value == false)
                {
                    ProcessorStatus &= unchecked((byte)~ProcessorStatusBit.BreakCommand);
                }
                else
                {
                    ProcessorStatus |= (byte)ProcessorStatusBit.BreakCommand;
                }
            }
        }

        /// <summary>
        /// direct access to the overflow flag in the processor status register
        /// </summary>
        public bool Overflow
        {
            get => (ProcessorStatus & (byte)ProcessorStatusBit.Overflow) != 0;

            private set
            {
                if (value == false)
                {
                    ProcessorStatus &= unchecked((byte)~ProcessorStatusBit.Overflow);
                }
                else
                {
                    ProcessorStatus |= (byte)ProcessorStatusBit.Overflow;
                }
            }
        }

        /// <summary>
        /// direct access to the negative flag in the processor status register
        /// </summary>
        public bool Negative
        {
            get => (ProcessorStatus & (byte)ProcessorStatusBit.Negative) != 0;

            private set
            {
                if (value == false)
                {
                    ProcessorStatus &= unchecked((byte)~ProcessorStatusBit.Negative);
                }
                else
                {
                    ProcessorStatus |= (byte)ProcessorStatusBit.Negative;
                }
            }
        }
    }
}
