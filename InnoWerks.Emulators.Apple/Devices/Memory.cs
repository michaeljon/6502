using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;

#pragma warning disable CA1822

namespace InnoWerks.Emulators.Apple
{
    public class MemoryBlocks
    {
        private readonly Dictionary<byte, byte[]> mainMemory = [];
        private readonly Dictionary<byte, byte[]> auxMemory = [];

        private readonly Dictionary<byte, byte[]> current = [];

        private byte GetPage(ushort address) => (byte)((address & 0xFF00) >> 8);

        private byte GetOffset(ushort address) => (byte)(address & 0x00FF);

        private readonly byte[][] lcRam;         // IIe only

        // swappable lo rom banks
        private readonly byte[] loRom;           // $D000–$DFFF

        // switch-selectable
        private readonly byte[] cxRom;           // $C000-$CFFF

        // single hi rom bank
        private readonly byte[] hiRom;           // $E000–$FFFF


        public void Remap(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

        }

        public MemoryBlocks(SoftSwitches softSwitches)
        {
            for (int p = 0; p < 256; p++)
            {
                mainMemory.Add((byte)p, new byte[256]);
                auxMemory.Add((byte)p, new byte[256]);
            }

            // language card ram (should be private to MemoryIIe, really)
            lcRam = new byte[2][];
            lcRam[0] = new byte[4 * 1024];          // 4k RAM bank 1
            lcRam[1] = new byte[4 * 1024];          // 4k RAM bank 2

            loRom = new byte[4 * 1024];             // 4k ROM bank 1
            cxRom = new byte[4 * 1024];             // 4k switch selectable
            hiRom = new byte[8 * 1024];             // 8k ROM
        }
    }
}
