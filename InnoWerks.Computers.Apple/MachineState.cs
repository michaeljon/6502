using System;
using System.Collections.Generic;
using System.Linq;

namespace InnoWerks.Computers.Apple
{
    public enum ExpansionRomType
    {
        ExpRomNull = 0,
        ExpRomInternal,
        ExpRomPeripheral
    };


    public class MachineState
    {
        private readonly Random rng = new();

        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public MachineState()
        {
            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v.ToString()))
            {
                State[sw] = false;
            }
        }

        // used to hold the current slot device, if present
        public int CurrentSlot
        {
            get;
            set;
        }

        public ExpansionRomType ExpansionRomType { get; set; }

        /// <summary>
        /// Used to hold the most recent keyboard entry
        /// </summary>
        public byte KeyLatch { get; set; }

        /// <summary>
        /// Used to hold the most recent keyboard entry
        /// </summary>
        public bool KeyStrobe
        {
            get
            {
                return State[SoftSwitch.KeyboardStrobe];
            }

            set
            {
                State[SoftSwitch.KeyboardStrobe] = value;
            }
        }

        /// <summary>
        /// Simple handler to tell whether the language card / bsr
        /// is current read or write enabled.
        /// </summary>
        public bool LcActive =>
            State[SoftSwitch.LcReadEnabled] || State[SoftSwitch.LcWriteEnabled];

#pragma warning disable CA5394 // Do not use insecure randomness
        public byte FloatingValue => (byte)(rng.Next() & 0xFF);
#pragma warning restore CA5394 // Do not use insecure randomness

        public byte HandleReadStateToggle(Memory128k memoryBlocks, SoftSwitch softSwitch, bool toState, bool floating = false)
        {
            ArgumentNullException.ThrowIfNull(memoryBlocks);

#pragma warning disable CA5394 // Do not use insecure randomness
            byte returnValue = floating == true ? (byte)(rng.Next() & 0xFF) : (byte)0x00;
#pragma warning restore CA5394 // Do not use insecure randomness

            if (State[softSwitch] == toState)
            {
                return returnValue;
            }

            State[softSwitch] = toState;
            memoryBlocks.Remap();

            return returnValue;
        }

        public void HandleWriteStateToggle(Memory128k memoryBlocks, SoftSwitch softSwitch, bool toState)
        {
            ArgumentNullException.ThrowIfNull(memoryBlocks);

            if (State[softSwitch] == toState)
            {
                return;
            }

            State[softSwitch] = toState;
            memoryBlocks.Remap();
        }
    }
}
