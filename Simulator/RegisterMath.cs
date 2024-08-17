namespace InnoWerks.Simulators
{
    public static class RegisterMath
    {
        public static byte Inc(byte a) => (byte)((a + 1) & 0xff);
        public static byte Dec(byte a) => (byte)((a - 1) & 0xff);

        public static bool IsZero(int a) => (a & 0xff) == 0x00;
        public static bool IsNonZero(int a) => (a & 0xff) != 0x00;
        public static bool IsHighBitSet(int a) => (a & 0x80) == 0x80;
        public static bool IsHighBitClear(int a) => (a & 0x80) == 0x00;
        public static byte TruncateToByte(int a) => (byte)(a & 0xff);

        public static byte LowByte(ushort a) => (byte)(a & 0xff);
        public static byte HighByte(ushort a) => (byte)((a >> 8) & 0xff);
        public static ushort MakeShort(byte hi, byte lo) => (ushort)((hi << 8) | lo);
    }
}
