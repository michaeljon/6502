using System;
using System.Runtime.InteropServices;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace WozParse.Chunks
{
    public struct Preamble : IEquatable<Preamble>
    {
        public uint chunk_id;
        public byte check_byte;
        public byte[] crlf;
        public uint crc;

        public static Preamble Read(Span<byte> bytes, ref int streamPosition)
        {
            var preamble = new Preamble();

            preamble.chunk_id = BitConverter.ToUInt32(bytes.Slice(0)); streamPosition += 4;

            if (preamble.chunk_id != (uint)ChunkId.Preamble)
            {
                throw new InvalidChunkIdException(ChunkId.Preamble, preamble.chunk_id);
            }

            preamble.check_byte = bytes[streamPosition++];
            preamble.crlf = bytes.Slice(streamPosition, 3).ToArray(); streamPosition += 3;
            preamble.crc = BitConverter.ToUInt32(bytes.Slice(streamPosition, 4)); streamPosition += 4;

            return preamble;
        }

        public override readonly bool Equals(object obj)
        {
            return ((Preamble)obj).chunk_id == chunk_id;
        }

        public override readonly int GetHashCode()
        {
            return chunk_id.GetHashCode();
        }

        public readonly bool Equals(Preamble other)
        {
            return other.chunk_id == chunk_id;
        }

        public static bool operator ==(Preamble left, Preamble right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Preamble left, Preamble right)
        {
            return !(left == right);
        }
    }
}
