namespace InnoWerks.Emulators.Apple
{
#pragma warning disable CA1707
    public static class SoftSwitch
    {
        public const ushort BASE = 0xC000;
        public const ushort TOP = 0xC0FF;

        // MEMORY MANAGEMENT SOFT SWITCHES

        public const ushort KBD = 0xC000;
        public const ushort CLR80STORE = 0xC000;  // Allow page2 to switch video page1 page2
        public const ushort SET80STORE = 0xC001;  // Allow page2 to switch main & aux video memory
        public const ushort RAMRDOFF = 0xC002;  // Read enable main memory from $0200-$BFFF
        public const ushort RAMDRON = 0xC003;  //  Read enable aux memory from $0200-$BFFF
        public const ushort RAMWRTOFF = 0xC004;  // Write enable main memory from $0200-$BFFF
        public const ushort RAMWRTON = 0xC005;  // Write enable aux memory from $0200-$BFFF
        public const ushort INTCXROMOFF = 0xC006;  // Enable slot ROM from $C100-$CFFF
        public const ushort INTCXROMON = 0xC007;  // Enable main ROM from $C100-$CFFF
        public const ushort ALZTPOFF = 0xC008;  // Enable main memory from $0000-$01FF & avl BSR
        public const ushort ALTZPON = 0xC009;  //  Enable aux memory from $0000-$01FF & avl BSR
        public const ushort SLOTC3ROMOFF = 0xC00A;  // Enable main ROM from $C300-$C3FF
        public const ushort SLOTC3ROMON = 0xC00B;  // Enable slot ROM from $C300-$C3FF
        public const ushort IO_RROMBNK2 = 0xC082;

        // VIDEO SOFT SWITCHES

        public const ushort CLR80DISP = 0xC00C;  // Turn off 80 column display
        public const ushort SET80DISP = 0xC00D;  //  Turn on 80 column display
        public const ushort CLRALTCHAR = 0xC00E;  // Turn off alternate characters
        public const ushort SETALTCHAR = 0xC00F;  // Turn on alternate characters
        public const ushort TEXTOFF = 0xC050;  //  Select graphics mode
        public const ushort TEXTON = 0xC051;  //  Select text mode
        public const ushort MIXEDOFF = 0xC052;  // Use full screen for graphics
        public const ushort MIXEDON = 0xC053;  //  Use graphics with 4 lines of text
        public const ushort PAGE2OFF = 0xC054;  // Select panel display (or main video memory)
        public const ushort PAGE2ON = 0xC055;  //  Select page2 display (or aux video memory)
        public const ushort HIRESOFF = 0xC056;  // Select low resolution graphics
        public const ushort HIRESON = 0xC057;  //  Select high resolution graphics

        // SOFT SWITCH STATUS FLAGS

        public const ushort KBDSTROBE = 0xC010;  //  1=key pressed 0=keys free    (clears strobe)
        public const ushort BSRBANK2 = 0xC011;  // 1=bank2 available    0=bank1 available
        public const ushort BSRREADRAM = 0xC012;  // 1=BSR active for read 0=$D000-$FFFF active
        public const ushort RAMRD = 0xC013;  //  0=main $0200-$BFFF active reads  1=aux active
        public const ushort RAMWRT = 0xC014;  //  0=main $0200-$BFFF active writes 1=aux writes
        public const ushort INTCXROM = 0xC015;  // 1=main $C100-$CFFF ROM active 0=slot active
        public const ushort ALTZP = 0xC016;  //  1=aux $0000-$1FF+auxBSR    0=main available
        public const ushort SLOTC3ROM = 0xC017;  // 1=slot $C3 ROM active 0=main $C3 ROM active
        public const ushort RD80STORE = 0xC018;  //  1=page2 switches main/aux   0=page2 video
        public const ushort VERTBLANK = 0xC019;  // 1=vertical retrace on 0=vertical retrace off
        public const ushort TEXT = 0xC01A;  //  1=text mode is active 0=graphics mode active
        public const ushort MIXED = 0xC01B;  //  1=mixed graphics & text    0=full screen
        public const ushort PAGE2 = 0xC01C;  //  1=video page2 selected or aux
        public const ushort HIRES = 0xC01D;  //  1=high resolution graphics   0=low resolution
        public const ushort RDALTCHR = 0xC01E;  // 1=alt character set on   0=alt char set off
        public const ushort R80DISPL = 0xC01F;  //  1=80 col display on 0=80 col display off

        public const ushort OPENAPPLE = 0xC061;
        public const ushort SOLIDAPPLE = 0xC062;
    }
#pragma warning restore CA1707
}
