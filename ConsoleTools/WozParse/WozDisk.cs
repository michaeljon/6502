using WozParse.Chunks;

#pragma warning disable CA1819 // Properties should not return arrays

namespace WozParse
{
    public class WozDisk
    {
        public byte[] RawBytes { get; set; }

        public Info Info { get; set; }

        public TrackMap TrackMap { get; set; }

        public Tracks Tracks { get; set; }

        public byte ReadByte(int halfTrack, int byteOffset)
        {
            var trackIndex = TrackMap.GetTrackIndex(halfTrack);
            var track = Tracks.GetTrack(trackIndex);
            return track.GetByte(byteOffset);
        }
    }
}
