namespace InnoWerks.Simulators
{
    public static class RegisterMath
    {
        public static byte Add(byte a, byte b) => (byte)(a + b);
        public static byte Subtract(byte a, byte b) => (byte)(a - b);
        public static byte And(byte a, byte b) => (byte)(a & b);
        public static byte Or(byte a, byte b) => (byte)(a | b);
        public static byte XOr(byte a, byte b) => (byte)(a ^ b);
        public static byte Inc(byte a) => (byte)((a + 1) & 0xff);
        public static byte Dec(byte a) => (byte)((a - 1) & 0xff);
        public static bool IsZero(byte a) => a == 0x00;
        public static bool IsHighBitSet(byte a) => (a & 0x80) == 0x80;
        public static bool IsHighBitClear(byte a) => (a & 0x80) == 0x00;
        public static byte TruncateToByte(byte a) => (byte)(a & 0xff);

        public static byte Add(short a, short b) => (byte)(a + b);
        public static byte Subtract(short a, short b) => (byte)(a - b);
        public static byte And(short a, short b) => (byte)(a & b);
        public static byte Or(short a, short b) => (byte)(a | b);
        public static byte XOr(short a, short b) => (byte)(a ^ b);
        public static byte Inc(short a) => (byte)((a + 1) & 0xff);
        public static byte Dec(short a) => (byte)((a - 1) & 0xff);
        public static bool IsZero(short a) => (a & 0xff) == 0x00;
        public static bool IsHighBitSet(short a) => (a & 0x80) == 0x80;
        public static bool IsHighBitClear(short a) => (a & 0x80) == 0x00;
        public static byte TruncateToByte(short a) => (byte)(a & 0xff);

        public static byte Add(ushort a, ushort b) => (byte)(a + b);
        public static byte Subtract(ushort a, ushort b) => (byte)(a - b);
        public static byte And(ushort a, ushort b) => (byte)(a & b);
        public static byte Or(ushort a, ushort b) => (byte)(a | b);
        public static byte XOr(ushort a, ushort b) => (byte)(a ^ b);
        public static byte Inc(ushort a) => (byte)((a + 1) & 0xff);
        public static byte Dec(ushort a) => (byte)((a - 1) & 0xff);
        public static bool IsZero(ushort a) => (a & 0xff) == 0x00;
        public static bool IsHighBitSet(ushort a) => (a & 0x80) == 0x80;
        public static bool IsHighBitClear(ushort a) => (a & 0x80) == 0x00;
        public static byte TruncateToByte(ushort a) => (byte)(a & 0xff);

        public static byte Add(int a, int b) => (byte)(a + b);
        public static byte Subtract(int a, int b) => (byte)(a - b);
        public static byte And(int a, int b) => (byte)(a & b);
        public static byte Or(int a, int b) => (byte)(a | b);
        public static byte XOr(int a, int b) => (byte)(a ^ b);
        public static byte Inc(int a) => (byte)((a + 1) & 0xff);
        public static byte Dec(int a) => (byte)((a - 1) & 0xff);
        public static bool IsZero(int a) => (a & 0xff) == 0x00;
        public static bool IsHighBitSet(int a) => (a & 0x80) == 0x80;
        public static bool IsHighBitClear(int a) => (a & 0x80) == 0x00;
        public static byte TruncateToByte(int a) => (byte)(a & 0xff);

        public static byte Add(uint a, uint b) => (byte)(a + b);
        public static byte Subtract(uint a, uint b) => (byte)(a - b);
        public static byte And(uint a, uint b) => (byte)(a & b);
        public static byte Or(uint a, uint b) => (byte)(a | b);
        public static byte XOr(uint a, uint b) => (byte)(a ^ b);
        public static byte Inc(uint a) => (byte)((a + 1) & 0xff);
        public static byte Dec(uint a) => (byte)((a - 1) & 0xff);
        public static bool IsZero(uint a) => (a & 0xff) == 0x00;
        public static bool IsHighBitSet(uint a) => (a & 0x80) == 0x80;
        public static bool IsHighBitClear(uint a) => (a & 0x80) == 0x00;
        public static byte TruncateToByte(uint a) => (byte)(a & 0xff);

        public static byte LowByte(ushort a) => (byte)(a & 0xff);
        public static byte HighByte(ushort a) => (byte)((a >> 8) & 0xff);
        public static ushort MakeShort(byte hi, byte lo) => (ushort)((hi << 8) | lo);
    }
}
