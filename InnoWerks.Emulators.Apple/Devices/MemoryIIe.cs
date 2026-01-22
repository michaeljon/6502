using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class MemoryIIe // : IDevice
    {
        private readonly AppleConfiguration configuration;

        private readonly MachineState machineState;

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

        public MemoryIIe(AppleConfiguration configuration, MachineState machineState)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(machineState);

            this.configuration = configuration;
            this.machineState = machineState;

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

        public byte Read(ushort address)
        {
            if (address < 0x0200)
            {
                return machineState.State[SoftSwitch.ZpAux] ? auxRam[address] : mainRam[address];
            }

            // todo - handle page1, page2, main/aux switching here
            // for addresses $0400-$07FF and $0800-$0BFF
            else if (0x0400 <= address && address <= 0x07FF)
            {
                // this is text page 1
                if (machineState.State[SoftSwitch.Store80] == false)
                {
                    return machineState.State[SoftSwitch.Page2] ? auxRam[address] : mainRam[address];
                }

                return mainRam[address];
            }
            else if (address < 0xC000)
            {
                return machineState.State[SoftSwitch.AuxRead] ? auxRam[address] : mainRam[address];
            }
            else if (0xC000 <= address && address <= 0xC0FF)
            {
                /* no-op */
                Debug.WriteLine($"Why are we reading address {address:X4}");
            }
            else if (0xC100 <= address && address <= 0xCFFF)
            {
                if (machineState.State[SoftSwitch.AuxRead] == true)
                {
                    return auxRam[address];
                }

                int offset = address - 0xC000;
                return cxRom[offset];
            }
            else if (0xD000 <= address && address <= 0xDFFF)
            {
                int offset = address - 0xD000;
                if (machineState.State[SoftSwitch.LcReadEnabled] == true)
                {
                    return lcRam[machineState.State[SoftSwitch.LcBank1] ? 1 : 0][offset];
                }

                if (machineState.State[SoftSwitch.AuxRead] == true)
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
                if (machineState.State[SoftSwitch.ZpAux] == true)
                {
                    auxRam[address] = value;
                }
                else
                {
                    mainRam[address] = value;
                }
            }

            // todo - handle page1, page2, main/aux switching here
            // for addresses $0400-$07FF and $0800-$0BFF
            else if (0x0400 <= address && address <= 0x07FF)
            {
                // this is text page 1
                if (machineState.State[SoftSwitch.Store80] == false)
                {
                    mainRam[address] = value;
                }
                else
                {
                    auxRam[address] = value;
                }
            }

            else if (address < 0xC000)
            {
                if (configuration.Model == AppleModel.AppleIIe && machineState.State[SoftSwitch.AuxWrite])
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
                /* no-op */
            }
            else if (0xC100 <= address && address <= 0xCFFF)
            {
                if (machineState.State[SoftSwitch.AuxWrite] == true)
                {
                    auxRam[address] = value;
                }
            }
            else if (0xD000 <= address && address <= 0xDFFF)
            {
                if (machineState.State[SoftSwitch.LcWriteEnabled] == true)
                {
                    lcRam[machineState.State[SoftSwitch.LcBank1] ? 1 : 0][address - 0xD000] = value;
                }
                else if (machineState.State[SoftSwitch.AuxWrite] == true)
                {
                    auxRam[address] = value;
                }
            }
            else if (0xE000 <= address && address <= 0xFFFF)
            {
                if (machineState.State[SoftSwitch.AuxWrite] == true)
                {
                    auxRam[address] = value;
                }
                else
                {
                    mainRam[address] = value;
                }
            }
        }

        public byte Peek(ushort address)
        {
            // return a non-cycle counting read into "current" memory
            return Read(address);
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
            Array.Copy(objectCode, 20 * 1024, loRom, 0, 4 * 1024);

            // load the remaining 12k from the 16k block into hi rom
            Array.Copy(objectCode, 24 * 1024, hiRom, 0, 8 * 1024);
        }

        public void LoadProgramToRam(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            Array.Copy(objectCode, 0, mainRam, origin, objectCode.Length);
        }

        internal void DumpPage(byte page, PeekArea peekArea)
        {
            if (peekArea == PeekArea.Current)
            {
                throw new NotImplementedException("Can't read 'current', specify an exact regions");
            }

            byte[] bytes = peekArea switch
            {
                PeekArea.MainRam => mainRam,
                PeekArea.AuxRam => auxRam,
                PeekArea.LanguageCardRam => machineState.State[SoftSwitch.LcBank1] ? lcRam[1] : lcRam[0],
                PeekArea.LowRom => loRom,
                PeekArea.CxRom => cxRom,
                PeekArea.HighRom => hiRom,

                _ => throw new ArgumentOutOfRangeException(nameof(peekArea)),
            };

            ushort baseAddr = peekArea switch
            {
                PeekArea.MainRam => (ushort)(page << 8),
                PeekArea.AuxRam => (ushort)(page << 8),
                PeekArea.LanguageCardRam => (ushort)((page << 8) - 0xD000),
                PeekArea.LowRom => (ushort)((page << 8) - 0xD000),
                PeekArea.CxRom => (ushort)((page << 8) - 0xC000),
                PeekArea.HighRom => (ushort)((page << 8) - 0xE000),

                _ => throw new ArgumentOutOfRangeException(nameof(peekArea)),
            };

            Console.Write("       ");
            for (var b = 0; b < 32; b++)
            {
                if (b > 0x00 && b % 0x08 == 0)
                {
                    Console.Write("  ");
                }

                Console.Write("{0:X2} ", b);
            }

            Console.WriteLine();

            for (var l = baseAddr; l < baseAddr + 0x100; l += 32)
            {
                Console.Write("{0:X4}:  ", l);

                for (var b = 0; b < 32; b++)
                {
                    if (b > 0x00 && b % 0x08 == 0)
                    {
                        Console.Write("  ");
                    }

                    Console.Write("{0:X2} ", bytes[(ushort)(l + b)]);
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}
