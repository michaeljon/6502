using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using InnoWerks.Processors;

#pragma warning disable CA1822

namespace InnoWerks.Emulators.Apple
{
    public class MemoryBlocks
    {
        // primary bank with 12k RAM
        private const int LcBank2 = 0;

        // secondary bank with 4k RAM
        private const int LcBank1 = 1;

        private readonly MachineState machineState;

        private byte GetPage(ushort address) => (byte)((address & 0xFF00) >> 8);

        private byte GetOffset(ushort address) => (byte)(address & 0x00FF);

        // 48k $00-$C0
        private readonly MemoryPage[] mainMemory;

        // 48k $00-$C0
        private readonly MemoryPage[] auxMemory;

        // active r/w memory, 64k (256 pages $00-$FF)
        private readonly MemoryPage[] activeRead = [];

        private readonly MemoryPage[] activeWrite = [];

        // language card low ram
        private readonly MemoryPage[][] lcD0MainRam = new MemoryPage[2][];                                // $D000-$DFFF
        private readonly MemoryPage[][] lcD0AuxRam = new MemoryPage[2][];                                 // $D000-$DFFF

        // language card high ram
        private readonly MemoryPage[] lcEFMainRam;       // $E000-$FFFF
        private readonly MemoryPage[] lcEFAuxRam;        // $E000-$FFFF

        // switch-selectable
        private readonly MemoryPage[] intCxRom;          // $C000-$CFFF

        // swappable lo rom banks
        private readonly MemoryPage[] intDxRom;          // $D000–$DFFF

        // single hi rom bank
        private readonly MemoryPage[] intEFRom;          // $E000–$FFFF

        // device rom, c100-c700, numbered from 0 for convenience
        private readonly MemoryPage[] loSlotRom = new MemoryPage[8];

        // device rom, c800, numbered from 0 for convenience
        private readonly MemoryPage[][] hiSlotRom = new MemoryPage[8][];

        public MemoryBlocks(MachineState machineState)
        {
            this.machineState = machineState;

            //
            // setup enough spac to hold our working memory pointers, let's
            // call it, say, 64k worth of pages
            //

            activeRead = new MemoryPage[64 * 1024 / MemoryPage.PageSize];
            activeWrite = new MemoryPage[64 * 1024 / MemoryPage.PageSize];

            //
            // main and aux ram $0000-$C000
            //

            mainMemory = new MemoryPage[48 * 1024 / MemoryPage.PageSize];
            auxMemory = new MemoryPage[48 * 1024 / MemoryPage.PageSize];
            for (var p = 0x00; p < 0xC0; p++)
            {
                mainMemory[p] = new MemoryPage("main", p);
                auxMemory[p] = new MemoryPage("aux", p);

                activeRead[p] = mainMemory[p];
                activeWrite[p] = mainMemory[p];
            }

            //
            // language card RAM
            //

            // 4k for the 2 banks which are only from $D000-DFFF
            lcD0MainRam[LcBank1] = new MemoryPage[4 * 1024 / MemoryPage.PageSize];
            lcD0MainRam[LcBank2] = new MemoryPage[4 * 1024 / MemoryPage.PageSize];

            lcD0AuxRam[LcBank1] = new MemoryPage[4 * 1024 / MemoryPage.PageSize];
            lcD0AuxRam[LcBank2] = new MemoryPage[4 * 1024 / MemoryPage.PageSize];

            for (var p = 0; p < 4 * 1024 / MemoryPage.PageSize; p++)
            {
                lcD0MainRam[LcBank1][p] = new MemoryPage("lcD0MainRam[BANK1]", 0xD0 + p);
                lcD0MainRam[LcBank2][p] = new MemoryPage("lcD0MainRam[BANK2]", 0xD0 + p);

                lcD0AuxRam[LcBank1][p] = new MemoryPage("lcD0AUxRam[BANK1]", 0xD0 + p);
                lcD0AuxRam[LcBank2][p] = new MemoryPage("lcD0AUxRam[BANK2]", 0xD0 + p);
            }

            // language card high ram $E000-$FFFF
            lcEFMainRam = new MemoryPage[8 * 1024 / MemoryPage.PageSize];
            lcEFAuxRam = new MemoryPage[8 * 1024 / MemoryPage.PageSize];
            for (var p = 0; p < 8 * 1024 / MemoryPage.PageSize; p++)
            {
                lcEFMainRam[p] = new MemoryPage("lcEFMainRam", 0xE0 + p);
                lcEFAuxRam[p] = new MemoryPage("lcEFAuxRam", 0xE0 + p);
            }

            //
            // ROM space
            //

            // 4k switch selectable $C000-$CFFF
            intCxRom = new MemoryPage[4 * 1024 / MemoryPage.PageSize];
            for (var p = 0; p < 4 * 1024 / MemoryPage.PageSize; p++)
            {
                intCxRom[p] = new MemoryPage("intCxRom", 0xC0 + p);
            }

            // 4k ROM bank 1 $D000-$DFFF
            intDxRom = new MemoryPage[4 * 1024 / MemoryPage.PageSize];
            for (var p = 0; p < 4 * 1024 / MemoryPage.PageSize; p++)
            {
                intDxRom[p] = new MemoryPage("intDxRom", 0xD0 + p);
            }

            // 8k ROM $E000-$FFFF
            intEFRom = new MemoryPage[8 * 1024 / MemoryPage.PageSize];
            for (var p = 0; p < 8 * 1024 / MemoryPage.PageSize; p++)
            {
                intEFRom[p] = new MemoryPage("intEFRom", 0xE0 + p);
            }

            //
            // slot ROM
            //

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
            RemapRead();
            RemapWrite();

            // DumpActiveMemory();
        }

