using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public abstract class AppleSoftSwitchesBase : IDevice, ISoftSwitchStateProvider
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public abstract string Name { get; }

        public bool Handles(ushort address)
            => address >= 0xC000 && address <= 0xC0FF;

        public abstract byte Read(ushort address);

        public abstract void Write(ushort address, byte value);

        public abstract void Reset();

        protected void WarmBoot()
        {
            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v))
            {
                State[sw] = false;
            }

            // route $C8XX-CFFF to the slots
            State[SoftSwitch.IntC8RomEnabled] = true;
        }
    }
}
