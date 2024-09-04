namespace InnoWerks.Simulators
{
    public interface IMemory
    {
        byte Read(ushort address);

        byte Peek(ushort address);

        void Write(ushort address, byte value);

        ushort ReadWord(ushort address);

        ushort PeekWord(ushort address);

        void WriteWord(ushort address, ushort value);

        void LoadProgram(byte[] objectCode, ushort origin);

        byte this[ushort address] { get; set; }
    }
}