        private void RemapRead()
        {
            //
            // copy over the rom blocks, we might override below
            //
            InjectRom(intDxRom, 0xD0, 0xE0);
            InjectRom(intEFRom, 0xE0, 0x100);

            //
            // handle language card
            //
            if (machineState.State[SoftSwitch.LcReadEnabled] == true)
            {
                var bank = (ushort)(machineState.State[SoftSwitch.LcBank1] ? LcBank1 : LcBank2);
                InjectIndirectMemory(
                    activeRead,
                    machineState.State[SoftSwitch.ZpAux] == false ? lcD0MainRam[bank] : lcD0AuxRam[bank],
                    0xD0, 0xE0
                );

                InjectIndirectMemory(
                    activeRead,
                    machineState.State[SoftSwitch.ZpAux] == false ? lcEFMainRam : lcEFAuxRam,
                    0xE0, 0x100
                );
            }

            //
            // display pages TXT page 1 and HIRES page 1
            //
            if (machineState.State[SoftSwitch.Store80] == true)
            {
                InjectDirectMemory(
                    activeRead,
                    machineState.State[SoftSwitch.Page2] == true ? auxMemory : mainMemory,
                    0x04, 0x08
                );

                if (machineState.State[SoftSwitch.HiRes] == true)
                {
                    InjectDirectMemory(
                        activeRead,
                        machineState.State[SoftSwitch.Page2] == true ? auxMemory : mainMemory,
                        0x20, 0x40
                    );
                }
            }

            //
            // zero page and stack      $00 - $01
            //
            InjectDirectMemory(
                activeRead,
                machineState.State[SoftSwitch.ZpAux] == true ? auxMemory : mainMemory,
                0x00, 0x02
            );

            //
            // ROM                      $C0 - $C7
            //
            if (machineState.State[SoftSwitch.SlotRomEnabled] == false)
            {
                InjectRom(intCxRom, 0xC0, 0xD0);
                activeRead[0xC0] = MemoryPage.Zeros;
            }
            else
            {
                // walk each slot and hook up its rom
                for (var slot = 0; slot < 8; slot++)
                {
                    activeRead[0xC0 + slot] = loSlotRom[slot];
                }

                if (machineState.State[SoftSwitch.Slot3RomEnabled] == false)
                {
                    // point c3 at internal rom
                    activeRead[0xC3] = intCxRom[0x03];
                }

                if (machineState.State[SoftSwitch.IntC8RomEnabled] == true)
                {
                    // point c8 at internal rom
                    for (var loop = 0xC8; loop < 0xD0; loop++)
                    {
                        activeRead[loop] = intCxRom[loop - 0xC0];
                    }
                }
                else
                {
                    // hook up the active slot's rom to c8
                    if (machineState.CurrentSlot != -1)
                    {
                        for (var loop = 0xC8; loop < 0xD0; loop++)
                        {
                            InjectRom(hiSlotRom[machineState.CurrentSlot], 0xC8, 0xD0);
                        }
                    }
                }
            }

            activeRead[0xC0] = MemoryPage.FFs;
        }

