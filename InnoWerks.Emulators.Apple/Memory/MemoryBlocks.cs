using System;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable CA1822

namespace InnoWerks.Emulators.Apple
{
    public class MemoryBlocks
    {
        private const int NumberOfPages = 64 * 1024 / MemoryPage.PageSize;

        private readonly MemoryPage[] mainMemory = new MemoryPage[NumberOfPages];
        private readonly MemoryPage[] auxMemory = new MemoryPage[NumberOfPages];

        private readonly List<(MemoryPage ReadFrom, MemoryPage WriteTo)> activeMemory = [];

        private readonly MachineState machineState;

        private byte GetPage(ushort address) => (byte)((address & 0xFF00) >> 8);

        private byte GetOffset(ushort address) => (byte)(address & 0x00FF);

        private readonly MemoryPage[][] lcRam = new MemoryPage[2][];                                    // $D000-$DFFF

        // switch-selectable
        private readonly MemoryPage[] intCxRom = new MemoryPage[4 * 1024 / MemoryPage.PageSize];           // $C000-$CFFF

        // swappable lo rom banks
        private readonly MemoryPage[] intDxRom = new MemoryPage[4 * 1024 / MemoryPage.PageSize];           // $D000–$DFFF

        // single hi rom bank
        private readonly MemoryPage[] intEFRom = new MemoryPage[8 * 1024 / MemoryPage.PageSize];           // $E000–$FFFF

        // device rom, c100-c700, numbered from 0 for convenience
        private readonly MemoryPage[] loSlotRom = new MemoryPage[8];

        // device rom, c800, numbered from 0 for convenience
        private readonly MemoryPage[][] hiSlotRom = new MemoryPage[8][];

        public MemoryBlocks(MachineState machineState)
        {
            this.machineState = machineState;

            for (var p = 0; p < NumberOfPages; p++)
            {
                mainMemory[p] = new MemoryPage("main", p);
                auxMemory[p] = new MemoryPage("aux", p);

                activeMemory.Add((mainMemory[p], auxMemory[p]));
            }

            // language card ram
            lcRam[0] = new MemoryPage[12 * 1024 / MemoryPage.PageSize];
            lcRam[1] = new MemoryPage[4 * 1024 / MemoryPage.PageSize];

            // 4k for the 2 banks which are only from $D000-DFFF
            for (var p = 0; p < 4 * 1024 / MemoryPage.PageSize; p++)
            {
                lcRam[0][p] = new MemoryPage("lcRam[0]", 0xD0 + p);
                lcRam[1][p] = new MemoryPage("lcRam[1]", 0xD0 + p);
            }

            // 8k for the the remaining RAM at $E000-$FFFF
            for (var p = 4 * 1024 / MemoryPage.PageSize; p < 12 * 1024 / MemoryPage.PageSize; p++)
            {
                lcRam[0][p] = new MemoryPage("lcRam[0]", 0xE0 + p);
            }

            // 4k switch selectable $C000-$CFFF
            for (var p = 0; p < 4 * 1024 / MemoryPage.PageSize; p++)
            {
                intCxRom[p] = new MemoryPage("intCxRom", 0xC0 + p);
            }

            // 4k ROM bank 1 $D000-$DFFF
            for (var p = 0; p < 4 * 1024 / MemoryPage.PageSize; p++)
            {
                intDxRom[p] = new MemoryPage("intDxRom", 0xD0 + p);
            }

            // 8k ROM $E000-$FFFF
            for (var p = 0; p < 8 * 1024 / MemoryPage.PageSize; p++)
            {
                intEFRom[p] = new MemoryPage("intEFRom", 0xE0 + p);
            }

            // cx slot rom, one page per slot, $C100-$C7FF
            for (var slot = 0; slot < 8; slot++)
            {
                loSlotRom[slot] = MemoryPage.Zeros;
            }

            // c8 slot rom, one page per slot, $C800-$CFFF
            for (var slot = 0; slot < 8; slot++)
            {
                hiSlotRom[slot] = new MemoryPage[2048 / MemoryPage.PageSize];

                for (var page = 0; page < 2048 / MemoryPage.PageSize; page++)
                {
                    hiSlotRom[slot][page] = MemoryPage.Zeros;
                }
            }

            Remap();
        }

