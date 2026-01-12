using System;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class DiskIISlotDevice : SlotRomDevice
    {
        // Disk state (simplified)
        private readonly byte[] trackBuffer = new byte[256];  // single track buffer
        private int trackPosition;
        private byte currentDrive;

        // Control latches (simplified)
        private bool driveMotorOn;
        private bool writeEnabled;

        public DiskIISlotDevice(byte[] romImage)
            : base(6, "Disk II", romImage) { }

        public override bool Handles(ushort address)
        {
            return IsIoReadRequest(address) || IsRomReadRequest(address);
        }

        public override byte Read(ushort address)
        {
            if (IsRomReadRequest(address))
            {
                return ReadSlotRom(address);
            }

            switch (address - IoBaseAddressLo)
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

        public override void Write(ushort address, byte value)
        {
            switch (address - IoBaseAddressLo)
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

        public override void Reset()
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
