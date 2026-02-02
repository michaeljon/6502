using System;
using System.Runtime.InteropServices;

namespace WozParse.Chunks
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Chunk : IEquatable<Chunk>
    {
        /// <summary>
        /// The ASCII string ‘WOZ2’. 0x325A4F57
        /// </summary>
        [FieldOffset(0)]
        public uint chunk_id;

        /// <summary>
        /// Make sure that high bits are valid (no 7-bit data transmission)
        /// </summary>
        [FieldOffset(4)]
        public uint chunk_size;

        public readonly string ChunkId()
        {
            return chunk_id switch
            {
                0x4F464E49 => "INFO",
                0x50414D54 => "TMAP",
                0x534B5254 => "TRKS",
                0x54495257 => "WRIT",
                0x4154454D => "META",

                _ => throw new ArgumentOutOfRangeException(nameof(chunk_id), chunk_id, "The chunk_id is not recognized by this parser"),
            };
        }

        public override readonly bool Equals(object obj)
        {
            return ((Chunk)obj).chunk_id == chunk_id;
        }

        public override readonly int GetHashCode()
        {
            return chunk_id.GetHashCode();
        }

        public readonly bool Equals(Chunk other)
        {
            return other.chunk_id == chunk_id;
        }

        public static bool operator ==(Chunk left, Chunk right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Chunk left, Chunk right)
        {
            return !(left == right);
        }
    }
}
