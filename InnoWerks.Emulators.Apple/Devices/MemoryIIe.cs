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
        private const ushort LANG_A3 = 0b00001000;
        private const ushort LANG_A0A1 = 0b00000011;

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
                    case SoftSwitchAddress.RDRAMRD: return (byte)(State[SoftSwitch.AuxRead] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDRAMWRT: return (byte)(State[SoftSwitch.AuxWrite] ? 0x80 : 0x00);

                    case SoftSwitchAddress.RDALTZP: return (byte)(State[SoftSwitch.ZpAux] ? 0x80 : 0x00);

                    case SoftSwitchAddress.RD80STORE: return (byte)(State[SoftSwitch.Store80] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDTEXT: return (byte)(State[SoftSwitch.TextMode] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDMIXED: return (byte)(State[SoftSwitch.MixedMode] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDPAGE2: return (byte)(State[SoftSwitch.Page2] ? 0x80 : 0x00);
                    case SoftSwitchAddress.RDHIRES: return (byte)(State[SoftSwitch.HiRes] ? 0x80 : 0x00);
                }

                if (address == SoftSwitchAddress.KBDSTRB)
                {
                    return HandleC011();
                }
                else if (address == SoftSwitchAddress.RDLCBNK2)
                {
                    return HandleC012();
                }
                else if (address >= 0xC080 && address <= 0xC08F)
                {
                    return HandleReadC08x(address);
                }

                return 0xFF;
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
                if (State[SoftSwitch.LcReadEnabled])
                {
                    return mainRam[address];
                }

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
            }

            // $D000–$FFFF ROM or RAM
            if (configuration.Model == AppleModel.AppleIIe)
            {
                if (0xD000 <= address && address <= 0xDFFF)
                {
                    if (State[SoftSwitch.LcWriteEnabled])
                    {
                        mainRam[address] = value;
                    }

                    return;
                }

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

        private byte HandleC011()
        {
            return (byte)(State[SoftSwitch.LcBank1] == false ? 0x80 : 0x00);
        }

        private byte HandleC012()
        {
            return (byte)(State[SoftSwitch.LcReadEnabled] ? 0x80 : 0x00);
        }

        private byte HandleReadC08x(ushort address)
        {
            // Bank select
            if ((address & LANG_A3) != 0)
            {
                // 1 = any access sets Bank_1
                State[SoftSwitch.LcBank1] = true;
            }
            else
            {
                // 0 = any access resets Bank_1
                State[SoftSwitch.LcBank1] = false;
            }

            // Read enable
            if (((address & LANG_A0A1) == 0) || ((address & LANG_A0A1) == 3))
            {
                // 00, 11 - set READ_ENABLE
                State[SoftSwitch.LcReadEnabled] = true;
            }
            else
            {
                // 01, 10 - reset READ_ENABLE
                State[SoftSwitch.LcReadEnabled] = false;
            }

            // PRE_WRITE
            int old_pre_write = preWrite;

            if ((address & 0b00000001) == 1)
            {
                // read 1 or 3, 00000001 - set PRE_WRITE
                preWrite = 1;
            }
            else
            {
                // read 0 or 2, 00000000 - reset PRE_WRITE
                preWrite = 0;
            }

            // Write Enable
            if ((old_pre_write == 1) && ((address & 0b00000001) == 1))
            {
                // PRE_WRITE set, read 1 or 3, 00000000 - reset WRITE_ENABLE'
                State[SoftSwitch.LcWriteEnabled] = false;
            }

            if ((address & 0b00000001) == 0)
            {
                // read 0 or 2, set _WRITE_ENABLE, 00000001 - set WRITE_ENABLE'
                State[SoftSwitch.LcWriteEnabled] = true;
            }

            SimDebugger.Info("LcBank1: {0}, LcReadEnabled: {1}, preWrite: {2}, LcWriteEnabled: {3}\n",
                State[SoftSwitch.LcBank1],
                State[SoftSwitch.LcReadEnabled],
                preWrite,
                State[SoftSwitch.LcWriteEnabled]);

            // handle the MMU configuration here
            return 0x00;
        }

        private void HandleWriteC08x(ushort address, byte value)
        {
            // Bank select
            if ((address & LANG_A3) != 0)
            {
                // 1 = any access sets Bank_1
                State[SoftSwitch.LcBank1] = true;
            }
            else
            {
                // 0 = any access resets Bank_1
                State[SoftSwitch.LcBank1] = false;
            }

            // Read enable
            if (((address & LANG_A0A1) == 0) || ((address & LANG_A0A1) == 3))
            {
                // 00, 11 - set READ_ENABLE
                State[SoftSwitch.LcReadEnabled] = true;
            }
            else
            {
                // 01, 10 - reset READ_ENABLE
                State[SoftSwitch.LcReadEnabled] = false;
            }

            // PRE_WRITE -- any write, reests PRE_WRITE
            preWrite = 0;

            // Write Enable
            if ((address & 0b00000001) == 0)
            {
                // write 0 or 2
                State[SoftSwitch.LcWriteEnabled] = true;
            }

            SimDebugger.Info("LcBank1: {0}, LcReadEnabled: {1}, preWrite: {2}, LcWriteEnabled: {3}\n",
                State[SoftSwitch.LcBank1],
                State[SoftSwitch.LcReadEnabled],
                preWrite,
                State[SoftSwitch.LcWriteEnabled]);
        }
    }
}
