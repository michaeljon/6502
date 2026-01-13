namespace InnoWerks.Emulators.Apple
{
#pragma warning disable CA1707
    public static class SoftSwitchAddress
    {
        //
        // see http://apple2.guidero.us/doku.php/mg_notes/general/io_page
        //

        public const ushort KBD = 0xC000;   // Last Key Pressed (+ 128 if strobe not cleared)

        // MEMORY MANAGEMENT SOFT SWITCHES

        public const ushort CLR80STORE = 0xC000;  // Allow page2 to switch video page1 page2
        public const ushort SET80STORE = 0xC001;  // Allow page2 to switch main & aux video memory
        public const ushort RDMAINRAM = 0xC002;  // Read enable main memory from $0200-$BFFF
        public const ushort RDCARDRAM = 0xC003;  //  Read enable aux memory from $0200-$BFFF
        public const ushort WRMAINRAM = 0xC004;  // Write enable main memory from $0200-$BFFF
        public const ushort WRCARDRAM = 0xC005;  // Write enable aux memory from $0200-$BFFF
        public const ushort SETSLOTCXROM = 0xC006;  // Enable slot ROM from $C100-$CFFF
        public const ushort SETINTCXROM = 0xC007;  // Enable main ROM from $C100-$CFFF
        public const ushort SETSTDZP = 0xC008;  // Enable main memory from $0000-$01FF & avl BSR
        public const ushort SETALTZP = 0xC009;  //  Enable aux memory from $0000-$01FF & avl BSR
        public const ushort SETINTC3ROM = 0xC00A;  // Enable main ROM from $C300-$C3FF
        public const ushort SETSLOTC3ROM = 0xC00B;  // Enable slot ROM from $C300-$C3FF

        // VIDEO SOFT SWITCHES

        public const ushort CLR80VID = 0xC00C;  // Turn off 80 column display
        public const ushort SET80VID = 0xC00D;  //  Turn on 80 column display
        public const ushort CLRALTCHAR = 0xC00E;  // Turn off alternate characters
        public const ushort SETALTCHAR = 0xC00F;  // Turn on alternate characters

        // SOFT SWITCH STATUS FLAGS

        public const ushort KBDSTRB = 0xC010;  //  1=key pressed 0=keys free    (clears strobe)
        public const ushort RDLCBNK2 = 0xC011;  // 1=bank2 available    0=bank1 available
        public const ushort RDLCRAM = 0xC012;  // 1=BSR active for read 0=$D000-$FFFF active
        public const ushort RDRAMRD = 0xC013;  //  0=main $0200-$BFFF active reads  1=aux active
        public const ushort RDRAMWRT = 0xC014;  //  0=main $0200-$BFFF active writes 1=aux writes
        public const ushort RDCXROM = 0xC015;  // 1=main $C100-$CFFF ROM active 0=slot active
        public const ushort RDALTZP = 0xC016;  //  1=aux $0000-$1FF+auxBSR    0=main available
        public const ushort RDC3ROM = 0xC017;  // 1=slot $C3 ROM active 0=main $C3 ROM active
        public const ushort RD80STORE = 0xC018;  //  1=page2 switches main/aux   0=page2 video
        public const ushort RDVBL = 0xC019;  // 1=vertical retrace on 0=vertical retrace off
        public const ushort RDTEXT = 0xC01A;  //  1=text mode is active 0=graphics mode active
        public const ushort RDMIXED = 0xC01B;  //  1=mixed graphics & text    0=full screen
        public const ushort RDPAGE2 = 0xC01C;  //  1=video page2 selected or aux
        public const ushort RDHIRES = 0xC01D;  //  1=high resolution graphics   0=low resolution
        public const ushort RDALTCHR = 0xC01E;  // 1=alt character set on   0=alt char set off
        public const ushort RD80VID = 0xC01F;  //  1=80 col display on 0=80 col display off

        public const ushort TAPEOUT = 0xC020;
        public const ushort SPKR = 0xC030;
        public const ushort STROBE = 0xC040;

        // VIDEO SOFT SWITCHES

        public const ushort TXTCLR = 0xC050;  //  Select graphics mode
        public const ushort TXTSET = 0xC051;  //  Select text mode
        public const ushort MIXCLR = 0xC052;  // Use full screen for graphics
        public const ushort MIXSET = 0xC053;  //  Use graphics with 4 lines of text
        public const ushort TXTPAGE1 = 0xC054;  // Select panel display (or main video memory)
        public const ushort TXTPAGE2 = 0xC055;  //  Select page2 display (or aux video memory)
        public const ushort LORES = 0xC056;  // Select low resolution graphics
        public const ushort HIRES = 0xC057;  //  Select high resolution graphics

        // Annunciator pairs
        public const ushort CLRAN0 = 0xC058;
        public const ushort SETAN0 = 0xC059;
        public const ushort CLRAN1 = 0xC05A;
        public const ushort SETAN1 = 0xC05B;
        public const ushort CLRAN2 = 0xC05C;
        public const ushort SETAN2 = 0xC05D;
        public const ushort CLRAN3 = 0xC05E;
        public const ushort SETAN3 = 0xC05F;

        // Other hardware
        public const ushort TAPEIN = 0xC060;
        public const ushort BUTTON0 = 0xC061;
        public const ushort OPENAPPLE = 0xC061;
        public const ushort BUTTON1 = 0xC062;
        public const ushort SOLIDAPPLE = 0xC062;
        public const ushort BUTTON2 = 0xC063;
        public const ushort SHIFT = 0xC063;
        public const ushort PADDLE0 = 0xC064;
        public const ushort PADDLE1 = 0xC065;
        public const ushort PADDLE2 = 0xC066;
        public const ushort PADDLE3 = 0xC067;

        public const ushort PTRIG = 0xC070;

        public const ushort RDIOUDIS = 0xC07E;
        public const ushort RDDHIRES = 0xC07F;

        public const ushort IOUDISON = 0xC07E;
        public const ushort IOUDISOFF = 0xC07F;
    }
#pragma warning restore CA1707
}
