using System;
using System.IO;

namespace WozParse
{
    public enum ChunkId
    {
        Unknown,
        Preamble = 0x325A4F57,
        Info = 0x4F464E49,
        TrackMap = 0x50414D54,
        Tracks = 0x534B5254,
        Write = 0x54495257,
        Meta = 0x4154454D,
    }

    public static class WozParser
    {
        public static void Parse(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            using var fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
            var bytes = new byte[fileStream.Length];
            var span = new ReadOnlySpan<byte>(bytes);

            fileStream.ReadExactly(bytes);

            while (fileStream.Length - fileStream.Position > 8)
            {
                uint chunk_id = BitConverter.ToUInt32(span.Slice(0, 4));
                uint chunk_size = BitConverter.ToUInt32(span.Slice(4, 4));

                switch ((ChunkId)chunk_id)
                {
                    case ChunkId.Info:
                        // read the INFO chunk
                        break;
                    case ChunkId.TrackMap:
                        // read the TMAP chunk
                        break;
                    case ChunkId.Tracks:
                        // read the TRKS chunk
                        break;
                    case ChunkId.Write:
                        // read the WRIT chunk
                        break;
                    case ChunkId.Meta:
                        // read the META chunk
                        break;
                    default:
                        // no idea what this chunk is, so skip it
                        fileStream.Seek(chunk_size, SeekOrigin.Current);
                        break;
                }
            }
        }
    }
}
