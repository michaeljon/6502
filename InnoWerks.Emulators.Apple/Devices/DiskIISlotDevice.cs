using System;
using System.Linq;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class DiskIISlotDevice : IDevice
    {
        private const ushort IO_BASE = 0xC080; // Disk II I/O base
        private const ushort SLOT6_OFFSET = 0x080;

        // Disk state (simplified)
        private readonly byte[] trackBuffer = new byte[256];  // single track buffer
        private int trackPosition;
        private byte currentDrive;

        // Control latches (simplified)
        private bool driveMotorOn;
        private bool writeEnabled;

        private readonly byte[] rom; // 256-byte slot ROM

        public DevicePriority Priority => DevicePriority.Slot;

        public DiskIISlotDevice(byte[] romImage)
        {
            ArgumentNullException.ThrowIfNull(romImage, nameof(romImage));

            if (romImage.Length < 256)
            {
                throw new ArgumentException("Disk II ROM must be at least 256 bytes");
            }

            rom = romImage;
        }

        public bool Handles(ushort address)
        {
            return (address >= IO_BASE + SLOT6_OFFSET && address <= IO_BASE + SLOT6_OFFSET + 0x0F) // I/O registers
                   || (address >= 0xC800 && address <= 0xC8FF);
        }

        public byte Read(ushort address)
        {
            if (address >= 0xC800 && address <= 0xC8FF)
            {
                return rom[address - 0xC800];
            }

            if (address >= IO_BASE + SLOT6_OFFSET && address <= IO_BASE + SLOT6_OFFSET + 0x0F)
            {
                // handle disk I/O register reads here
                return 0x00; // placeholder
            }

            switch (address - IO_BASE)
            {
                case 0x0C: // DATA register ($C08C)
                    // Return next byte from track buffer
                    byte b = trackBuffer[trackPosition];
                    trackPosition = (trackPosition + 1) % trackBuffer.Length;
                    return b;

                case 0x00: // STATUS ($C080)
                    return driveMotorOn ? (byte)0x01 : (byte)0x00;

                case 0x01: // TRACK ($C081)
                    return (byte)trackPosition;

                default:
                    // Other registers (CONTROL, DRIVE SELECT, etc.) are stubbed for now
                    return 0;
            }
        }

        public void Write(ushort address, byte value)
        {
            switch (address - IO_BASE)
            {
                case 0x00: // CONTROL / STATUS command
                    // Simplified: interpret bits
                    driveMotorOn = (value & 0x01) != 0;
                    writeEnabled = (value & 0x02) != 0;
                    break;

                case 0x01: // TRACK select
                    trackPosition = 0; // reset to start of track for simplicity
                    currentDrive = value;
                    break;

                case 0x0C: // DATA register ($C08C) - writes only if write enabled
                    if (writeEnabled)
                    {
                        trackBuffer[trackPosition] = value;
                        trackPosition = (trackPosition + 1) % trackBuffer.Length;
                    }
                    break;

                default:
                    // ignore other writes for now
                    break;
            }
        }

        public void Reset()
        {
            trackPosition = 0;
            currentDrive = 0;

            driveMotorOn = false;
            writeEnabled = false;
        }

        // Optional: allow loading a track from a file
        public void LoadTrack(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            Array.Copy(data, trackBuffer, Math.Min(data.Length, trackBuffer.Length));
            trackPosition = 0;
        }
    }
}
