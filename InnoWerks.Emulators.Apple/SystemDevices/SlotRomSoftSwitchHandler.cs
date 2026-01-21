using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class SlotRomSoftSwitchHandler : IDevice
    {
        private readonly List<ushort> handles =
        [
            SoftSwitchAddress.SETSLOTCXROM,
            SoftSwitchAddress.SETINTCXROM,
            SoftSwitchAddress.RDCXROM,

            SoftSwitchAddress.SETINTC3ROM,
            SoftSwitchAddress.SETSLOTC3ROM,
            SoftSwitchAddress.RDC3ROM,
        ];

        private readonly SoftSwitches softSwitches;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => $"Internal Slot Rom Handler";

        public SlotRomSoftSwitchHandler(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool HandlesRead(ushort address)
            => handles.Contains(address);

        public bool HandlesWrite(ushort address)
            => handles.Contains(address);

        public byte Read(ushort address)
        {
            SimDebugger.Info($"Read Internal Slot Rom SoftSwitch Handler({address:X4}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            // this block, if the address is handled, short-circuit returns
            return address switch
            {
                SoftSwitchAddress.RDCXROM => (byte)(softSwitches.State[SoftSwitch.SlotRomEnabled] ? 0x80 : 0x00),
                SoftSwitchAddress.RDC3ROM => (byte)(softSwitches.State[SoftSwitch.Slot3RomEnabled] ? 0x80 : 0x00),

                _ =>
                    throw new ArgumentOutOfRangeException(nameof(address), $"Read {address:X4} is not supported in this device")
            };
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"Write Internal Slot Rom SoftSwitch Handler({address:X4}, {value:X2}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            switch (address)
            {
                case SoftSwitchAddress.SETSLOTCXROM: softSwitches.State[SoftSwitch.SlotRomEnabled] = true; return;
                case SoftSwitchAddress.SETINTCXROM: softSwitches.State[SoftSwitch.SlotRomEnabled] = false; return;

                case SoftSwitchAddress.SETINTC3ROM: softSwitches.State[SoftSwitch.Slot3RomEnabled] = false; return;
                case SoftSwitchAddress.SETSLOTC3ROM: softSwitches.State[SoftSwitch.Slot3RomEnabled] = true; return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(address), $"Write {address:X4} is not supported in this device");
            }
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
        }
    }
}
