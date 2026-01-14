using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class MemoryIIe : IDevice
    {
        // private readonly List<ushort> handles =
        // [
        //     // read
        //     SoftSwitchAddress.RDLCBNK2,
        //     SoftSwitchAddress.RDLCRAM,

        //     SoftSwitchAddress.RDRAMRD,
        //     SoftSwitchAddress.RDRAMWRT,

        //     SoftSwitchAddress.RDCXROM,
        //     SoftSwitchAddress.RDALTZP,
        //     SoftSwitchAddress.RDC3ROM,

        //     SoftSwitchAddress.RD80STORE,
        //     SoftSwitchAddress.RDTEXT,
        //     SoftSwitchAddress.RDMIXED,
        //     SoftSwitchAddress.RDPAGE2,
        //     SoftSwitchAddress.RDLCBNK2,

        //     // write
        //     SoftSwitchAddress.CLR80STORE,
        //     SoftSwitchAddress.SET80STORE,

        //     SoftSwitchAddress.RDMAINRAM,
        //     SoftSwitchAddress.RDCARDRAM,

        //     SoftSwitchAddress.WRMAINRAM,
        //     SoftSwitchAddress.WRCARDRAM,

        //     SoftSwitchAddress.SETSTDZP,
        //     SoftSwitchAddress.SETALTZP,
        // ];

        private readonly AppleConfiguration configuration;

        // main and auxiliary ram
        private readonly byte[] mainRam;

        private readonly byte[] auxRam;          // IIe only

        // swappable lo rom banks
        private readonly byte[][] loRom;         // $D000–$DFFF

        // switch-selectable
        private readonly byte[] cxRom;           // $C000-$CFFF

        // single hi rom bank
        private readonly byte[] hiRom;           // $E000–$FFFF

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
            }

            // todo: come back around and replace this per configuration
            loRom = new byte[2][];
            loRom[0] = new byte[4 * 1024];          // 4k ROM bank 1
            loRom[1] = new byte[4 * 1024];          // 4k ROM bank 2

            cxRom = new byte[4 * 1024];             // 4k switch selectable
            hiRom = new byte[8 * 1024];             // 8k ROM
        }

        public bool Handles(ushort address) => true;

        public byte Read(ushort address)
        {
            // RAM
            if (address < 0xC000)
            {
                if (configuration.Model == AppleModel.AppleIIe && State[SoftSwitch.AuxRead])
                {
                    return auxRam[address];
                }
                else
                {
                    return mainRam[address];
                }
            }

            // soft switches
            if (0xC000 <= address && address <= 0xC0FF)
            {
                switch (address)
                {
                    case SoftSwitchAddress.RDLCBNK2: return (byte)(State[SoftSwitch.LcBank2] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDLCRAM: return (byte)(State[SoftSwitch.LcWriteEnabled] ? 0x80 : 0x00);

                    case SoftSwitchAddress.RDRAMRD: return (byte)(State[SoftSwitch.AuxRead] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDRAMWRT: return (byte)(State[SoftSwitch.AuxWrite] ? 0x80 : 0x00);

                    case SoftSwitchAddress.RDALTZP: return (byte)(State[SoftSwitch.ZpAux] ? 0x80 : 0x00);

                    case SoftSwitchAddress.RD80STORE: return (byte)(State[SoftSwitch.Store80] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDTEXT: return (byte)(State[SoftSwitch.TextMode] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDMIXED: return (byte)(State[SoftSwitch.MixedMode] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDPAGE2: return (byte)(State[SoftSwitch.Page2] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDHIRES: return (byte)(State[SoftSwitch.HiRes] ? 0x80 : 0x00);
                }
            }

            // $C100-$CFFF was handled by the bus, if slot rom is enabled,
            // otherwise we're being asked to handle it
            if (0xC000 <= address && address <= 0xCFFF)
            {
                int offset = address - 0xC000;
                return cxRom[offset];
            }
            else if (0xD000 <= address && address <= 0xDFFF)
            {
                int offset = address - 0xD000;
                return loRom[State[SoftSwitch.AuxRead] ? 1 : 0][offset];
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
            if (address < 0xC000)
            {
                if (configuration.Model == AppleModel.AppleIIe && State[SoftSwitch.AuxWrite])
                {
                    auxRam[address] = value;
                    return;
                }
                else
                {
                    mainRam[address] = value;
                    return;
                }
            }

            if (0xC000 <= address && address <= 0xC0FF)
            {
                switch (address)
                {
                    case SoftSwitchAddress.CLR80STORE: State[SoftSwitch.Store80] = false; break;
                    case SoftSwitchAddress.SET80STORE: State[SoftSwitch.Store80] = true; break;

                    case SoftSwitchAddress.RDMAINRAM: State[SoftSwitch.AuxRead] = false; break;
                    case SoftSwitchAddress.RDCARDRAM: State[SoftSwitch.AuxRead] = true; break;

                    case SoftSwitchAddress.WRMAINRAM: State[SoftSwitch.AuxWrite] = false; break;
                    case SoftSwitchAddress.WRCARDRAM: State[SoftSwitch.AuxWrite] = true; break;

                    case SoftSwitchAddress.SETSTDZP: State[SoftSwitch.ZpAux] = false; break;
                    case SoftSwitchAddress.SETALTZP: State[SoftSwitch.ZpAux] = true; break;
                }
            }

            // $D000–$FFFF ROM or RAM
            if (configuration.Model == AppleModel.AppleIIe)
            {
                // ROM write-through enabled (rare, but firmware does this)
                if (State[SoftSwitch.AuxWrite] == true)
                {
                    mainRam[address] = value;
                }
            }

            // Apple II / II+ ROM is always read-only
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
    }
}
