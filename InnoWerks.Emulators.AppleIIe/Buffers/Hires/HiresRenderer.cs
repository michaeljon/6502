using System;
using System.Diagnostics;
using System.IO;
using InnoWerks.Computers.Apple;
using InnoWerks.Simulators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace InnoWerks.Emulators.AppleIIe
{
    public class HiresRenderer : IDisposable
    {
        private static readonly Color HiresBlack = new(0, 0, 0);
        private static readonly Color HiresPurple = new(128, 0, 255);
        private static readonly Color HiresGreen = new(0, 192, 0);

        // private static readonly Color HiresWhite = new(255, 255, 255);
        // private static readonly Color HiresOrange = new(255, 128, 0);
        // private static readonly Color HiresBlue = new(0, 0, 255);

        //
        // MonoGame stuff
        //
        private readonly Texture2D whitePixel;

        private readonly MosTechnologiesCpu cpu;
        private readonly IBus bus;
        private readonly MachineState machineState;

        private bool disposed;

        private readonly HiresMemoryReader hiresMemoryReader;
        private readonly DhiresMemoryReader dhiresMemoryReader;

        public HiresRenderer(
            GraphicsDevice graphicsDevice,
            MosTechnologiesCpu cpu,
            IBus bus,
            Memory128k memoryBlocks,
            MachineState machineState,

            ContentManager contentManager
            )
        {
            ArgumentNullException.ThrowIfNull(graphicsDevice);
            ArgumentNullException.ThrowIfNull(cpu);
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(memoryBlocks);
            ArgumentNullException.ThrowIfNull(machineState);

            ArgumentNullException.ThrowIfNull(contentManager);

            this.machineState = machineState;
            this.cpu = cpu;
            this.bus = bus;

            whitePixel = new Texture2D(graphicsDevice, 1, 1);
            whitePixel.SetData([Color.White]);

            hiresMemoryReader = new(memoryBlocks, machineState);
            dhiresMemoryReader = new(memoryBlocks, machineState);
        }

        public void DrawHiresMode(SpriteBatch spriteBatch, int start, int count)
        {
            ArgumentNullException.ThrowIfNull(spriteBatch);

            var hiresBuffer = new HiresBuffer();
            hiresMemoryReader.ReadHiresPage(hiresBuffer);

            int pixelWidth = DisplayCharacteristics.AppleBlockWidth / 2;
            int pixelHeight = DisplayCharacteristics.AppleBlockHeight;

            for (int y = start; y < start + count; y++)
            {
                for (int x = 0; x < 280; x++)
                {
                    if (!hiresBuffer.GetPixel(y, x))
                        continue; // Off pixel → nothing to draw

                    byte sourceByte = hiresBuffer.GetSourceByte(y, x);

                    // Phase calculation: bit7 of byte + horizontal position
                    bool phaseBit = (sourceByte & 0x80) != 0;
                    bool phase = ((x & 1) == 1) ^ phaseBit;

                    Color color = phase ? HiresGreen : HiresPurple;

                    var rect = new Rectangle(
                        x * pixelWidth,
                        y * pixelHeight,
                        pixelWidth,
                        pixelHeight);

                    spriteBatch.Draw(whitePixel, rect, color);
                }
            }
        }

        public void DrawDhiresMode(SpriteBatch spriteBatch, int start, int count)
        {
            ArgumentNullException.ThrowIfNull(spriteBatch);

            var buffer = new DhiresBuffer();
            dhiresMemoryReader.ReadDhiresPage(buffer);

            int width = DisplayCharacteristics.HiresAppleWidth;
            int pixelWidth = DisplayCharacteristics.AppleBlockWidth / 2;
            int pixelHeight = DisplayCharacteristics.AppleBlockHeight;

            for (int y = start; y < start + count; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var p = buffer.GetPixel(y, x);

#pragma warning disable CA2201 // Do not raise reserved exception types
                    if (p.AuxBit && p.MainBit == false && p.IsOn == false)
                        throw new Exception("AUX-only pixel not considered ON");

                    if (!p.AuxBit && p.MainBit && p.IsOn == false)
                        throw new Exception("MAIN-only pixel not considered ON");
#pragma warning restore CA2201 // Do not raise reserved exception types

                    Color color = HiresBlack;

                    if (p.IsOn)
                    {
                        bool phase = ((x % 2) == 0) ^ ((p.SourceByte & 0x80) != 0);

                        if (p.AuxBit && !p.MainBit)
                            color = phase ? HiresGreen : HiresPurple;
                        else if (!p.AuxBit && p.MainBit)
                            color = phase ? HiresGreen : HiresPurple;
                        else
                            color = phase ? HiresGreen : HiresPurple; // both bits set
                    }

                    var rect = new Rectangle(
                        x * pixelWidth,
                        y * pixelHeight,
                        pixelWidth,
                        pixelHeight);

                    spriteBatch.Draw(whitePixel, rect, color);
                }
            }
        }

        private void DrawHiresPixel(SpriteBatch spriteBatch, int x, int y, byte sourceByte)
        {
            // Each logical pixel is scaled to your AppleBlockWidth / AppleBlockHeight
            const int pixelWidth = DisplayCharacteristics.AppleBlockWidth / 2;  // 280 pixels per scanline vs 140 for 80-LORES
            const int pixelHeight = DisplayCharacteristics.AppleBlockHeight;

            // Determine phase for artifact color
            bool phaseBit = (sourceByte & 0x80) != 0; // bit 7 controls horizontal phase
            bool phase = ((x & 1) == 1) ^ phaseBit;

            // Decide color based on pixel value and phase
            Color color = phase ? HiresGreen : HiresPurple;

            var rect = new Rectangle(
                x * pixelWidth,
                y * pixelHeight,
                pixelWidth,
                pixelHeight);

            // Draw a single pixel rectangle
            spriteBatch.Draw(whitePixel, rect, color);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed == true)
            {
                return;
            }

            if (disposing)
            {
                whitePixel?.Dispose();
            }

            disposed = true;
        }
    }
}
