using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Memory : IDevice
    {
        private readonly List<ushort> handlesRead =
        [
            SoftSwitchAddress.RDLCBNK2,
            SoftSwitchAddress.RDLCRAM,

            SoftSwitchAddress.RDRAMRD,
            SoftSwitchAddress.RDRAMWRT,

            SoftSwitchAddress.RDALTSTKZP,

            0xC080, 0xC081, 0xC082, 0xC083, 0xC084, 0xC085, 0xC086, 0xC087,
            0xC088, 0xC089, 0xC08A, 0xC08B, 0xC08C, 0xC08D, 0xC08E, 0xC08F,
        ];

        private readonly List<ushort> handlesWrite =
        [
            SoftSwitchAddress.RDMAINRAM,
            SoftSwitchAddress.RDCARDRAM,

            SoftSwitchAddress.WRMAINRAM,
            SoftSwitchAddress.WRCARDRAM,

            SoftSwitchAddress.CLRALSTKZP,
            SoftSwitchAddress.SETALTSTKZP,

            0xC080, 0xC081, 0xC082, 0xC083, 0xC084, 0xC085, 0xC086, 0xC087,
            0xC088, 0xC089, 0xC08A, 0xC08B, 0xC08C, 0xC08D, 0xC08E, 0xC08F,
        ];


        private const ushort LANG_A3 = 0b00001000;

        private const ushort LANG_A0A1 = 0b00000011;

        private const ushort LANG_A0 = 0b00000001;

        private int preWrite;

        private readonly SoftSwitches softSwitches;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => $"Memory Switches";

        public Memory(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool HandlesRead(ushort address) =>
            handlesRead.Contains(address);

        public bool HandlesWrite(ushort address) =>
            handlesWrite.Contains(address);

        public byte Read(ushort address)
        {
            SimDebugger.Info($"Read Memory Switch({address:X4}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            switch (address)
            {
                case SoftSwitchAddress.RDLCBNK2: return (byte)(softSwitches.State[SoftSwitch.LcBank1] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDLCRAM: return (byte)(softSwitches.State[SoftSwitch.LcReadEnabled] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDRAMRD: return (byte)(softSwitches.State[SoftSwitch.AuxRead] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDRAMWRT: return (byte)(softSwitches.State[SoftSwitch.AuxWrite] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDALTSTKZP: return (byte)(softSwitches.State[SoftSwitch.ZpAux] ? 0x80 : 0x00);
            }

            if (address >= 0xC080 && address <= 0xC08F)
            {
                HandleReadC08x(address);
                return 0x00;
            }

            throw new ArgumentOutOfRangeException(nameof(address), $"Read {address:X4} is not supported in this device");
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"Write Memory Switch({address:X4}, {value:X2}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            switch (address)
            {
                case SoftSwitchAddress.RDMAINRAM: softSwitches.State[SoftSwitch.AuxRead] = false; return;
                case SoftSwitchAddress.RDCARDRAM: softSwitches.State[SoftSwitch.AuxRead] = true; return;

                case SoftSwitchAddress.WRMAINRAM: softSwitches.State[SoftSwitch.AuxWrite] = false; return;
                case SoftSwitchAddress.WRCARDRAM: softSwitches.State[SoftSwitch.AuxWrite] = true; return;

                case SoftSwitchAddress.CLRALSTKZP: softSwitches.State[SoftSwitch.ZpAux] = false; return;
                case SoftSwitchAddress.SETALTSTKZP: softSwitches.State[SoftSwitch.ZpAux] = true; return;
            }

            if (0xC080 <= address && address <= 0xC08F)
            {
                HandleWriteC08x(address, value);
                return;
            }

            throw new ArgumentOutOfRangeException(nameof(address), $"Write {address:X4} is not supported in this device");
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
        }

        private void HandleReadC08x(ushort address)
        {
            // Bank select
            softSwitches.State[SoftSwitch.LcBank1] = (address & LANG_A3) != 0;

            // Read enable
            int low = address & LANG_A0A1;
            softSwitches.State[SoftSwitch.LcReadEnabled] = (low == 0 || low == 3);

            // Write enable sequencing (critical)
            if ((address & LANG_A0) == 1)
            {
                if (preWrite == 1)
                    softSwitches.State[SoftSwitch.LcWriteEnabled] = true;
                else
                    preWrite = 1;
            }
            else
            {
                preWrite = 0;
                softSwitches.State[SoftSwitch.LcWriteEnabled] = false;
            }
        }

        private void HandleWriteC08x(ushort address, byte value)
        {
            preWrite = 0;

            // Writes to C08x do NOT affect LC state on real hardware
        }
    }
}
