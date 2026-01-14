namespace InnoWerks.Emulators.Apple
{
    public enum SoftSwitch
    {
        // Keyboard & system
        KeyboardData,
        KeyboardStrobe,
        Speaker,
        TapeOut,
        TapeIn,
        GameStrobe,

        Annunciator0,
        Annunciator1,
        Annunciator2,
        Annunciator3,

        Button0,
        Button1,
        Button2,

        // for the IIe only
        OpenApple,
        SolidApple,
        ShiftKey,

        Paddle0,
        Paddle1,
        Paddle2,
        Paddle3,

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
        AuxRamEnabled,

        VerticalBlank,

        // ROM & slot control
        SlotRomEnabled,
        Slot3RomEnabled,
        IntC8RomEnabled,
        ReadRomWriteRam,

        // Language Card banks
        LanguageCardEnabled,
        LcBank1,
        LcReadEnabled,
        LcWriteEnabled,
    }
}
