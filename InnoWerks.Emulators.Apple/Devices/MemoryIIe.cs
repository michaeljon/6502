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
            SoftSwitchAddress.RDMAINRAM,
            SoftSwitchAddress.RDCARDRAM,

            SoftSwitchAddress.WRMAINRAM,
            SoftSwitchAddress.WRCARDRAM,

            SoftSwitchAddress.SETSTDZP,
            SoftSwitchAddress.SETALTZP,

            SoftSwitchAddress.RDRAMRD,
            SoftSwitchAddress.RDRAMWRT,

            SoftSwitchAddress.RDALTZP,

            SoftSwitchAddress.RDLCBNK2,
            SoftSwitchAddress.RDLCRAM,

            0xC080, 0xC081, 0xC082, 0xC083, 0xC084, 0xC085, 0xC086, 0xC087,
            0xC088, 0xC089, 0xC08A, 0xC08B, 0xC08C, 0xC08D, 0xC08E, 0xC08F,
        ];

        private const ushort LANG_A3 = 0b00001000;

        private const ushort LANG_A0A1 = 0b00000011;

        private const ushort LANG_A0 = 0b00000001;

        private readonly AppleConfiguration configuration;

        private readonly SoftSwitches softSwitches;

        // main and auxiliary ram
        private readonly byte[] mainRam;

        private readonly byte[] auxRam;          // IIe only

        private readonly byte[][] lcRam;          // IIe only

        // swappable lo rom banks
        private readonly byte[] loRom;           // $D000–$DFFF

        // switch-selectable
        private readonly byte[] cxRom;           // $C000-$CFFF

        // single hi rom bank
        private readonly byte[] hiRom;           // $E000–$FFFF

        private int preWrite;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Memory IIe";

        public MemoryIIe(AppleConfiguration configuration, SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(softSwitches);

            this.configuration = configuration;
            this.softSwitches = softSwitches;

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
            loRom = new byte[4 * 1024];             // 4k ROM bank

            cxRom = new byte[4 * 1024];             // 4k switch selectable

            hiRom = new byte[8 * 1024];             // 8k ROM
        }

        public bool Handles(ushort address) =>
            handles.Contains(address);

        public byte Read(ushort address)
        {
            if (address < 0x0200)
            {
                return softSwitches.State[SoftSwitch.ZpAux] ? auxRam[address] : mainRam[address];
            }
            else if (address < 0xC000)
            {
                return softSwitches.State[SoftSwitch.AuxRead] ? auxRam[address] : mainRam[address];
            }
            else if (0xC000 <= address && address <= 0xC0FF)
            {
                SimDebugger.Info($"Read Memory({address:X4})\n");

                switch (address)
                {
                    case SoftSwitchAddress.RDLCBNK2: return (byte)(softSwitches.State[SoftSwitch.LcBank1] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDLCRAM: return (byte)(softSwitches.State[SoftSwitch.LcReadEnabled] ? 0x80 : 0x00);

                    case SoftSwitchAddress.RDRAMRD: return (byte)(softSwitches.State[SoftSwitch.AuxRead] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDRAMWRT: return (byte)(softSwitches.State[SoftSwitch.AuxWrite] ? 0x80 : 0x00);

                    case SoftSwitchAddress.RDALTZP: return (byte)(softSwitches.State[SoftSwitch.ZpAux] ? 0x80 : 0x00);
                }

                if (address >= 0xC080 && address <= 0xC08F)
                {
                    HandleReadC08x(address);
                }
            }
            else if (0xC100 <= address && address <= 0xCFFF)
            {
                if (softSwitches.State[SoftSwitch.AuxRead] == true)
                {
                    return auxRam[address];
                }

                int offset = address - 0xC000;
                return cxRom[offset];
            }
            else if (0xD000 <= address && address <= 0xDFFF)
            {
                int offset = address - 0xD000;
                if (softSwitches.State[SoftSwitch.LcReadEnabled] == true)
                {
                    return lcRam[softSwitches.State[SoftSwitch.LcBank1] ? 1 : 0][offset];
                }

                if (softSwitches.State[SoftSwitch.AuxRead] == true)
                {
                    return auxRam[address];
                }

                return loRom[offset];
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
                if (softSwitches.State[SoftSwitch.ZpAux] == true)
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
                if (configuration.Model == AppleModel.AppleIIe && softSwitches.State[SoftSwitch.AuxWrite])
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
                SimDebugger.Info($"Write Memory({address:X4}, {value:X2})\n");

                switch (address)
                {
                    case SoftSwitchAddress.RDMAINRAM: softSwitches.State[SoftSwitch.AuxRead] = false; return;
                    case SoftSwitchAddress.RDCARDRAM: softSwitches.State[SoftSwitch.AuxRead] = true; return;

                    case SoftSwitchAddress.WRMAINRAM: softSwitches.State[SoftSwitch.AuxWrite] = false; return;
                    case SoftSwitchAddress.WRCARDRAM: softSwitches.State[SoftSwitch.AuxWrite] = true; return;

                    case SoftSwitchAddress.SETSTDZP: softSwitches.State[SoftSwitch.ZpAux] = false; return;
                    case SoftSwitchAddress.SETALTZP: softSwitches.State[SoftSwitch.ZpAux] = true; return;
                }

                if (0xC080 <= address && address <= 0xC08F)
                {
                    HandleWriteC08x(address, value);
                }

                return;
            }
            else if (0xC100 <= address && address <= 0xCFFF)
            {
                if (softSwitches.State[SoftSwitch.AuxWrite] == true)
                {
                    auxRam[address] = value;
                }
            }
            else if (0xD000 <= address && address <= 0xDFFF)
            {
                if (softSwitches.State[SoftSwitch.LcWriteEnabled] == true)
                {
                    lcRam[softSwitches.State[SoftSwitch.LcBank1] ? 1 : 0][address - 0xD000] = value;
                }
                else if (softSwitches.State[SoftSwitch.AuxWrite] == true)
                {
                    auxRam[address] = value;
                }
            }
            else if (0xE000 <= address && address <= 0xFFFF)
            {
                if (softSwitches.State[SoftSwitch.AuxWrite] == true)
                {
                    auxRam[address] = value;
                }
                else
                {
                    mainRam[address] = value;
                }
            }
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset() { }

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
            Array.Copy(objectCode, 20 * 1024, loRom, 0, 4 * 1024);

            // load the remaining 12k from the 16k block into hi rom
            Array.Copy(objectCode, 24 * 1024, hiRom, 0, 8 * 1024);
        }

        public void LoadProgramToRam(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            Array.Copy(objectCode, 0, mainRam, origin, objectCode.Length);
        }

        private void HandleReadC08x(ushort address)
        {
            SimDebugger.Info($"Read Memory({address:X4})\n");

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
            SimDebugger.Info($"Write Memory({address:X4}, {value:X2})\n");

            preWrite = 0;

            // Writes to C08x do NOT affect LC state on real hardware

            // // If CPU is currently executing from LC RAM, ignore LC control changes
            // if (cpu.Registers.ProgramCounter >= 0xD000 && cpu.Registers.ProgramCounter <= 0xFFFF && softSwitches.State[SoftSwitch.LcWriteEnabled])
            // {
            //     return;
            // }

            // // Bank select
            // softSwitches.State[SoftSwitch.LcBank1] = (address & LANG_A3) != 0;

            // // Read enable
            // int low = address & LANG_A0A1;
            // softSwitches.State[SoftSwitch.LcReadEnabled] = (low == 0 || low == 3);

            // // Any write clears write capability
            // preWrite = 0;
            // softSwitches.State[SoftSwitch.LcWriteEnabled] = false;
        }
    }
}
