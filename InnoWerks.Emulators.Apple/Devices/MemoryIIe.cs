using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class MemoryIIe : IDevice
    {
        private readonly List<ushort> handles =
        [
            SoftSwitchAddress.CLR80STORE,
            SoftSwitchAddress.SET80STORE,

            SoftSwitchAddress.RDMAINRAM,
            SoftSwitchAddress.RDCARDRAM,

            SoftSwitchAddress.WRMAINRAM,
            SoftSwitchAddress.WRCARDRAM,

            SoftSwitchAddress.SETSTDZP,
            SoftSwitchAddress.SETALTZP,

            SoftSwitchAddress.RDRAMRD,
            SoftSwitchAddress.RDRAMWRT,

            SoftSwitchAddress.RDALTZP,

            SoftSwitchAddress.RD80STORE,
            SoftSwitchAddress.RDLCBNK2,
            SoftSwitchAddress.RDLCRAM,

            0xC080, 0xC081, 0xC082, 0xC083, 0xC084, 0xC085, 0xC086, 0xC087,
            0xC088, 0xC089, 0xC08A, 0xC08B, 0xC08C, 0xC08D, 0xC08E, 0xC08F,
        ];

        private const ushort LANG_A3 = 0b00001000;

        private const ushort LANG_A0A1 = 0b00000011;

        private const ushort LANG_A0 = 0b00000001;

        private readonly AppleConfiguration configuration;

        private ICpu cpu;

        // main and auxiliary ram
        private readonly byte[] mainRam;

        private readonly byte[] auxRam;          // IIe only

        private readonly byte[][] lcRam;          // IIe only

        // swappable lo rom banks
        private readonly byte[][] loRom;         // $D000–$DFFF

        // switch-selectable
        private readonly byte[] cxRom;           // $C000-$CFFF

        // single hi rom bank
        private readonly byte[] hiRom;           // $E000–$FFFF

        private int preWrite;

        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Memory IIe";

        public MemoryIIe(AppleConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            this.configuration = configuration;

            Reset();

            mainRam = new byte[64 * 1024];

            if (configuration.Model == AppleModel.AppleIIe && configuration.HasAuxMemory)
            {
                auxRam = new byte[64 * 1024];
                lcRam = new byte[2][];
                lcRam[0] = new byte[4 * 1024];          // 4k RAM bank 1
                lcRam[1] = new byte[4 * 1024];          // 4k RAM bank 2
            }

            // todo: come back around and replace this per configuration
            loRom = new byte[2][];
            loRom[0] = new byte[4 * 1024];          // 4k ROM bank 1
            loRom[1] = new byte[4 * 1024];          // 4k ROM bank 2

            cxRom = new byte[4 * 1024];             // 4k switch selectable
            hiRom = new byte[8 * 1024];             // 8k ROM
        }

        public bool Handles(ushort address) =>
            handles.Contains(address);

        public byte Read(ushort address)
        {
            if (address < 0x0200)
            {
                return State[SoftSwitch.ZpAux] ? auxRam[address] : mainRam[address];
            }
            else if (address < 0xC000)
            {
                return State[SoftSwitch.AuxRead] ? auxRam[address] : mainRam[address];
            }
            else if (0xC000 <= address && address <= 0xC0FF)
            {
                switch (address)
                {
                    case SoftSwitchAddress.RDLCBNK2: return (byte)(State[SoftSwitch.LcBank1] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDLCRAM: return (byte)(State[SoftSwitch.LcReadEnabled] ? 0x80 : 0x00);

                    case SoftSwitchAddress.RDRAMRD: return (byte)(State[SoftSwitch.AuxRead] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDRAMWRT: return (byte)(State[SoftSwitch.AuxWrite] ? 0x80 : 0x00);

                    case SoftSwitchAddress.RDALTZP: return (byte)(State[SoftSwitch.ZpAux] ? 0x80 : 0x00);

                    case SoftSwitchAddress.RD80STORE: return (byte)(State[SoftSwitch.Store80] ? 0x80 : 0x00);
                }

                if (address >= 0xC080 && address <= 0xC08F)
                {
                    HandleReadC08x(address);
                }
            }
            else if (0xC100 <= address && address <= 0xCFFF)
            {
                if (State[SoftSwitch.AuxRead] == true)
                {
                    return auxRam[address];
                }

                int offset = address - 0xC000;
                return cxRom[offset];
            }
            else if (0xD000 <= address && address <= 0xDFFF)
            {
                int offset = address - 0xD000;
                if (State[SoftSwitch.LcReadEnabled] == true)
                {
                    return lcRam[State[SoftSwitch.LcBank1] ? 1 : 0][offset];
                }

                if (State[SoftSwitch.AuxRead] == true)
                {
                    return auxRam[address];
                }

                return loRom[State[SoftSwitch.LcBank1] ? 1 : 0][offset];
            }
            else if (0xE000 <= address && address <= 0xFFFF)
            {
                int offset = address - 0xE000;
                return hiRom[offset];
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            if (address < 0x0200)
            {
                if (State[SoftSwitch.ZpAux] == true)
                {
                    auxRam[address] = value;
                }
                else
                {
                    mainRam[address] = value;
                }
            }
            if (address < 0xC000)
            {
                if (configuration.Model == AppleModel.AppleIIe && State[SoftSwitch.AuxWrite])
                {
                    auxRam[address] = value;
                }
                else
                {
                    mainRam[address] = value;
                }
            }
            else if (0xC000 <= address && address <= 0xC0FF)
            {
                switch (address)
                {
                    case SoftSwitchAddress.CLR80STORE: State[SoftSwitch.Store80] = false; return;
                    case SoftSwitchAddress.SET80STORE: State[SoftSwitch.Store80] = true; return;

                    case SoftSwitchAddress.RDMAINRAM: State[SoftSwitch.AuxRead] = false; return;
                    case SoftSwitchAddress.RDCARDRAM: State[SoftSwitch.AuxRead] = true; return;

                    case SoftSwitchAddress.WRMAINRAM: State[SoftSwitch.AuxWrite] = false; return;
                    case SoftSwitchAddress.WRCARDRAM: State[SoftSwitch.AuxWrite] = true; return;

                    case SoftSwitchAddress.SETSTDZP: State[SoftSwitch.ZpAux] = false; return;
                    case SoftSwitchAddress.SETALTZP: State[SoftSwitch.ZpAux] = true; return;
                }

                if (0xC080 <= address && address <= 0xC08F)
                {
                    HandleWriteC08x(address, value);
                }

                return;
            }
            else if (0xC100 <= address && address <= 0xCFFF)
            {
                if (State[SoftSwitch.AuxWrite] == true)
                {
                    auxRam[address] = value;
                }
            }
            else if (0xD000 <= address && address <= 0xDFFF)
            {
                if (State[SoftSwitch.LcWriteEnabled] == true)
                {
                    lcRam[State[SoftSwitch.LcBank1] ? 1 : 0][address - 0xD000] = value;
                }
                else if (State[SoftSwitch.AuxWrite] == true)
                {
                    auxRam[address] = value;
                }
            }
            else if (0xE000 <= address && address <= 0xFFFF)
            {
                if (State[SoftSwitch.AuxWrite] == true)
                {
                    auxRam[address] = value;
                }
                else
                {
                    mainRam[address] = value;
                }
            }
        }

        public void Reset()
        {
            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v))
            {
                State[sw] = false;
            }
        }

        public void LoadProgramToRom(byte[] objectCode)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            // this is ugly, but it'll work for now
            if (configuration.Model != AppleModel.AppleIIe)
            {
                throw new NotImplementedException("ROM loading is only supported for IIe devices");
            }

            if (objectCode.Length != 32 * 1024)
            {
                throw new NotImplementedException("IIe ROM must be 32k");
            }

            // load the first 4k from the 16k block at the end into cx rom
            Array.Copy(objectCode, 16 * 1024, cxRom, 0, 4 * 1024);

            // load the first 4k from the 16k block at the end into lo rom
            Array.Copy(objectCode, 20 * 1024, loRom[0], 0, 4 * 1024);
            Array.Copy(objectCode, 20 * 1024, loRom[1], 0, 4 * 1024);

            // load the remaining 12k from the 16k block into hi rom
            Array.Copy(objectCode, 24 * 1024, hiRom, 0, 8 * 1024);
        }

        public void LoadProgramToRam(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            Array.Copy(objectCode, 0, mainRam, origin, objectCode.Length);
        }

        public void SetCpu(ICpu cpu)
        {
            ArgumentNullException.ThrowIfNull(cpu, nameof(cpu));

            this.cpu = cpu;
        }

        private void HandleReadC08x(ushort address)
        {
            SimDebugger.Info($"HandleReadC08x({address:X4})\n");

            // Bank select
            State[SoftSwitch.LcBank1] = (address & LANG_A3) != 0;

            // Read enable
            int low = address & LANG_A0A1;
            State[SoftSwitch.LcReadEnabled] = (low == 0 || low == 3);

            // Write enable sequencing (critical)
            if ((address & LANG_A0) == 1)
            {
                if (preWrite == 1)
                    State[SoftSwitch.LcWriteEnabled] = true;
                else
                    preWrite = 1;
            }
            else
            {
                preWrite = 0;
                State[SoftSwitch.LcWriteEnabled] = false;
            }
        }

        private void HandleWriteC08x(ushort address, byte value)
        {
            SimDebugger.Info($"HandleWriteC08x({address:X4}, {value:X2})\n");

            preWrite = 0;

            // Writes to C08x do NOT affect LC state on real hardware

            // // If CPU is currently executing from LC RAM, ignore LC control changes
            // if (cpu.Registers.ProgramCounter >= 0xD000 && cpu.Registers.ProgramCounter <= 0xFFFF && State[SoftSwitch.LcWriteEnabled])
            // {
            //     return;
            // }

            // // Bank select
            // State[SoftSwitch.LcBank1] = (address & LANG_A3) != 0;

            // // Read enable
            // int low = address & LANG_A0A1;
            // State[SoftSwitch.LcReadEnabled] = (low == 0 || low == 3);

            // // Any write clears write capability
            // preWrite = 0;
            // State[SoftSwitch.LcWriteEnabled] = false;
        }
    }
}
