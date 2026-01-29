using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace InnoWerks.Emulators.AppleIIe
{
    public sealed class HostLayout
    {
        public Rectangle AppleDisplay { get; set; }

        public Rectangle Registers { get; set; }

        public Rectangle CpuTrace { get; set; }

        public static HostLayout ComputeLayout(int windowWidth, int windowHeight)
        {
            const int RegistersWidth = 360;
            const int Padding = 8;

            int availableWidth = windowWidth - RegistersWidth - Padding * 3;
            int availableHeight = windowHeight - Padding * 2;

            int scale = Math.Max(
                1,
                Math.Min(availableWidth / Display.AppleDisplayWidth, availableHeight / Display.AppleDisplayHeight)
            );

            int appleRenderWidth = Display.AppleDisplayWidth * scale;
            int appleRenderHeight = Display.AppleDisplayHeight * scale;

            return new HostLayout
            {
                AppleDisplay = new Rectangle(
                    Padding,
                    Padding,
                    appleRenderWidth,
                    appleRenderHeight
                ),

                Registers = new Rectangle(
                    Padding,
                    appleRenderHeight + (2 * Padding),
                    appleRenderWidth,
                    windowHeight - ((Padding * 3) + appleRenderHeight)
                ),

                CpuTrace = new Rectangle(
                    appleRenderWidth + Padding * 2,
                    Padding,
                    windowWidth - (appleRenderWidth + (3 * Padding)),
                    windowHeight - (Padding * 3)
                )
            };
        }
    }
}
