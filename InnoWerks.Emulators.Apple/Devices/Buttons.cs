using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Buttons : IDevice, ISoftSwitchStateProvider
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => $"Buttons";

        public bool Handles(ushort address)
            => address >= SoftSwitchAddress.BUTTON0 && address <= SoftSwitchAddress.BUTTON2;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"HandlePaddle({address:X4})\n");

            return address switch
            {
                SoftSwitchAddress.BUTTON0 => (byte)(State[SoftSwitch.Button0] ? 0x80 : 0x00),
                SoftSwitchAddress.BUTTON1 => (byte)(State[SoftSwitch.Button1] ? 0x80 : 0x00),
                SoftSwitchAddress.BUTTON2 => (byte)(State[SoftSwitch.Button2] ? 0x80 : 0x00),
                _ => 0x00,
            };
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandlePaddle({address:X4}, {value:X2})\n");
        }

        public void Reset()
        {
            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v))
            {
                State[sw] = false;
            }
        }
    }
}
