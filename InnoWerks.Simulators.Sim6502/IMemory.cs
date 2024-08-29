namespace InnoWerks.Simulators
{
    public interface IMemory
    {
        byte Read(ushort address, bool countAccess = true);

        void Write(ushort address, byte value, bool countAccess = true);

        void LoadProgram(byte[] objectCode, ushort origin);

        byte this[ushort address] { get; set; }
    }
}
