namespace InnoWerks.Computers.Apple
{
    public class DiskIIDrive
    {
        public byte[][] Tracks = new byte[35][];

        public int CurrentTrack;

        public int BitPosition;

        public bool WriteProtected;

        public DiskIIDrive()
        {
            // DOS 3.3 nibble track (~6656 bytes)
            for (int i = 0; i < 35; i++)
            {
                Tracks[i] = new byte[6656];
            }
        }
    }
}