        /// <summary>
        /// Overall memory map
        ///
        /// BSR / ROM                $E0 - $FF   mainMemory / auxMemory / intEFRom
        /// Bank 2                   $D0 - $DF   lcRam
        /// Bank 1                   $D0 - $DF   lcRam / intDxRom
        /// INT ROM                  $C0 - $CF   intCxRom
        /// Hi RAM                   $60 - $BF   mainMemory / auxMemory
        /// Hi-res Page 2            $40 - $5F
        /// Hi-res Page 1            $20 - $3F
        /// RAM                      $0C - $1F
        /// Text Page 2              $08 - $0B
        /// Text Page 1              $04 - $07
        /// BASIC workspace          $02 - $03
        /// zero page and stack      $00 - $01
        ///
        /// </summary>
        public void Remap()
        {
            ArgumentNullException.ThrowIfNull(machineState, nameof(machineState));

            // reset the aux/main selector
            for (var loop = 0x00; loop < 0xD0; loop++)
            {
                activeMemory[loop] = (mainMemory[loop], mainMemory[loop]);
            }

            // reset the I/O page
            activeMemory[0xC0] = (null, null);

            // zero page and stack      $00 - $01
            for (var loop = 0x00; loop < 0x02; loop++)
            {
                // memshadow[loop] = SW_ALTZP ? memaux + (loop << 8) : memmain + (loop << 8);

                if (machineState.State[SoftSwitch.ZpAux] == false)
                {
                    activeMemory[loop] = (mainMemory[loop], mainMemory[loop]);
                }
                else
                {
                    activeMemory[loop] = (auxMemory[loop], auxMemory[loop]);
                }
            }

            // primary working memory   $02 - $C0
            for (var loop = 0x02; loop < 0xC0; loop++)
            {
                // memshadow[loop] = SW_AUXREAD ? memaux+(loop << 8)
                // 	: memmain+(loop << 8);

                // memwrite[loop]  = ((SW_AUXREAD != 0) == (SW_AUXWRITE != 0))
                // 	? mem+(loop << 8)
                // 	: SW_AUXWRITE	? memaux+(loop << 8)
                // 					: memmain+(loop << 8);

                switch (machineState.AuxReadAuxWriteBitmask)
                {
                    case AuxReadAuxWriteBitmaskType.NotReadNotWrite:
                        activeMemory[loop] = (mainMemory[loop], mainMemory[loop]);
                        break;

                    case AuxReadAuxWriteBitmaskType.NotReadOkWrite:
                        activeMemory[loop] = (mainMemory[loop], auxMemory[loop]);
                        break;

                    case AuxReadAuxWriteBitmaskType.OkReadNotWrite:
                        activeMemory[loop] = (auxMemory[loop], mainMemory[loop]);
                        break;

                    case AuxReadAuxWriteBitmaskType.OkReadOkWrite:
                        activeMemory[loop] = (auxMemory[loop], auxMemory[loop]);
                        break;
                }
            }

            // display pages TXT and HIRES
            if (machineState.State[SoftSwitch.Store80] == true)
            {
                for (var loop = 0x04; loop < 0x08; loop++)
                {
                    activeMemory[loop] = machineState.State[SoftSwitch.Page2] == true ?
                        (auxMemory[loop], auxMemory[loop]) :
                        (mainMemory[loop], mainMemory[loop]);
                }

                if (machineState.State[SoftSwitch.HiRes] == true)
                {
                    for (var loop = 0x20; loop < 0x40; loop++)
                    {
                        activeMemory[loop] = machineState.State[SoftSwitch.Page2] == true ?
                            (auxMemory[loop], auxMemory[loop]) :
                            (mainMemory[loop], mainMemory[loop]);
                    }
                }
            }

            if (machineState.LcActive == true)
            {
                for (var loop = 0xC0; loop < 0xD0; loop++)
                {
                    var r = machineState.State[SoftSwitch.AuxRead] == true ? auxMemory[loop] : intCxRom[loop - 0xC0];
                    var w = machineState.State[SoftSwitch.AuxWrite] == true ? auxMemory[loop] : null;

                    activeMemory[loop] = (r, w);
                }
            }
            else
            {
                // INT ROM                  $C0 - $CF   intCxRom
                for (var loop = 0xC0; loop < 0xC8; loop++)
                {
                    MemoryPage r;

                    if (machineState.State[SoftSwitch.SlotRomEnabled] == true)
                    {
                        r = machineState.State[SoftSwitch.Slot3RomEnabled] == false ?
                            intCxRom[loop - 0xC0] :
                            loSlotRom[loop - 0xC0];
                    }
                    else
                    {
                        r = intCxRom[loop - 0xC0];
                    }

                    activeMemory[loop] = (r, null);
                }

                for (var loop = 0xC8; loop < 0xD0; loop++)
                {
                    if (machineState.State[SoftSwitch.SlotRomEnabled] == true)
                    {
                        // we can only read from one of two places: the internal ROM or the device ROM
                        if (machineState.State[SoftSwitch.IntC8RomEnabled] == true)
                        {
                            // this is the internal ROM, and it's read-only
                            activeMemory[loop] = (intCxRom[loop - 0xC0], null);
                        }
                        else
                        {
                            // this indicates to the bus that it's slot ROM and it's read-only
                            var slot = loop - 0xC8 + 1;
                            if (hiSlotRom[slot] != null)
                            {
                                activeMemory[loop] = (hiSlotRom[slot][loop - 0xC8], null);
                            }
                            else
                            {
                                activeMemory[loop] = (null, null);
                            }
                        }
                    }
                    else
                    {
                        // in this case we're not using the C8 as ROM
                        switch (machineState.AuxReadAuxWriteBitmask)
                        {
                            case AuxReadAuxWriteBitmaskType.NotReadNotWrite:
                                activeMemory[loop] = (intCxRom[loop - 0xC0], null);
                                break;

                            case AuxReadAuxWriteBitmaskType.NotReadOkWrite:
                                activeMemory[loop] = (mainMemory[loop], auxMemory[loop]);
                                break;

                            case AuxReadAuxWriteBitmaskType.OkReadNotWrite:
                                activeMemory[loop] = (auxMemory[loop], mainMemory[loop]);
                                break;

                            case AuxReadAuxWriteBitmaskType.OkReadOkWrite:
                                activeMemory[loop] = (auxMemory[loop], auxMemory[loop]);
                                break;
                        }
                    }
                }
            }

            //
            // this section of memory is
            //
            //    1  ROM from intDxRom
            //    2  RAM from language card bank 1 or bank 2
            //    3  RAM from the second 64k / aux memory
            var bank = (ushort)(machineState.State[SoftSwitch.LcBank1] ? 1 : 0);
            for (var loop = 0xD0; loop < 0xE0; loop++)
            {
                MemoryPage r = intDxRom[loop - 0xD0];
                MemoryPage w = null;

                if (machineState.LcActive)
                {
                    r = machineState.State[SoftSwitch.ZpAux] ?
                        auxMemory[loop] :
                        lcRam[bank][loop - 0xD0];
                }

                if (machineState.State[SoftSwitch.AuxWrite])
                {
                    w = machineState.LcActive ?
                        mainMemory[loop] :
                        lcRam[bank][loop - 0xD0];
                }

                activeMemory[loop] = (r, w);
            }

            //
            // this section of memory is
            //
            //    1  ROM from intEFRom
            //    2  RAM from language card upper memory
            //    3  RAM from the second 64k / aux memory
            for (var loop = 0xE0; loop < 0x100; loop++)
            {
                MemoryPage r = intEFRom[loop - 0xE0];
                MemoryPage w = null;

                if (machineState.LcActive)
                {
                    r = machineState.State[SoftSwitch.ZpAux] ?
                        auxMemory[loop] :
                        lcRam[0][loop - 0xE0];
                }

                if (machineState.State[SoftSwitch.AuxWrite])
                {
                    w = machineState.LcActive ?
                        mainMemory[loop] :
                        lcRam[0][loop - 0xE0];
                }

                activeMemory[loop] = (r, w);
            }

            // DumpActiveMemory();
        }

