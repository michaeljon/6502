namespace InnoWerks.Emulators.Apple
{
    public enum SoftSwitch
    {
        // Video
        TextMode,
        MixedMode,
        Page2,
        HiRes,
        DoubleHiRes,
        IOU,

        // 80 column / aux
        Store80,
        AuxRead,
        AuxWrite,
        ZpAux,
        StackAux,
        EightyColumnFirmware,
        AltCharSet,

        VerticalBlank,

        // ROM & slot control
        SlotRomEnabled,
        Slot3RomEnabled,
        IntC8RomEnabled,

        LcBank1,
        LcReadEnabled,
        LcWriteEnabled,
    }
}
