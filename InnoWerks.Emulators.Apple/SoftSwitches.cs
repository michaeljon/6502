using System;
using System.Collections.Generic;
using System.Linq;

namespace InnoWerks.Emulators.Apple
{
    public class SoftSwitches
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public SoftSwitches()
        {
            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v.ToString()))
            {
                State[sw] = false;
            }
        }

        /// <summary>
        /// Used to hold the most recent keyboard entry
        /// </summary>
        public byte KeyLatch { get; set; }

        /// <summary>
        /// Used to hold the most recent keyboard entry
        /// </summary>
        public bool KeyStrobe { get; set; }

        /// <summary>
        /// Simple handler to tell whether the language card / bsr
        /// is current read or write enabled.
        /// </summary>
        public bool LcActive =>
            State[SoftSwitch.LcReadEnabled] || State[SoftSwitch.LcWriteEnabled];

        public byte AuxReadAuxWriteBitmask
        {
            get
            {
                if (State[SoftSwitch.AuxRead] == false && State[SoftSwitch.AuxWrite] == false)
                {
                    return 0x00;
                }
                if (State[SoftSwitch.AuxRead] == false && State[SoftSwitch.AuxWrite] == true)
                {
                    return 0x01;
                }
                if (State[SoftSwitch.AuxRead] == true && State[SoftSwitch.AuxWrite] == false)
                {
                    return 0x10;
                }
                if (State[SoftSwitch.AuxRead] == true && State[SoftSwitch.AuxWrite] == true)
                {
                    return 0x11;
                }

                // this is really a bad case, the compiler can't tell we are unreachable
                return 0xff;
            }
        }
    }
}
