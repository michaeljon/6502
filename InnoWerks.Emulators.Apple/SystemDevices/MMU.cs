// #define DEBUG_C08X_HANDLER
#define DEBUG_WRITE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class MMU : IDevice
    {
        private int preWrite;

        private readonly IBus bus;

        private readonly MachineState machineState;

        private readonly List<ushort> handlesRead =
        [
            //
            // LANGUAGE CARD
            //
            SoftSwitchAddress.RDLCBNK2,
            SoftSwitchAddress.RDLCRAM,

            SoftSwitchAddress.RDRAMRD,
            SoftSwitchAddress.RDRAMWRT,
            SoftSwitchAddress.RDALTSTKZP,
            SoftSwitchAddress.RD80STORE,
            SoftSwitchAddress.RDPAGE2,
            SoftSwitchAddress.RDHIRES,
            SoftSwitchAddress.RDDHIRES,

            //
            // I/O BANKING
            //
            SoftSwitchAddress.RDCXROM,
            SoftSwitchAddress.RDC3ROM,

            0xC080, 0xC081, 0xC082, 0xC083, 0xC084, 0xC085, 0xC086, 0xC087,
            0xC088, 0xC089, 0xC08A, 0xC08B, 0xC08C, 0xC08D, 0xC08E, 0xC08F,
        ];

        private readonly List<ushort> handlesWrite =
        [
            //
            // LANGUAGE CARD
            //
            SoftSwitchAddress.CLR80STORE,
            SoftSwitchAddress.SET80STORE,
            SoftSwitchAddress.RDMAINRAM,
            SoftSwitchAddress.RDCARDRAM,
            SoftSwitchAddress.WRMAINRAM,
            SoftSwitchAddress.WRCARDRAM,
            SoftSwitchAddress.CLRALSTKZP,
            SoftSwitchAddress.SETALTSTKZP,

            //
            // I/O BANKING
            //
            SoftSwitchAddress.SETSLOTCXROM,
            SoftSwitchAddress.SETINTCXROM,
            SoftSwitchAddress.SETINTC3ROM,
            SoftSwitchAddress.SETSLOTC3ROM,

            0xC080, 0xC081, 0xC082, 0xC083, 0xC084, 0xC085, 0xC086, 0xC087,
            0xC088, 0xC089, 0xC08A, 0xC08B, 0xC08C, 0xC08D, 0xC08E, 0xC08F,
        ];

        private const ushort LANG_A3 = 0b00001000;

        private const ushort LANG_A0A1 = 0b00000011;

        private const ushort LANG_A0 = 0b00000001;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => $"MMU";

        public MMU(MachineState machineState, IBus bus)
        {
            ArgumentNullException.ThrowIfNull(machineState, nameof(machineState));
            ArgumentNullException.ThrowIfNull(bus, nameof(bus));

            this.machineState = machineState;
            this.bus = bus;

            bus.AddDevice(this);
        }

        public bool HandlesRead(ushort address)
            => handlesRead.Contains(address);

        public bool HandlesWrite(ushort address)
            => handlesWrite.Contains(address);

        public (byte value, bool remapNeeded) Read(ushort address)
        {
            if (address < 0xC080)
            {
                SimDebugger.Info($"Read MMU({address:X4}) [{SoftSwitchAddress.LookupAddress(address)}]\n");
            }

            switch (address)
            {
                //
                // LANGUAGE CARD
                //

                case SoftSwitchAddress.RDLCBNK2: return ((byte)(machineState.State[SoftSwitch.LcBank1] ? 0x80 : 0x00), false);
                case SoftSwitchAddress.RDLCRAM: return ((byte)(machineState.State[SoftSwitch.LcReadEnabled] ? 0x80 : 0x00), false);

                case SoftSwitchAddress.RDRAMRD: return ((byte)(machineState.State[SoftSwitch.AuxRead] ? 0x80 : 0x00), false);
                case SoftSwitchAddress.RDRAMWRT: return ((byte)(machineState.State[SoftSwitch.AuxWrite] ? 0x80 : 0x00), false);
                case SoftSwitchAddress.RDALTSTKZP: return ((byte)(machineState.State[SoftSwitch.ZpAux] ? 0x80 : 0x00), false);
                case SoftSwitchAddress.RD80STORE: return ((byte)(machineState.State[SoftSwitch.Store80] ? 0x80 : 0x00), false);
                case SoftSwitchAddress.RDPAGE2: return ((byte)(machineState.State[SoftSwitch.Page2] ? 0x80 : 0x00), false);
                case SoftSwitchAddress.RDHIRES: return ((byte)(machineState.State[SoftSwitch.HiRes] ? 0x80 : 0x00), false);
                case SoftSwitchAddress.RDDHIRES: return ((byte)(machineState.State[SoftSwitch.DoubleHiRes] ? 0x80 : 0x00), false);

                case SoftSwitchAddress.RDCXROM: return ((byte)(machineState.State[SoftSwitch.InternalRomEnabled] ? 0x80 : 0x00), false);
                case SoftSwitchAddress.RDC3ROM: return ((byte)(machineState.State[SoftSwitch.Slot3RomEnabled] ? 0x80 : 0x00), false);
            }

            if (address >= 0xC080 && address <= 0xC08F)
            {
                return (0x00, HandleReadC08x(address));
            }

            return (0x00, false);
        }

        public bool Write(ushort address, byte value)
        {
            if (address < 0xC080)
            {
                SimDebugger.Info($"Write MMU({address:X4}, {value:X2}) [{SoftSwitchAddress.LookupAddress(address)}]\n");
            }

            switch (address)
            {
                //
                // LANGUAGE CARD
                //
                case SoftSwitchAddress.CLR80STORE: return machineState.HandleWriteStateToggle(SoftSwitch.Store80, false);
                case SoftSwitchAddress.SET80STORE: return machineState.HandleWriteStateToggle(SoftSwitch.Store80, true);
                case SoftSwitchAddress.RDMAINRAM: return machineState.HandleWriteStateToggle(SoftSwitch.AuxRead, false);
                case SoftSwitchAddress.RDCARDRAM: return machineState.HandleWriteStateToggle(SoftSwitch.AuxRead, true);
                case SoftSwitchAddress.WRMAINRAM: return machineState.HandleWriteStateToggle(SoftSwitch.AuxWrite, false);
                case SoftSwitchAddress.WRCARDRAM: return machineState.HandleWriteStateToggle(SoftSwitch.AuxWrite, true);
                case SoftSwitchAddress.CLRALSTKZP: return machineState.HandleWriteStateToggle(SoftSwitch.ZpAux, false);
                case SoftSwitchAddress.SETALTSTKZP: return machineState.HandleWriteStateToggle(SoftSwitch.ZpAux, true);

                //
                // I/O BANKING
                //
                case SoftSwitchAddress.SETSLOTCXROM: return machineState.HandleWriteStateToggle(SoftSwitch.InternalRomEnabled, false);
                case SoftSwitchAddress.SETINTCXROM: return machineState.HandleWriteStateToggle(SoftSwitch.InternalRomEnabled, true);
                case SoftSwitchAddress.SETINTC3ROM: return machineState.HandleWriteStateToggle(SoftSwitch.Slot3RomEnabled, false);
                case SoftSwitchAddress.SETSLOTC3ROM: return machineState.HandleWriteStateToggle(SoftSwitch.Slot3RomEnabled, true);
            }

            if (0xC080 <= address && address <= 0xC08F)
            {
                return HandleWriteC08x(address, value);
            }

            throw new ArgumentOutOfRangeException(nameof(address), $"Write {address:X4} is not supported in this device");
        }

        public void Tick(int cycles) { /* NO-OP */ }

        public void Reset()
        {
            // bank 2 is the primary bank
            machineState.State[SoftSwitch.LcBank1] = false;
            machineState.State[SoftSwitch.InternalRomEnabled] = true;
        }

        private bool HandleReadC08x(ushort address)
        {
#if DEBUG_C08X_HANDLER
            var entryState = "";

            entryState += machineState.State[SoftSwitch.LcBank1] ? "b=1," : "b=2,";
            entryState += machineState.State[SoftSwitch.LcReadEnabled] ? "r=1," : "r=0,";
            entryState += machineState.State[SoftSwitch.LcWriteEnabled] ? "w=1," : "w=0,";
            entryState += $"p={preWrite}";
#endif

            var lcBank1 = machineState.State[SoftSwitch.LcBank1];
            var lcReadEnabled = machineState.State[SoftSwitch.LcReadEnabled];
            var lcWriteEnabled = machineState.State[SoftSwitch.LcWriteEnabled];

            // Bank select
            machineState.State[SoftSwitch.LcBank1] = (address & LANG_A3) != 0;

            // Read enable
            int low = address & LANG_A0A1;
            machineState.State[SoftSwitch.LcReadEnabled] = low == 0 || low == 3;

            // Write enable sequencing (critical)
            if ((address & LANG_A0) == 1)
            {
                if (preWrite == 1)
                    machineState.State[SoftSwitch.LcWriteEnabled] = true;
                else
                    preWrite = 1;
            }
            else
            {
                preWrite = 0;
                machineState.State[SoftSwitch.LcWriteEnabled] = false;
            }

#if DEBUG_C08X_HANDLER
            var exitState = "";

            exitState += machineState.State[SoftSwitch.LcBank1] ? "b=1," : "b=2,";
            exitState += machineState.State[SoftSwitch.LcReadEnabled] ? "r=1," : "r=0,";
            exitState += machineState.State[SoftSwitch.LcWriteEnabled] ? "w=1," : "w=0,";
            exitState += $"p={preWrite}";

            SimDebugger.Info($"Read MMU({address:X4}) entry: {entryState} exit: {exitState}\n");
#endif

            return lcBank1 != machineState.State[SoftSwitch.LcBank1] ||
                   lcReadEnabled != machineState.State[SoftSwitch.LcReadEnabled] ||
                   lcWriteEnabled != machineState.State[SoftSwitch.LcWriteEnabled];
        }
        private bool HandleWriteC08x(ushort address, byte value)
        {
#if DEBUG_C08X_HANDLER
            var entryState = "";

            entryState += machineState.State[SoftSwitch.LcBank1] ? "b=1," : "b=2,";
            entryState += machineState.State[SoftSwitch.LcReadEnabled] ? "r=1," : "r=0,";
            entryState += machineState.State[SoftSwitch.LcWriteEnabled] ? "w=1," : "w=0,";
            entryState += $"p={preWrite}";
#endif

            preWrite = 0;

            var lcBank1 = machineState.State[SoftSwitch.LcBank1];
            var lcReadEnabled = machineState.State[SoftSwitch.LcReadEnabled];
            var lcWriteEnabled = machineState.State[SoftSwitch.LcWriteEnabled];

            // Bank select
            machineState.State[SoftSwitch.LcBank1] = (address & LANG_A3) != 0;

            // Read enable
            int low = address & LANG_A0A1;
            machineState.State[SoftSwitch.LcReadEnabled] = low == 0 || low == 3;

#if DEBUG_C08X_HANDLER
            var exitState = "";

            exitState += machineState.State[SoftSwitch.LcBank1] ? "b=1," : "b=2,";
            exitState += machineState.State[SoftSwitch.LcReadEnabled] ? "r=1," : "r=0,";
            exitState += machineState.State[SoftSwitch.LcWriteEnabled] ? "w=1," : "w=0,";
            exitState += $"p={preWrite}";

            SimDebugger.Info($"Write MMU({address:X4}) entry: {entryState} exit: {exitState}\n");
#endif

            return lcBank1 != machineState.State[SoftSwitch.LcBank1] ||
                   lcReadEnabled != machineState.State[SoftSwitch.LcReadEnabled] ||
                   lcWriteEnabled != machineState.State[SoftSwitch.LcWriteEnabled];
        }
    }
}
