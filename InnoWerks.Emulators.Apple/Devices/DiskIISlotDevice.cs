using System;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
#pragma warning disable CA1051, IDE1006

    public class DiskIIDrive
    {
        public byte[][] Tracks = new byte[35][];

        public int CurrentTrack;

        public int BitPosition;

        public bool WriteProtected;

        public DiskIIDrive()
        {
            // DOS 3.3 nibble track (~6656 bytes)
            for (int i = 0; i < 35; i++)
            {
                Tracks[i] = new byte[6656];
            }
        }
    }

    public sealed class DiskIISlotDevice : SlotRomDevice
    {
        private readonly DiskIIDrive[] drives = new DiskIIDrive[2];

        private bool motorOn;

        private bool driveSelect;

        private int phase;

        private byte shiftRegister;

        private DiskIIDrive CurrentDrive =>
            driveSelect ? drives[0] : drives[1];

        public DiskIISlotDevice(SoftSwitches softSwitches, byte[] romImage)
            : base(6, "Disk II Controller", softSwitches, romImage)
        {
            drives[0] = new DiskIIDrive();
            drives[1] = new DiskIIDrive();
        }

        public override bool Handles(ushort address) =>
            (address & 0xFFF0) == 0xC0E0 || (address & 0xFF00) == 0xC600;

        public override byte Read(ushort address)
        {
            if ((address & 0xFF00) == 0xC600)
            {
                // this is really explicit and it's because disk ii is special
                return ReadSlotRom(address);
            }

            switch (address & 0x0F)
            {
                case 0x0C:   // shift read
                    ShiftRead();
                    break;

                case 0x0E:   // data read
                    return shiftRegister;

                default:
                    Handle(address);
                    break;
            }

            return 0x00;
        }

        public override void Write(ushort address, byte value)
        {
            switch (address & 0x0F)
            {
                case 0x0F:   // data write
                    shiftRegister = value;
                    return;

                default:
                    Handle(address);
                    return;
            }
        }

        public override void Tick(int cycles) {/* NO-OP */ }

        public override void Reset()
        {
            motorOn = false;
            driveSelect = false;
            phase = 0;
            shiftRegister = 0;
        }

        public DiskIIDrive GetDrive(int drive)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(drive, nameof(drive));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(drive, 2, nameof(drive));

            return drives[drive];
        }

        private void Handle(ushort address)
        {
            switch (address & 0x0F)
            {
                case 0x00: SetPhase(0, false); break;
                case 0x01: SetPhase(0, true); break;
                case 0x02: SetPhase(1, false); break;
                case 0x03: SetPhase(1, true); break;
                case 0x04: SetPhase(2, false); break;
                case 0x05: SetPhase(2, true); break;
                case 0x06: SetPhase(3, false); break;
                case 0x07: SetPhase(3, true); break;

                case 0x08: motorOn = false; break;
                case 0x09: motorOn = true; break;

                case 0x0A: driveSelect = false; break;
                case 0x0B: driveSelect = true; break;
            }
        }

        private void SetPhase(int p, bool on)
        {
            if (!motorOn) return;

            if (on)
            {
                if (p == (phase + 1) % 4) { CurrentDrive.CurrentTrack++; }
                else if (p == (phase + 3) % 4) { CurrentDrive.CurrentTrack--; }

                CurrentDrive.CurrentTrack = Math.Clamp(CurrentDrive.CurrentTrack, 0, 34);
                phase = p;
            }
        }

        private void ShiftRead()
        {
            if (!motorOn) return;

            var drive = CurrentDrive;
            var track = drive.Tracks[drive.CurrentTrack];

            int bytePos = drive.BitPosition >> 3;
            int bitPos = 7 - (drive.BitPosition & 7);

            int bit = (track[bytePos] >> bitPos) & 1;

            shiftRegister = (byte)((shiftRegister << 1) | bit);

            drive.BitPosition = (drive.BitPosition + 1) % (track.Length * 8);
        }
    }

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
