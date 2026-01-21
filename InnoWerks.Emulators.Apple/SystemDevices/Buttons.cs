using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Buttons : IDevice
    {
        private readonly SoftSwitches softSwitches;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => $"Buttons";

        private readonly List<ushort> handlesRead =
        [
            // II/II+ only, on IIe these are keyboard modifiers
            // SoftSwitchAddress.BUTTON0,
            // SoftSwitchAddress.BUTTON1,

            SoftSwitchAddress.BUTTON2,
        ];

        private readonly List<ushort> handlesWrite =
        [
        ];

        public Buttons(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool HandlesRead(ushort address)
            => handlesRead.Contains(address);

        public bool HandlesWrite(ushort address)
            => handlesWrite.Contains(address);

        public byte Read(ushort address)
        {
            SimDebugger.Info($"Read Button({address:X4}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            return address switch
            {
                SoftSwitchAddress.BUTTON0 => (byte)(softSwitches.State[SoftSwitch.Button0] ? 0x80 : 0x00),
                SoftSwitchAddress.BUTTON1 => (byte)(softSwitches.State[SoftSwitch.Button1] ? 0x80 : 0x00),
                SoftSwitchAddress.BUTTON2 => (byte)(softSwitches.State[SoftSwitch.Button2] ? 0x80 : 0x00),

                _ =>
                    throw new ArgumentOutOfRangeException(nameof(address), $"Read {address:X4} is not supported in this device")
            };
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"Write Button({address:X4}, {value:X2}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            throw new ArgumentOutOfRangeException(nameof(address), $"Write {address:X4} is not supported in this device");
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
            softSwitches.State[SoftSwitch.Button0] = false;
            softSwitches.State[SoftSwitch.Button1] = false;
            softSwitches.State[SoftSwitch.Button2] = false;
        }
    }
}
