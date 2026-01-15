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

        public byte KeyLatch { get; set; }
    }
}
