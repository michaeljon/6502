namespace InnoWerks.Emulators.AppleIIe
{
    public readonly struct TextCell : System.IEquatable<TextCell>
    {
        public readonly byte Ascii { get; init; }

        public TextCell(byte ascii)
        {
            Ascii = ascii;
        }

        public char ToChar()
        {
            // Apple II text is basically ASCII 0x20–0x7E for now
            if (Ascii < 0x20 || Ascii > 0x7E)
                return ' ';

            return (char)Ascii;
        }

        public override bool Equals(object obj)
        {
            return ((TextCell)obj).Ascii == Ascii;
        }

        public override int GetHashCode()
        {
            return Ascii.GetHashCode();
        }

        public bool Equals(TextCell other)
        {
            return other.Ascii == Ascii;
        }

        public static bool operator ==(TextCell left, TextCell right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextCell left, TextCell right)
        {
            return !(left == right);
        }
    }
}
