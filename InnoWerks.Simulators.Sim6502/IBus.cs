namespace InnoWerks.Simulators
{
    public interface IBus
    {
        void AddDevice(IDevice device);

        /// <summary>
        /// Starts a memory transaction to record a single
        /// CPU step's cycle count. Initializes the transaction
        /// cycle count to 0. This is a debug interface used
        /// for unit-testing instruction cycle counts.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Ends a memory transaction for a single CPU step
        /// and returns the number of memory accesses that
        /// occurred during that transaction. This is a debug
        /// interface used for unit-testing instruction cycle counts.
        /// </summary>
        /// <returns></returns>
        int EndTransaction();

        /// <summary>
        /// Returns the current CPU cycle count since the
        /// last CPU reset.
        /// </summary>
        ulong CycleCount { get; }

        /// <summary>
        /// Allows for a non-cycle impacting read on the bus. This is
        /// used for debug and CPU-internal access the underlying memory.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        byte Peek(ushort address);

        /// <summary>
        /// Allows for a non-cycle impacting write to the bus. This is
        /// used for debug and CPU-internal access to the underlying memory.
        /// /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        void Poke(ushort address, byte value);

        /// <summary>
        /// Reads a byte from the address and updates the cycle count. This
        /// operation may read RAM, ROM, an I/O port, or a slot.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        byte Read(ushort address);

        /// <summary>
        /// Writes a byte to the address and updates the cycle count. This
        /// operation may write RAM, an I/O port, or a slot.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        void Write(ushort address, byte value);

        /// <summary>
        /// Copies a "program" into ROM.
        /// This method does not impact cycle counts. Programs that are destined
        /// for a ROM address (> 0xd000) ignore the origin and assume that the
        /// ROM file includes the current initialization, reset, and nmi
        /// vectors already.
        /// </summary>
        /// <param name="objectCode"></param>
        void LoadProgramToRom(byte[] objectCode);

        /// <summary>
        /// Copies a "program" into RAM at the specified origin. For RAM-targeted
        /// loads the origin is written to the initialization vector and is used
        /// for starting the program.
        ///
        /// This allows for custom ROMs and non-Apple code to use this bus
        /// and the attached CPU.
        /// </summary>
        /// <param name="objectCode"></param>
        /// <param name="origin"></param>
        void LoadProgramToRam(byte[] objectCode, ushort origin);
    }
}
