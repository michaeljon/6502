using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using InnoWerks.Computers.Apple;
using InnoWerks.Processors;
using InnoWerks.Simulators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace InnoWerks.Emulators.AppleIIe
{
    public class Display : IDisposable
    {
        public const int AppleCellWidth = 7;

        public const int AppleCellHeight = 8;

        public const int AppleBlockWidth = 7;

        public const int AppleBlockHeight = 4;

        public const int InternalWidth = 280;

        public const int InternalHeight = 192;

        private const int GlyphWidth = 8;
        private const int GlyphHeight = 8;
        private const int GlyphsPerRow = 16;
        private const int GlyphCount = 512;

        private const int TexWidth = GlyphsPerRow * GlyphWidth;   // 128
        private const int TexHeight = (GlyphCount / GlyphsPerRow) * GlyphHeight; // 256

        //
        // MonoGame stuff
        //
        private GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;
        private Texture2D whitePixel;
        private Texture2D[] loresPixels;
        private Texture2D charTexture;
        private RenderTarget2D appleTarget;

        private readonly MachineState machineState;

        private readonly TextMemoryReader textMemoryReader;
        private readonly LoresMemoryReader loresMemoryReader;

        private bool disposed;

        public Display(GraphicsDevice graphicsDevice, MemoryBlocks memoryBlocks, MachineState machineState)
        {
            ArgumentNullException.ThrowIfNull(graphicsDevice);
            ArgumentNullException.ThrowIfNull(memoryBlocks);
            ArgumentNullException.ThrowIfNull(machineState);

            this.graphicsDevice = graphicsDevice;
            this.machineState = machineState;

            textMemoryReader = new TextMemoryReader(memoryBlocks, machineState);
            loresMemoryReader = new LoresMemoryReader(memoryBlocks, machineState);
        }

        public void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphicsDevice);

            whitePixel = new Texture2D(graphicsDevice, 1, 1);
            whitePixel.SetData([Color.White]);

            // this is a hack for now
            loresPixels = new Texture2D[LoresCell.PaletteSize];
            for (var p = 0; p < LoresCell.PaletteSize; p++)
            {
                loresPixels[p] = new Texture2D(graphicsDevice, 1, 1);
                loresPixels[p].SetData([LoresCell.GetPaletteColor(p)]);
            }

            appleTarget = new RenderTarget2D(
                graphicsDevice,
                560,
                InternalHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.PreserveContents
            );

            LoadCharacterRom();
        }

        private void LoadCharacterRom()
        {
            var charRom = File.ReadAllBytes("roms/342-0133.bin");
            Debug.Assert(charRom.Length == 4096);

            charTexture = new Texture2D(graphicsDevice, TexWidth, TexHeight);

            var pixels = new Color[TexWidth * TexHeight];

            for (int ch = 0; ch < GlyphCount; ch++)
            {
                int gx = (ch % GlyphsPerRow) * GlyphWidth;
                int gy = (ch / GlyphsPerRow) * GlyphHeight;

                for (int row = 0; row < 8; row++)
                {
                    byte bits = charRom[ch * 8 + row];

                    for (int col = 0; col < 7; col++)
                    {
                        bool on = (bits & (1 << col)) != 0;
                        pixels[(gy + row) * TexWidth + (gx + col)] =
                            on ? Color.White : Color.Transparent;
                    }
                }
            }

            charTexture.SetData(pixels);
        }

        public void Draw(bool flashOn)
        {
            ArgumentNullException.ThrowIfNull(graphicsDevice);

            //
            // draw the content to the off-screen buffer
            //
            graphicsDevice.SetRenderTarget(appleTarget);
            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                sortMode: SpriteSortMode.Deferred
            );

            if (machineState.State[SoftSwitch.TextMode])
            {
                DrawTextMode(0, 24, flashOn);
            }
            else if (machineState.State[SoftSwitch.TextMode] == false && machineState.State[SoftSwitch.MixedMode] == false)
            {
                DrawLoresMode(0, 24);
            }
            else if (machineState.State[SoftSwitch.MixedMode] == true)
            {
                DrawLoresMode(0, 20);
                DrawTextMode(20, 4, flashOn);
            }

            spriteBatch.End();

            //
            // blt the off-screen buffer onto the display
            //
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                sortMode: SpriteSortMode.Deferred
            );

            int vw = graphicsDevice.Viewport.Width;
            int vh = graphicsDevice.Viewport.Height;

            float scaleX = vw / (float)InternalWidth;
            float scaleY = vh / (float)InternalHeight;
            float scale = MathF.Floor(MathF.Min(scaleX, scaleY));

            if (scale < 1) scale = 1;

            int dstW = (int)(InternalWidth * scale);
            int dstH = (int)(InternalHeight * scale);

            int offsetX = (vw - dstW) / 2;
            int offsetY = (vh - dstH) / 2;

            spriteBatch.Draw(
                appleTarget,
                new Rectangle(offsetX, offsetY, dstW, dstH),
                Color.White
            );

            spriteBatch.End();
        }

        private void DrawLoresMode(int start, int count)
        {
            var loresBuffer = new LoresBuffer();
            loresMemoryReader.ReadLoresPage(loresBuffer);

            for (var row = start; row < start + count; row++)
            {
                for (var col = 0; col < 40; col++)
                {
                    var cell = loresBuffer.Get(row, col);

                    DrawBlocks(
                        cell,
                        col,
                        row
                    );
                }
            }
        }

        private void DrawTextMode(int start, int count, bool flashOn)
        {
            var textBuffer = new TextBuffer();
            textMemoryReader.ReadTextPage(textBuffer);

            var cols = machineState.State[SoftSwitch.EightyColumnMode] ? 80 : 40;

            for (var row = start; row < start + count; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    var cell = textBuffer.Get(row, col);

                    DrawChar(cell.Ascii, col, row);
                }
            }
        }

        private void DrawChar(byte ascii, int col, int row)
        {
            float scaleX = machineState.State[SoftSwitch.EightyColumnMode] ? 4f / 7f : 1f;
            // float scaleY = 1f;

            var inverse = (ascii & 0x80) != 0;
            var flash = (ascii & 0x40) != 0;

            int glyph = machineState.State[SoftSwitch.EightyColumnMode] ?
                ascii & 0x7F :
                (ascii & 0x3F) | ((ascii & 0x40) != 0 ? 0x40 : 0x00);

            var fg = inverse ? Color.Black : Color.LightGreen;
            var bg = inverse ? Color.LightGreen : Color.Black;

            var srcX = (glyph % 16) * 8;
            var srcY = (glyph / 16) * 8;

            var src = new Rectangle(srcX, srcY, 7, 8);
            var dst = new Rectangle(col * AppleCellWidth, row * AppleCellHeight, (int)(AppleCellWidth * scaleX), AppleCellHeight);

            // Background
            if (bg != Color.Transparent)
            {
                spriteBatch.Draw(whitePixel, dst, bg);
            }

            var pos = new Vector2(col * AppleCellWidth, row * AppleCellHeight);

            spriteBatch.Draw(
                charTexture,
                dst,
                src,
                fg);

            // spriteBatch.Draw(
            //     charTexture,
            //     pos,
            //     src,
            //     fg,
            //     0f,
            //     Vector2.Zero,
            //     new Vector2(scaleX, scaleY),
            //     SpriteEffects.None,
            //     0f);
        }

        private void DrawBlocks(LoresCell cell, int col, int row)
        {
            var topPixel = loresPixels[cell.TopIndex];
            var topRect = new Rectangle(
                col * AppleBlockWidth,
                row * 2 * AppleBlockHeight,
                AppleBlockWidth,
                AppleBlockHeight);

            spriteBatch.Draw(topPixel, topRect, cell.Top);

            var bottomPixel = loresPixels[cell.BottomIndex];
            var bottomRect = new Rectangle(
                col * AppleBlockWidth,
                ((row * 2) + 1) * AppleBlockHeight,
                AppleBlockWidth,
                AppleBlockHeight);

            spriteBatch.Draw(bottomPixel, bottomRect, cell.Bottom);
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
                spriteBatch?.Dispose();
                whitePixel?.Dispose();
                charTexture?.Dispose();
                appleTarget?.Dispose();
            }

            disposed = true;
        }
    }
}
