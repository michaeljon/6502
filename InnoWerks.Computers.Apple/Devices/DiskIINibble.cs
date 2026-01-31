using System;

namespace InnoWerks.Computers.Apple
{
    public static class DiskIINibble
    {
        // 6-and-2 encoding table
        private static readonly byte[] EncodeTable =
        [
            0x96,0x97,0x9A,0x9B,0x9D,0x9E,0x9F,0xA6,
            0xA7,0xAB,0xAC,0xAD,0xAE,0xAF,0xB2,0xB3,
            0xB4,0xB5,0xB6,0xB7,0xB9,0xBA,0xBB,0xBC,
            0xBD,0xBE,0xBF,0xCB,0xCD,0xCE,0xCF,0xD3,
            0xD6,0xD7,0xD9,0xDA,0xDB,0xDC,0xDD,0xDE,
            0xDF,0xE5,0xE6,0xE7,0xE9,0xEA,0xEB,0xEC,
            0xED,0xEE,0xEF,0xF2,0xF3,0xF4,0xF5,0xF6,
            0xF7,0xF9,0xFA,0xFB,0xFC,0xFD,0xFE,0xFF
        ];

        public static byte[] EncodeSector(byte[] sector)
        {
            ArgumentNullException.ThrowIfNull(sector, nameof(sector));

            byte[] buf = new byte[342];
            byte[] aux = new byte[86];

            for (int i = 0; i < 256; i++)
            {
                byte v = sector[i];
                aux[i % 86] |= (byte)(((v >> 1) & 1) << (i / 86));
                aux[(i + 86) % 86] |= (byte)(((v >> 3) & 1) << (i / 86));
                aux[(i + 172) % 86] |= (byte)(((v >> 5) & 1) << (i / 86));
                buf[i] = (byte)((v >> 2) & 0x3F);
            }

            for (int i = 0; i < 86; i++)
            {
                buf[256 + i] = aux[i];
            }

            byte prev = 0;
            for (int i = 0; i < 342; i++)
            {
                byte v = buf[i];
                buf[i] = EncodeTable[prev ^ v];
                prev = v;
            }

            return buf;
        }

        public static byte[] BuildTrack(byte[] dsk, int track)
        {
            byte[] trackData = new byte[6656];
            int pos = 0;

            for (int sector = 0; sector < 16; sector++)
            {
                void WriteSync(int count)
                {
                    for (int i = 0; i < count; i++)
                    {
                        trackData[pos++] = 0xFF;
                    }
                }

                void WriteAddressField()
                {
                    trackData[pos++] = 0xD5;
                    trackData[pos++] = 0xAA;
                    trackData[pos++] = 0x96;

                    void WriteOddEven(byte v)
                    {
                        trackData[pos++] = (byte)(0xAA | (v >> 1));
                        trackData[pos++] = (byte)(0xAA | v);
                    }

                    WriteOddEven((byte)track);
                    WriteOddEven((byte)sector);
                    WriteOddEven(0xFE); // volume

                    trackData[pos++] = 0xDE;
                    trackData[pos++] = 0xAA;
                    trackData[pos++] = 0xEB;
                }

                void WriteDataField(byte[] sectorData)
                {
                    trackData[pos++] = 0xD5;
                    trackData[pos++] = 0xAA;
                    trackData[pos++] = 0xAD;

                    foreach (var b in sectorData)
                    {
                        trackData[pos++] = b;
                    }

                    trackData[pos++] = 0xDE;
                    trackData[pos++] = 0xAA;
                    trackData[pos++] = 0xEB;
                }

                WriteSync(40);
                WriteAddressField();

                WriteSync(10);

                int offset = (track * 16 + sector) * 256;
                var raw = new byte[256];
                Array.Copy(dsk, offset, raw, 0, 256);

                var encoded = EncodeSector(raw);
                WriteDataField(encoded);
            }

            while (pos < trackData.Length)
            {
                trackData[pos++] = 0xFF;
            }

            return trackData;
        }

        public static void LoadDisk(DiskIIDrive drive, byte[] dsk)
        {
            ArgumentNullException.ThrowIfNull(drive, nameof(drive));

            for (int t = 0; t < 35; t++)
            {
                drive.Tracks[t] = BuildTrack(dsk, t);
            }
        }
    }
}
