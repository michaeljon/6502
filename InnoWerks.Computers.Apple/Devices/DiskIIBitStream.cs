
using System;
using System.Collections.Generic;
using System.IO;

namespace InnoWerks.Computers.Apple
{
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix

    public sealed class DiskIIBitStream
    {
        private readonly byte[] nibbles;

        public int Length => nibbles.Length;

        public DiskIIBitStream(byte[] nibbles)
        {
            this.nibbles = nibbles;
        }

        public byte ReadNibble(int position)
        {
            return nibbles[position % nibbles.Length];
        }

        // ============================================================
        // Static factory
        // ============================================================

        public static DiskIIBitStream[] FromDsk(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            return FromDsk(File.ReadAllBytes(path));
        }

        public static DiskIIBitStream[] FromDsk(byte[] dsk)
        {
            ArgumentNullException.ThrowIfNull(dsk);

            if (dsk.Length != 143360)
            {
                throw new ArgumentException("Only standard 140K DOS 3.3 DSK images supported");
            }

            var tracks = new DiskIIBitStream[35];
            for (int track = 0; track < 35; track++)
            {
                tracks[track] = BuildTrack(dsk, track);
            }

            return tracks;
        }

        // ============================================================
        // Track builder
        // ============================================================

        private static DiskIIBitStream BuildTrack(byte[] dsk, int track)
        {
            var bytes = new List<byte>();

            for (int logicalSector = 0; logicalSector < 16; logicalSector++)
            {
                int physicalSector = dos33Interleave[logicalSector];

                // Sync before address field
                WriteSync(bytes);

                WriteAddressField(bytes, track, physicalSector, 0xFE);

                WriteSync(bytes);

                byte[] sectorData = ReadSector(dsk, track, physicalSector);
                WriteDataField(bytes, sectorData);
            }

            return new DiskIIBitStream([.. bytes]);
        }

        // ============================================================
        // Address + Data Fields
        // ============================================================

        private static void WriteAddressField(List<byte> dst, int track, int sector, byte volume)
        {
            dst.Add(0xD5);
            dst.Add(0xAA);
            dst.Add(0x96);

            WriteOddEven(dst, volume);
            WriteOddEven(dst, (byte)track);
            WriteOddEven(dst, (byte)sector);
            WriteOddEven(dst, (byte)(volume ^ track ^ sector));

            dst.Add(0xDE);
            dst.Add(0xAA);
            dst.Add(0xEB);
        }

        private static void WriteDataField(List<byte> dst, byte[] sector)
        {
            dst.Add(0xD5);
            dst.Add(0xAA);
            dst.Add(0xAD);

            byte[] encoded = Encode6And2(sector);
            dst.AddRange(encoded);

            dst.Add(0xDE);
            dst.Add(0xAA);
            dst.Add(0xEB);
        }

        // ============================================================
        // Encoding helpers
        // ============================================================

        private static void WriteSync(List<byte> dst)
        {
            for (int i = 0; i < 10; i++)
                dst.Add(0xFF);
        }

        private static void WriteOddEven(List<byte> dst, byte value)
        {
            dst.Add((byte)((value >> 1) | 0xAA));
            dst.Add((byte)(value | 0xAA));
        }

        private static byte[] Encode6And2(byte[] sector)
        {
            byte[] output = new byte[343];
            byte[] aux = new byte[86];

            int auxIndex = 0;

            for (int i = 0; i < 256; i++)
            {
                aux[auxIndex] = (byte)((aux[auxIndex] << 2) | (sector[i] & 0x03));
                sector[i] >>= 2;

                auxIndex = (auxIndex + 1) % aux.Length;
            }

            int outIndex = 0;
            byte checksum = 0;

            for (int i = 0; i < aux.Length; i++)
            {
                checksum ^= aux[i];
                output[outIndex++] = gcrTable[checksum & 0x3F];
            }

            for (int i = 0; i < 256; i++)
            {
                checksum ^= sector[i];
                output[outIndex++] = gcrTable[checksum & 0x3F];
            }

            output[outIndex++] = gcrTable[checksum & 0x3F];

            return output;
        }

        // ============================================================
        // Utilities
        // ============================================================

        private static byte[] ReadSector(byte[] dsk, int track, int sector)
        {
            int offset = (track * 16 + sector) * 256;
            var data = new byte[256];
            Array.Copy(dsk, offset, data, 0, 256);
            return data;
        }

        private static bool[] BytesToBits(List<byte> bytes)
        {
            var bits = new bool[bytes.Count * 8];
            int index = 0;

            foreach (byte b in bytes)
            {
                for (int i = 7; i >= 0; i--)
                {
                    bits[index++] = ((b >> i) & 1) != 0;
                }
            }

            return bits;
        }

        // ============================================================
        // Constants
        // ============================================================

        private static readonly int[] dos33Interleave =
        [
            0x00, 0x07, 0x0E, 0x06,
            0x0D, 0x05, 0x0C, 0x04,
            0x0B, 0x03, 0x0A, 0x02,
            0x09, 0x01, 0x08, 0x0F
        ];

        private static readonly byte[] gcrTable =
        [
            0x96, 0x97, 0x9A, 0x9B, 0x9D, 0x9E, 0x9F, 0xA6,
            0xA7, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF, 0xB2, 0xB3,
            0xB4, 0xB5, 0xB6, 0xB7, 0xB9, 0xBA, 0xBB, 0xBC,
            0xBD, 0xBE, 0xBF, 0xCB, 0xCD, 0xCE, 0xCF, 0xD3,
            0xD6, 0xD7, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE,
            0xDF, 0xE5, 0xE6, 0xE7, 0xE9, 0xEA, 0xEB, 0xEC,
            0xED, 0xEE, 0xEF, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6,
            0xF7, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF
        ];
    }
}
