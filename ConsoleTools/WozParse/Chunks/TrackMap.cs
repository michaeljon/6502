using System;
using System.Collections.Generic;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace WozParse.Chunks
{
    public struct TrackMap : IEquatable<TrackMap>
    {
        public uint chunk_id;

        public uint chunk_size;

        private DiskType disk_type;

        private byte[] track_map;

        public static TrackMap Read(ReadOnlySpan<byte> bytes, ref int streamPosition)
        {
            var initialPosition = streamPosition;

            var instance = new TrackMap();

            instance.chunk_id = BitConverter.ToUInt32(bytes.Slice(streamPosition)); streamPosition += 4;

            if (instance.chunk_id != (uint)ChunkId.TrackMap)
            {
                throw new InvalidChunkIdException(ChunkId.TrackMap, instance.chunk_id);
            }

            instance.chunk_size = BitConverter.ToUInt32(bytes.Slice(streamPosition)); streamPosition += 4;

            // get the disk type
            instance.disk_type = (DiskType)bytes[21];   // todo: define this magic number

            switch (instance.disk_type)
            {
                case DiskType.Unknown:
                    // complain loudly
                    break;

                case DiskType.Disk525:
                    // todo: fix magic numbers, deal with 40 track disks
                    instance.track_map = bytes.Slice(88, 140).ToArray();
                    break;

                case DiskType.Disk35:
                    // todo: fix magic numbers
                    instance.track_map = bytes.Slice(88, 159).ToArray();
                    break;
            }

            streamPosition = initialPosition + (int)instance.chunk_size + 8;

            return instance;
        }

        public readonly byte GetTrackIndex(int side, int track)
        {
            if (disk_type != DiskType.Disk35)
            {
                throw new NotImplementedException("This method is only valid for 3.5\" disks");
            }

            return track_map[track * 2 + side];
        }

        public readonly byte GetTrackIndex(int quarterTrack)
        {
            if (disk_type != DiskType.Disk525)
            {
                throw new NotImplementedException("This method is only valid for 5.25\" disks");
            }

            return track_map[quarterTrack];
        }

        public override readonly bool Equals(object obj)
        {
            return ((TrackMap)obj).chunk_id == chunk_id;
        }

        public override readonly int GetHashCode()
        {
            return chunk_id.GetHashCode();
        }

        public readonly bool Equals(TrackMap other)
        {
            return other.chunk_id == chunk_id;
        }

        public static bool operator ==(TrackMap left, TrackMap right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TrackMap left, TrackMap right)
        {
            return !(left == right);
        }
    }
}