        public byte Read(ushort address)
        {
            var page = GetPage(address);
            var offset = GetOffset(address);

            if (activeMemory[page].ReadFrom != null)
            {
                return activeMemory[page].ReadFrom.Block[offset];
            }

            return 0xFF;
        }

        public void Write(ushort address, byte value)
        {
            var page = GetPage(address);
            var offset = GetOffset(address);

            if (activeMemory[page].WriteTo != null)
            {
                activeMemory[page].WriteTo.Block[offset] = value;
            }
        }

        public void LoadProgramToRom(byte[] objectCode)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            if (objectCode.Length != 32 * 1024)
            {
                throw new NotImplementedException("IIe ROM must be 32k");
            }

            for (var page = 0; page < 4 * 1024 / MemoryPage.PageSize; page++)
            {
                // load the first 4k from the 16k block at the end into cx rom
                Array.Copy(objectCode, (16 * 1024) + (page * 0x100), intCxRom[page].Block, 0, 0x100);
            }

            for (var page = 0; page < 4 * 1024 / MemoryPage.PageSize; page++)
            {
                // load the first 4k from the 16k block at the end into lo rom
                Array.Copy(objectCode, (20 * 1024) + (page * 0x100), intDxRom[page].Block, 0, 0x100);
            }

            for (var page = 0; page < 8 * 1024 / MemoryPage.PageSize; page++)
            {
                // load the remaining 8k from the 16k block into hi rom
                Array.Copy(objectCode, (24 * 1024) + (page * 0x100), intEFRom[page].Block, 0, 0x100);
            }
        }

