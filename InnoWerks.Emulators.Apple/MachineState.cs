using System;
using System.Collections.Generic;
using System.Linq;

namespace InnoWerks.Emulators.Apple
{
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
        public int CurrentSlot { get; set; }

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

        public (byte value, bool remapNeeded) HandleReadStateToggle(SoftSwitch softSwitch, bool toState, bool floating = false)
        {
            byte returnValue = 0x00;

            if (floating == true)
            {
#pragma warning disable CA5394 // Do not use insecure randomness
                returnValue = (byte)(rng.Next() & 0xFF);
#pragma warning restore CA5394 // Do not use insecure randomness
            }

            if (State[softSwitch] == toState)
            {
                return (returnValue, false);
            }

            State[softSwitch] = toState;

            return (returnValue, true);
        }

        public bool HandleWriteStateToggle(SoftSwitch softSwitch, bool toState)
        {
            if (State[softSwitch] == toState)
            {
                return false;
            }

            State[softSwitch] = toState;
            return true;
        }
    }
}
