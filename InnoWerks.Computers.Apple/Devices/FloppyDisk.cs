
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace InnoWerks.Computers.Apple
{
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
#pragma warning disable CA5394 // Do not use insecure randomness
#pragma warning disable CA1707 // Identifiers should not contain underscores

    public sealed class FloppyDisk
    {
        public const int TRACK_NIBBLE_LENGTH = 0x1A00;
        public const int TRACK_COUNT = 35;
        public const int SECTOR_COUNT = 16;
        public const int HALF_TRACK_COUNT = TRACK_COUNT * 2;
        public const int DISK_NIBBLE_LENGTH = TRACK_NIBBLE_LENGTH * TRACK_COUNT;
        public const int DISK_PLAIN_LENGTH = 143360;
        public const byte DEFAULT_VOLUME_NUMBER = 0xFE;

        private static readonly int[] dos33Interleave =
        [
            0x00, 0x07, 0x0E, 0x06, 0x0D, 0x05, 0x0C, 0x04,
            0x0B, 0x03, 0x0A, 0x02, 0x09, 0x01, 0x08, 0x0F
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

        private readonly byte[] nibbles;

        public int VolumeNumber { get; set; } = DEFAULT_VOLUME_NUMBER;

        public int Length => nibbles.Length;

        public byte ReadNibble(int position)
        {
            return nibbles[position % nibbles.Length];
        }

        public static FloppyDisk FromDsk(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            return FromDsk(File.ReadAllBytes(path));
        }

        public static FloppyDisk FromDsk(byte[] dsk)
        {
            ArgumentNullException.ThrowIfNull(dsk);

            if (dsk.Length != 143360)
            {
                throw new ArgumentException("Only standard 140K DOS 3.3 DSK images supported");
            }

            var nibbles = Nibblize(dsk);
            Debug.Assert(nibbles.Length == DISK_NIBBLE_LENGTH);
            return new FloppyDisk(nibbles);
        }

        private FloppyDisk(byte[] nibbles)
        {
            this.nibbles = nibbles;
        }

        private static byte[] Nibblize(byte[] nibbles)
        {
            var rng = new Random();
            var output = new List<byte>();

            for (var track = 0; track < TRACK_COUNT; track++)
            {
                for (var sector = 0; sector < SECTOR_COUNT; sector++)
                {
                    var gap2 = rng.Next(5, 9);

                    WriteNoiseBytes(output, 15);
                    WriteAddressBlock(output, track, sector);
                    WriteNoiseBytes(output, gap2);
                    NibblizeBlock(output, track, dos33Interleave[sector], nibbles);
                    WriteNoiseBytes(output, 38 - gap2);
                }
            }

            return [.. output];
        }

        private static void WriteNoiseBytes(List<byte> output, int cnt)
        {
            for (var b = 0; b < cnt; b++)
            {
                output.Add(0xFF);
            }
        }

        private static void WriteAddressBlock(List<byte> output, int track, int sector)
        {
            output.Add(0xD5);
            output.Add(0xAA);
            output.Add(0x96);

            var checksum = 0;

            checksum ^= DEFAULT_VOLUME_NUMBER;
            WriteOddEven(output, DEFAULT_VOLUME_NUMBER);

            checksum ^= track;
            WriteOddEven(output, (byte)track);

            checksum ^= sector;
            WriteOddEven(output, (byte)sector);

            WriteOddEven(output, (byte)(checksum & 0xFF));

            output.Add(0xDE);
            output.Add(0xAA);
            output.Add(0xEB);
        }

        private static void WriteOddEven(List<byte> output, byte value)
        {
            output.Add((byte)((value >> 1) | 0xAA));
            output.Add((byte)(value | 0xAA));
        }

        private static int DecodeOddEven(byte b1, byte b2)
        {
            return ((((b1 << 1) | 1) & b2) & 0xFF);
        }

        private static void NibblizeBlock(List<byte> output, int track, int sector, byte[] nibbles)
        {
            var offset = ((track * SECTOR_COUNT) + sector) * 256;

            // leave this as int until the end, it'll reduce all the casting
            var temp = new int[342];
            for (var i = 0; i < 256; i++)
            {
                temp[i] = (nibbles[offset + i] & 0xFF) >> 2;
            }

            int hi = 0x01;
            int med = 0xAB;
            int lo = 0x55;

            for (var i = 0; i < 0x56; i++)
            {
                temp[i + 256] = ((nibbles[offset + hi] & 1) << 5) |
                                ((nibbles[offset + hi] & 2) << 3) |
                                ((nibbles[offset + med] & 1) << 3) |
                                ((nibbles[offset + med] & 2) << 1) |
                                ((nibbles[offset + lo] & 1) << 1) |
                                ((nibbles[offset + lo] & 2) >> 1);

                hi = (hi - 1) & 0xFF;
                med = (med - 1) & 0xFF;
                lo = (lo - 1) & 0xFF;
            }

            output.Add(0xD5);
            output.Add(0xAA);
            output.Add(0xAD);

            var last = 0;
            for (var i = temp.Length - 1; i > 255; i--)
            {
                var value = temp[i] ^ last;
                output.Add(gcrTable[value]);
                last = temp[i];
            }

            for (var i = 0; i < 256; i++)
            {
                var value = temp[i] ^ last;
                output.Add(gcrTable[value]);
                last = temp[i];
            }

            output.Add(gcrTable[last]);
            output.Add(0xDE);
            output.Add(0xAA);
            output.Add(0xEB);
        }
    }
}
