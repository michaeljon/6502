using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace InnoWerks.Emulators.AppleIIe
{
    static class KeyMapper
    {
        private static readonly Dictionary<Keys, char> shiftedNumberSymbols = new()
        {
            { Keys.D1, '!' },
            { Keys.D2, '"' },
            { Keys.D3, '#' },
            { Keys.D4, '$' },
            { Keys.D5, '%' },
            { Keys.D6, '^' },
            { Keys.D7, '&' },
            { Keys.D8, '*' },
            { Keys.D9, '(' },
            { Keys.D0, ')' },
        };

        public static bool TryMap(Keys key, KeyboardState state, out byte ascii)
        {
            bool shift = state.IsKeyDown(Keys.LeftShift) ||
                         state.IsKeyDown(Keys.RightShift);

            ascii = 0;

            // Letters
            if (key >= Keys.A && key <= Keys.Z)
            {
                char c = (char)('A' + (key - Keys.A));
                ascii = (byte)(shift ? c : char.ToLowerInvariant(c));
                return true;
            }

            // Numbers and shifted symbols
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                if (shift && shiftedNumberSymbols.TryGetValue(key, out char sym))
                {
                    ascii = (byte)sym;
                }
                else
                {
                    ascii = (byte)('0' + (key - Keys.D0));
                }

                return true;
            }

            // Basic controls
            switch (key)
            {
                case Keys.Enter: ascii = 0x0D; return true;
                case Keys.Back: ascii = 0x08; return true;
                case Keys.Tab: ascii = 0x09; return true;
                case Keys.Escape: ascii = 0x1B; return true;
                case Keys.Space: ascii = 0x20; return true;

                case Keys.Left: ascii = 0x08; return true;
                case Keys.Right: ascii = 0x15; return true;
                case Keys.Up: ascii = 0x0B; return true;
                case Keys.Down: ascii = 0x0A; return true;

                case Keys.OemComma: ascii = (byte)(shift ? '<' : ','); return true;
                case Keys.OemPeriod: ascii = (byte)(shift ? '>' : '.'); return true;
                case Keys.OemMinus: ascii = (byte)(shift ? '_' : '-'); return true;
                case Keys.OemPlus: ascii = (byte)(shift ? '+' : '='); return true;
                case Keys.OemSemicolon: ascii = (byte)(shift ? ':' : ';'); return true;
                case Keys.OemQuotes: ascii = (byte)(shift ? '"' : '\''); return true;
                case Keys.OemQuestion: ascii = (byte)(shift ? '?' : '/'); return true;
                case Keys.OemPipe: ascii = (byte)(shift ? '|' : '\\'); return true;
                case Keys.OemOpenBrackets: ascii = (byte)(shift ? '{' : '['); return true;
                case Keys.OemCloseBrackets: ascii = (byte)(shift ? '}' : ']'); return true;
                case Keys.OemTilde: ascii = (byte)(shift ? '~' : '`'); return true;
            }

            return false;
        }
    }
}