        public void LoadProgramToRam(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            var pageNumber = GetPage(origin);
            var pages = objectCode.Length / MemoryPage.PageSize;
            var remainder = objectCode.Length - pages;

            for (var page = 0; page < pages; page++)
            {
                Array.Copy(objectCode, 0, mainMemory[pageNumber + page].Block, 0, 256);
            }

            if (remainder > 0)
            {
                Array.Copy(objectCode, 0, mainMemory[pageNumber + pages].Block, 0, remainder);
            }
        }

        public void LoadSlotCxRom(int slot, byte[] objectCode)
        {
            // slots load themselves starting at 1, so 0xC6 would map to
            // a Disk II in slot 6
            var memoryPage = new MemoryPage("Slot Cx ROM", 0xC0 + slot);
            Array.Copy(objectCode, 0, loSlotRom[slot].Block, 0, 256);
            loSlotRom[slot] = memoryPage;
        }

        public void LoadSlotC8Rom(int slot, byte[] objectCode)
        {
            hiSlotRom[slot] = new MemoryPage[2048 / MemoryPage.PageSize];

            for (var page = 0; page < 2048 / MemoryPage.PageSize; page++)
            {
                var memoryPage = new MemoryPage("Slot C8 ROM", 0xC8 + page);
                Array.Copy(objectCode, 0, memoryPage.Block, 0, 256);
                hiSlotRom[slot][page] = memoryPage;
            }
        }

        public MemoryPage ResolveRead(ushort address)
        {
            var page = GetPage(address);

            return activeMemory[page].ReadFrom;
        }

        public MemoryPage ResolveWrite(ushort address)
        {
            var page = GetPage(address);

            return activeMemory[page].WriteTo;
        }

        public byte GetMain(ushort address)
        {
            var page = GetPage(address);
            var offset = GetOffset(address);

            return mainMemory[page].Block[offset];
        }

        public byte GetAux(ushort address)
        {
            var page = GetPage(address);
            var offset = GetOffset(address);

            return auxMemory[page].Block[offset];
        }

        internal void DumpPage(MemoryPage memoryPage)
        {
            Debug.WriteLine("MemoryPage {0}", memoryPage);

            DumpPage(memoryPage.Block);
        }

        internal void DumpPage(byte[] page)
        {
            Debug.Write("       ");
            for (var b = 0; b < 32; b++)
            {
                if (b > 0x00 && b % 0x08 == 0)
                {
                    Debug.Write("  ");
                }

                Debug.Write($"{b:X2} ");
            }

            Debug.WriteLine("");

            for (var l = 0; l < 0x100; l += 32)
            {
                Debug.Write($"{l:X4}:  ");

                for (var b = 0; b < 32; b++)
                {
                    if (b > 0x00 && b % 0x08 == 0)
                    {
                        Debug.Write("  ");
                    }

                    Debug.Write($"{page[(ushort)(l + b)]:X2} ");
                }

                Debug.WriteLine("");
            }

            Debug.WriteLine("");
        }

        internal void DumpActiveMemory(byte startPage = 0x00, byte endPage = 0xff)
        {
            int page = 0x00;

            foreach (var (r, w) in activeMemory)
            {
                if (startPage <= page && page <= endPage)
                {
                    Debug.WriteLine($"[${page++:X2}] -- R: {r}    W: {w}");
                }
            }
        }

        internal void DumpNamedMemory(MemoryPage[] memoryPages)
        {
            for (var p = 0; p < memoryPages.Length; p++)
            {
                Debug.WriteLine($"[{p}]: {memoryPages[p]}");
            }
        }
    }
}
