namespace InnoWerks.Simulators
{
    public interface IBus
    {
        /// <summary>
        /// Starts a memory transaction to record a single
        /// CPU step's cycle count. Initializes the transaction
        /// cycle count to 0.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Ends a memory transaction for a single CPU step
        /// and returns the number of memory accesses that
        /// occurred during that transaction.
        /// </summary>
        /// <returns></returns>
        int EndTransaction();

        /// <summary>
        /// Returns the current CPU cycle count since the
        /// last CPU reset.
        /// </summary>
        long CycleCount { get; }

        /// <summary>
        /// Allows for a non-cycle impacting read on the bus.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        byte Peek(ushort address);

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
        /// Allows for a non-cycle impacting read on the bus.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        ushort PeekWord(ushort address);

        /// <summary>
        /// Shortcut method to correctly read a word from a memory location.
        /// Updates the cycle count accordingly.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        ushort ReadWord(ushort address);

        /// <summary>
        /// Writes a word to memory.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        void WriteWord(ushort address, ushort value);

        /// <summary>
        /// Copies a "program" into RAM / ROM at the specified origin. This
        /// method does not impact cycle counts.
        /// </summary>
        /// <param name="objectCode"></param>
        /// <param name="origin"></param>
        void LoadProgram(byte[] objectCode, ushort origin);

        /// <summary>
        /// Direct access to the underlying memory structures. This is for
        /// primarily debug and test access without incurring any overhead.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        byte this[ushort address] { get; set; }
    }
}
