using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace InnoWerks.Emulators.AppleIIe
{
    public sealed class HostLayout
    {
        // the main display
        public Rectangle AppleDisplay { get; set; }

        // the registers
        public Rectangle Registers { get; set; }

        // textmode, mixedmode, etc.
        public Rectangle Block1 { get; set; }

        // aux, main mem, etc.
        public Rectangle Block2 { get; set; }

        // slot selection, language card
        public Rectangle Block3 { get; set; }


        public Rectangle CpuTrace { get; set; }

        public static HostLayout ComputeLayout(int windowWidth, int windowHeight)
        {
            const int CpuTraceWidth = 240;
            const int Padding = 8;

            int availableWidth = windowWidth - CpuTraceWidth - Padding * 3;
            int availableHeight = windowHeight - Padding * 2;

            int scale = Math.Max(
                1,
                Math.Min(availableWidth / Display.AppleDisplayWidth, availableHeight / Display.AppleDisplayHeight)
            );

            int appleRenderWidth = Display.AppleDisplayWidth * scale;
            int appleRenderHeight = Display.AppleDisplayHeight * scale;

            int blockWidth = Padding + (appleRenderWidth / 4);

            return new HostLayout
            {
                AppleDisplay = new Rectangle(
                    Padding,
                    Padding,
                    appleRenderWidth,
                    appleRenderHeight
                ),

                CpuTrace = new Rectangle(
                    appleRenderWidth + Padding * 2,
                    Padding,
                    windowWidth - (appleRenderWidth + (3 * Padding)),
                    windowHeight - (Padding * 3)
                ),

                Registers = new Rectangle(
                    Padding,
                    appleRenderHeight + (2 * Padding),
                    blockWidth,
                    windowHeight - ((Padding * 3) + appleRenderHeight)
                ),

                Block1 = new Rectangle(
                    blockWidth * 1,
                    appleRenderHeight + (2 * Padding),
                    blockWidth,
                    windowHeight - ((Padding * 3) + appleRenderHeight)
                ),

                Block2 = new Rectangle(
                    blockWidth * 2,
                    appleRenderHeight + (2 * Padding),
                    blockWidth,
                    windowHeight - ((Padding * 3) + appleRenderHeight)
                ),

                Block3 = new Rectangle(
                    blockWidth * 3,
                    appleRenderHeight + (2 * Padding),
                    blockWidth,
                    windowHeight - ((Padding * 3) + appleRenderHeight)
                ),
            };
        }
    }
}
