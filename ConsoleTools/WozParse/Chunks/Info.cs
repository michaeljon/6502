using System;
using System.Text;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace WozParse.Chunks
{
    public enum BootSectorFormat
    {
        Unknown = 0,
        Sectors16 = 1,
        Sectors13 = 2,
        Both = 3
    }

    [Flags]
    public enum HardwareType
    {
        AppleII = 0x0001,
        AppleIIPlus = 0x0002,
        AppleIIe = 0x0004,
        AppleIIc = 0x0008,
        AppleIIeEnhanced = 0x0010,
        AppleIIgs = 0x0020,
        AppleIIcPlus = 0x0040,
        AppleIII = 0x0080,
        AppleIIIPlus = 0x0100,
    }

    public enum DiskType
    {
        Unknown = 0,

        Disk525 = 1,

        Disk35 = 2
    }

    public struct Info : IEquatable<Info>
    {
        public uint chunk_id;

        public uint chunk_size;

        public byte version;

        public DiskType disk_type;

        public bool write_protected;

        public bool synchronized;

        public bool cleaned;

        public string creator;

        public byte sides;

        public BootSectorFormat boot_sector_format;

        public byte optimal_bit_timing;

        public HardwareType compatible_hardware;

        public ushort required_ram;

        public ushort largest_track;

        public ushort flux_block;

        public ushort largest_flux_track;

        public static Info Read(ReadOnlySpan<byte> bytes, ref int streamPosition)
        {
            var initialPosition = streamPosition;

            var instance = new Info();

            instance.chunk_id = BitConverter.ToUInt32(bytes.Slice(streamPosition)); streamPosition += 4;

            if (instance.chunk_id != (uint)ChunkId.Info)
            {
                throw new InvalidChunkIdException(ChunkId.Unknown, instance.chunk_id);
            }

            instance.chunk_size = BitConverter.ToUInt32(bytes.Slice(streamPosition)); streamPosition += 4;
            instance.version = bytes[streamPosition++];
            instance.disk_type = (DiskType)bytes[streamPosition++];
            instance.write_protected = bytes[streamPosition++] == 1;
            instance.synchronized = bytes[streamPosition++] == 1;
            instance.cleaned = bytes[streamPosition++] == 1;

            // this is just ugly
            instance.creator = Encoding.ASCII.GetString(bytes.Slice(streamPosition, 32).ToArray()).Trim();
            streamPosition += 32;

            if (instance.version >= 2)
            {
                instance.sides = bytes[streamPosition++];
                instance.boot_sector_format = (BootSectorFormat)bytes[streamPosition++];
                instance.optimal_bit_timing = bytes[streamPosition++];
                instance.compatible_hardware = (HardwareType)bytes[streamPosition]; streamPosition += 2;
                instance.required_ram = bytes[streamPosition++]; streamPosition += 2;
                instance.largest_track = bytes[streamPosition++]; streamPosition += 2;

                if (instance.version >= 3)
                {
                    instance.flux_block = bytes[streamPosition++]; streamPosition += 2;
                    instance.largest_flux_track = bytes[streamPosition++]; streamPosition += 2;
                }
            }

            streamPosition = initialPosition + (int)instance.chunk_size + 8;

            return instance;
        }

        public override readonly bool Equals(object obj)
        {
            return ((Info)obj).chunk_id == chunk_id;
        }

        public override readonly int GetHashCode()
        {
            return chunk_id.GetHashCode();
        }

        public readonly bool Equals(Info other)
        {
            return other.chunk_id == chunk_id;
        }

        public static bool operator ==(Info left, Info right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Info left, Info right)
        {
            return !(left == right);
        }
    }
}