        private void RemapWrite()
        {
            //
            // mark the lo rom blocks as read-only
            //
            for (var loop = 0xC0; loop < 0xD0; loop++)
            {
                activeWrite[loop] = null;
            }

            //
            // handle language card and/or high ROM
            //
            if (machineState.State[SoftSwitch.LcWriteEnabled] == true)
            {
                var bank = (ushort)(machineState.State[SoftSwitch.LcBank1] ? LcBank1 : LcBank2);
                InjectIndirectMemory(
                    activeWrite,
                    machineState.State[SoftSwitch.ZpAux] == false ? lcD0MainRam[bank] : lcD0AuxRam[bank],
                    0xD0, 0xE0
                );

                InjectIndirectMemory(
                    activeWrite,
                    machineState.State[SoftSwitch.ZpAux] == false ? lcEFMainRam : lcEFAuxRam,
                    0xE0, 0x100
                );
            }
            else
            {
                for (var loop = 0xD0; loop < 0x100; loop++)
                {
                    activeWrite[loop] = null;
                }
            }

            //
            // display pages TXT page 1 and HIRES page 1
            //
            if (machineState.State[SoftSwitch.Store80] == true)
            {
                InjectDirectMemory(
                    activeWrite,
                    machineState.State[SoftSwitch.Page2] == true ? auxMemory : mainMemory,
                    0x04, 0x08
                );

                if (machineState.State[SoftSwitch.HiRes] == true)
                {
                    InjectDirectMemory(
                        activeWrite,
                        machineState.State[SoftSwitch.Page2] == true ? auxMemory : mainMemory,
                        0x20, 0x40
                    );
                }
            }

            //
            // zero page and stack      $00 - $01
            //
            InjectDirectMemory(
                activeWrite,
                machineState.State[SoftSwitch.ZpAux] == true ? auxMemory : mainMemory,
                0x00, 0x02
            );
        }

        private void InjectRom(MemoryPage[] memoryPages, int from, int to)
        {
            for (var p = from; p < to; p++)
            {
                activeRead[p] = memoryPages[p - from];
                activeWrite[p] = MemoryPage.FFs;
            }
        }

        private void InjectIndirectMemory(MemoryPage[] activeMemory, MemoryPage[] memoryPages, int from, int to)
        {
            for (var p = from; p < to; p++)
            {
                activeMemory[p] = memoryPages[p - from];
            }
        }

        private void InjectDirectMemory(MemoryPage[] activeMemory, MemoryPage[] memoryPages, int from, int to)
        {
            for (var p = from; p < to; p++)
            {
                activeMemory[p] = memoryPages[p];
            }
        }

        public byte Read(ushort address)
        {
            var page = GetPage(address);
            var offset = GetOffset(address);

            if (activeRead[page] != null)
            {
                if (address == 0xC600)
                {
                    Debugger.Break();
                }
                return activeRead[page].Block[offset];
            }

            return 0xFF;
        }

        public void Write(ushort address, byte value)
        {
            var page = GetPage(address);
            var offset = GetOffset(address);

            if (activeWrite[page] != null)
            {
                activeWrite[page].Block[offset] = value;
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
            var memoryPage = new MemoryPage($"slot{slot}-cx", 0xC0 + slot);
            Array.Copy(objectCode, 0, memoryPage.Block, 0, 256);

            loSlotRom[slot] = memoryPage;
        }

        public void LoadSlotC8Rom(int slot, byte[] objectCode)
        {
            hiSlotRom[slot] = new MemoryPage[2048 / MemoryPage.PageSize];

            for (var page = 0; page < 2048 / MemoryPage.PageSize; page++)
            {
                var memoryPage = new MemoryPage("slot{slot}-c8", 0xC8 + page);
                Array.Copy(objectCode, 0, memoryPage.Block, 0, 256);

                hiSlotRom[slot][page] = memoryPage;
            }
        }

        public MemoryPage ResolveRead(ushort address)
        {
            var page = GetPage(address);

            return activeRead[page];
        }

        public MemoryPage ResolveWrite(ushort address)
        {
            var page = GetPage(address);

            return activeWrite[page];
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

            DumpPage(memoryPage.Block, memoryPage.PageNumber);
        }

        internal void DumpPage(byte[] page, int pageNumber)
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

            Debug.Write("       ");
            for (var b = 0; b < 32; b++)
            {
                if (b > 0x00 && b % 0x08 == 0)
                {
                    Debug.Write("  ");
                }

                Debug.Write($"== ");
            }

            Debug.WriteLine("");

            for (var l = 0; l < 0x100; l += 32)
            {
                Debug.Write($"{l + pageNumber << 8:X4}:  ");

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
            for (int page = startPage; page < endPage; page++)
            {
                if (startPage <= page && page <= endPage)
                {
                    MemoryPage r = activeRead[page];
                    MemoryPage w = activeWrite[page];

                    Debug.WriteLine($"[${page:X2}] -- R: {r}    W: {w}");
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
