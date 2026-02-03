using System;
using System.Collections.Generic;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace WozParse.Chunks
{
    public struct Tracks : IEquatable<Tracks>
    {
        public uint chunk_id;

        public uint chunk_size;

        private Track[] tracks;

        public static Tracks Read(ReadOnlySpan<byte> bytes, ref int streamPosition)
        {
            var initialPosition = streamPosition;

            var instance = new Tracks();

            instance.chunk_id = BitConverter.ToUInt32(bytes.Slice(streamPosition)); streamPosition += 4;

            if (instance.chunk_id != (uint)ChunkId.Tracks)
            {
                throw new InvalidChunkIdException(ChunkId.Tracks, instance.chunk_id);
            }

            instance.chunk_size = BitConverter.ToUInt32(bytes.Slice(streamPosition)); streamPosition += 4;

            // get the disk type
            var disk_type = (DiskType)bytes[21];   // todo: define this magic number

            // todo: remove magic number
            // var track_count = disk_type == DiskType.Disk525 ? 35 : 160;
            var track_count = 160;

            instance.tracks = new Track[track_count];
            for (var n = 0; n < track_count; n++)
            {
                var track = new Track();

                track.starting_block = BitConverter.ToUInt16(bytes.Slice(streamPosition + 0));
                track.block_count = BitConverter.ToUInt16(bytes.Slice(streamPosition + 2));
                track.bit_count = BitConverter.ToUInt32(bytes.Slice(streamPosition + 4));

                var first_block_offset = track.starting_block * 512;    // location of block relative to file start
                var num_bytes_in_track = track.block_count * 512;       // 512 bytes per block, per spec

                track.track_data = bytes.Slice(first_block_offset, num_bytes_in_track).ToArray();

                instance.tracks[n] = track;

                streamPosition += 8;
            }

            streamPosition = initialPosition + (int)instance.chunk_size + 8;

            return instance;
        }

        public readonly Track GetTrack(int trackIndex)
        {
            return tracks[trackIndex];
        }

        public override readonly bool Equals(object obj)
        {
            return ((Tracks)obj).chunk_id == chunk_id;
        }

        public override readonly int GetHashCode()
        {
            return chunk_id.GetHashCode();
        }

        public readonly bool Equals(Tracks other)
        {
            return other.chunk_id == chunk_id;
        }

        public static bool operator ==(Tracks left, Tracks right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Tracks left, Tracks right)
        {
            return !(left == right);
        }
    }
}
