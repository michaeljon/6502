namespace InnoWerks.Computers.Apple
{
#pragma warning disable CA2211 // Non-constant fields should not be visible

    public static class FloppyDisk
    {
        public static int TRACK_NIBBLE_LENGTH = 0x1A00;
        public static int TRACK_COUNT = 35;
        public static int SECTOR_COUNT = 16;
        public static int HALF_TRACK_COUNT = TRACK_COUNT * 2;
        public static int DISK_NIBBLE_LENGTH = TRACK_NIBBLE_LENGTH * TRACK_COUNT;
        public static int DISK_PLAIN_LENGTH = 143360;
        public static int DISK_2MG_NON_NIB_LENGTH = DISK_PLAIN_LENGTH + 0x040;
        public static int DISK_2MG_NIB_LENGTH = DISK_NIBBLE_LENGTH + 0x040;
    }
}
