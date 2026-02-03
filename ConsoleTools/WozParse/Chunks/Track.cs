using System;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace WozParse.Chunks
{
    public struct Track : IEquatable<Track>
    {
        public ushort starting_block;

        public ushort block_count;

        public uint bit_count;

        public byte[] track_data;

        public readonly byte GetByte(int byteOffset)
        {
            return track_data[byteOffset];
        }

        public override readonly bool Equals(object obj)
        {
            return ((Track)obj).starting_block == starting_block;
        }

        public override readonly int GetHashCode()
        {
            return starting_block.GetHashCode();
        }

        public readonly bool Equals(Track other)
        {
            return other.starting_block == starting_block;
        }

        public static bool operator ==(Track left, Track right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Track left, Track right)
        {
            return !(left == right);
        }
    }
}
