using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using InnoWerks.Computers.Apple;
using InnoWerks.Processors;
using InnoWerks.Simulators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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

        public const int AppleDisplayWidth = 280;

        public const int AppleDisplayHeight = 192;

        private const int LoresAppleWidth = 280;

        private const int HiresAppleWidth = 560;

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
        private SpriteFont debugFont;
        private Texture2D whitePixel;
        private Texture2D[] loresPixels;
        private Texture2D charTexture;

        private readonly MosTechnologiesCpu cpu;
        private readonly IBus bus;
        private readonly MachineState machineState;

        private readonly TextMemoryReader textMemoryReader;
        private readonly LoresMemoryReader loresMemoryReader;

        private bool disposed;

        private readonly List<SoftSwitch> debugSwitchesBlock1 =
        [
            SoftSwitch.TextMode,
            SoftSwitch.MixedMode,
            SoftSwitch.Page2,
            SoftSwitch.HiRes,
            SoftSwitch.DoubleHiRes,
            SoftSwitch.IOUDisabled,
        ];

        private readonly List<SoftSwitch> debugSwitchesBlock2 =
        [
            SoftSwitch.Store80,
            SoftSwitch.AuxRead,
            SoftSwitch.AuxWrite,
            SoftSwitch.ZpAux,
            SoftSwitch.EightyColumnMode,
            SoftSwitch.AltCharSet,
        ];

        private readonly List<SoftSwitch> debugSwitchesBlock3 =
        [
            SoftSwitch.IntCxRomEnabled,
            SoftSwitch.Slot3RomEnabled,
            SoftSwitch.IntC8RomEnabled,
            SoftSwitch.LcBank2,
            SoftSwitch.LcReadEnabled,
            SoftSwitch.LcWriteEnabled,
        ];

        private readonly Dictionary<SoftSwitch, string> debugSwitchDisplay = new()
        {
            { SoftSwitch.TextMode, "TEXT" },
            { SoftSwitch.MixedMode, "MIXED" },
            { SoftSwitch.Page2, "PAGE2" },
            { SoftSwitch.HiRes, "HIRES" },
            { SoftSwitch.DoubleHiRes, "DHIRES" },
            { SoftSwitch.IOUDisabled, "IOUDIS" },

            { SoftSwitch.Store80, "STOR80" },
            { SoftSwitch.AuxRead, "AUXRD" },
            { SoftSwitch.AuxWrite, "AUXWR" },
            { SoftSwitch.ZpAux, "ZPAUX" },
            { SoftSwitch.EightyColumnMode, "80COL" },
            { SoftSwitch.AltCharSet, "ALTCHR" },

            { SoftSwitch.IntCxRomEnabled, "INTCX" },
            { SoftSwitch.Slot3RomEnabled, "SLOTC3" },
            { SoftSwitch.IntC8RomEnabled, "INTC8" },
            { SoftSwitch.LcBank2, "BANK2" },
            { SoftSwitch.LcReadEnabled, "LCRD" },
            { SoftSwitch.LcWriteEnabled, "LCWRT" },
        };

        public Display(
            GraphicsDevice graphicsDevice,
            MosTechnologiesCpu cpu,
            IBus bus,
            MemoryBlocks memoryBlocks,
            MachineState machineState)
        {
            ArgumentNullException.ThrowIfNull(graphicsDevice);
            ArgumentNullException.ThrowIfNull(cpu);
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(memoryBlocks);
            ArgumentNullException.ThrowIfNull(machineState);

            this.graphicsDevice = graphicsDevice;
            this.machineState = machineState;
            this.cpu = cpu;
            this.bus = bus;

            textMemoryReader = new TextMemoryReader(memoryBlocks, machineState);
            loresMemoryReader = new LoresMemoryReader(memoryBlocks, machineState);
        }

        public void LoadContent(ContentManager contentManager)
        {
            ArgumentNullException.ThrowIfNull(contentManager);

            spriteBatch = new SpriteBatch(graphicsDevice);
            debugFont = contentManager.Load<SpriteFont>("DebugFont");

            whitePixel = new Texture2D(graphicsDevice, 1, 1);
            whitePixel.SetData([Color.White]);

            // this is a hack for now
            loresPixels = new Texture2D[LoresCell.PaletteSize];
            for (var p = 0; p < LoresCell.PaletteSize; p++)
            {
                loresPixels[p] = new Texture2D(graphicsDevice, 1, 1);
                loresPixels[p].SetData([LoresCell.GetPaletteColor(p)]);
            }

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

        public void Draw(
            HostLayout hostLayout,
            CpuTraceBuffer cpuTraceBuffer,
            HashSet<ushort> breakpoints,
            bool flashOn)
        {
            ArgumentNullException.ThrowIfNull(hostLayout);
            ArgumentNullException.ThrowIfNull(cpuTraceBuffer);
            ArgumentNullException.ThrowIfNull(breakpoints);

            using var appleTarget = new RenderTarget2D(
                graphicsDevice,
                machineState.State[SoftSwitch.EightyColumnMode] ? HiresAppleWidth : LoresAppleWidth,
                AppleDisplayHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.PreserveContents
            );

            DrawAppleRegion(appleTarget, flashOn);

            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.LinearClamp);

            DrawPanel(hostLayout.AppleDisplay);
            spriteBatch.Draw(appleTarget, hostLayout.AppleDisplay, Color.White);

            DrawRegisters(hostLayout.Registers);
            DrawBlock(hostLayout.Block1, debugSwitchesBlock1);
            DrawBlock(hostLayout.Block2, debugSwitchesBlock2);
            DrawBlock(hostLayout.Block3, debugSwitchesBlock3);

            DrawCpuTrace(hostLayout.CpuTrace, cpuTraceBuffer, breakpoints);

            spriteBatch.End();
        }

        public CpuTraceEntry? HandleTraceClick(HostLayout hostLayout, CpuTraceBuffer cpuTraceBuffer, Point mousePos)
        {
            ArgumentNullException.ThrowIfNull(hostLayout);
            ArgumentNullException.ThrowIfNull(cpuTraceBuffer);

            if (hostLayout.CpuTrace.Contains(mousePos) == false)
            {
                return null;
            }

            int lineHeight = debugFont.LineSpacing;
            int bottom = hostLayout.CpuTrace.Bottom - 8;

            int indexFromBottom = (bottom - mousePos.Y) / lineHeight;
            if (indexFromBottom < 0)
            {
                return null;
            }

            var entries = cpuTraceBuffer.Entries.ToList();
            entries.Reverse(); // newest first

            if (indexFromBottom >= entries.Count)
            {
                return null;
            }

            return entries[indexFromBottom];
        }

        private void DrawAppleRegion(RenderTarget2D appleTarget, bool flashOn)
        {
            //
            // draw the content to the off-screen buffer
            //
            graphicsDevice.SetRenderTarget(appleTarget);
            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                samplerState: SamplerState.PointClamp);

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
        }

        private void DrawRegisters(Rectangle rectangle)
        {
            DrawPanel(rectangle);

            int x = rectangle.X + 8;
            int y = rectangle.Y + 8;

            DrawKeyValue($"PC:", $"{cpu.Registers.ProgramCounter:X4}", x, ref y);
            DrawKeyValue($"A:", $"{cpu.Registers.A:X2}", x, ref y);
            DrawKeyValue($"X:", $"{cpu.Registers.X:X2}", x, ref y);
            DrawKeyValue($"Y:", $"{cpu.Registers.Y:X2}", x, ref y);
            DrawKeyValue($"SP:", $"{cpu.Registers.StackPointer:X2}", x, ref y);
            DrawKeyValue($"PS:", $"{cpu.Registers.InternalGetFlagsDisplay}", x, ref y);
        }

        private void DrawBlock(Rectangle rectangle, List<SoftSwitch> softSwitches)
        {
            DrawPanel(rectangle);

            int x = rectangle.X + 8;
            int y = rectangle.Y + 8;

            foreach (var sw in softSwitches)
            {
                DrawKeyValue($"{debugSwitchDisplay[sw]}:", $"{(machineState.State[sw] ? 1 : 0)}", x, ref y);
            }
        }

        private void DrawCpuTrace(Rectangle rectangle, CpuTraceBuffer cpuTraceBuffer, HashSet<ushort> breakpoints)
        {
            DrawPanel(rectangle);

            int x = rectangle.X + 12;
            int y = rectangle.Bottom - debugFont.LineSpacing - 12;

            for (var i = cpuTraceBuffer.Count - 1; i > 0; i--)
            {
                var entry = cpuTraceBuffer[i];

                if (y < rectangle.Y + 8)
                {
                    break;
                }

                DrawTraceLine(entry, breakpoints, x, y);

                y -= debugFont.LineSpacing;
            }
        }

        private void DrawPanel(Rectangle rectangle)
        {
            spriteBatch.Draw(whitePixel, rectangle, Color.White);
            spriteBatch.Draw(whitePixel, new Rectangle(rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2), new Color(20, 20, 20));

            // spriteBatch.Draw(whitePixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, 1), Color.Gray);
        }

        private void DrawKeyValue(
            string key,
            string value,
            int x,
            ref int y)
        {
            spriteBatch.DrawString(debugFont, key, new Vector2(x, y), Color.LightGreen);
            spriteBatch.DrawString(debugFont, value, new Vector2(x + 64, y), Color.White);
            y += debugFont.LineSpacing;
        }

        private void DrawLine(
            Rectangle panel,
            string text,
            int x,
            ref int y,
            Color? color = null)
        {
            if (y + debugFont.LineSpacing > panel.Bottom)
                return;

            spriteBatch.DrawString(
                debugFont,
                text,
                new Vector2(x, y),
                color ?? Color.LightGreen);

            y += debugFont.LineSpacing;
        }

        private void DrawTraceLine(
            CpuTraceEntry cpuTraceEntry,
            HashSet<ushort> breakpoints,
            int x,
            int y)
        {
            var opcode = cpuTraceEntry.OpCode;
            var pc = cpuTraceEntry.ProgramCounter;
            var decoded = cpuTraceEntry.OpCode.DecodeOperand(cpu, bus);

            if (breakpoints.Contains(cpuTraceEntry.ProgramCounter))
            {
                spriteBatch.DrawString(
                    debugFont,
                    ">",
                    new Vector2(x - 8, y),
                    Color.Red);
            }

            spriteBatch.DrawString(debugFont, $"{pc:X4}", new Vector2(x, y), Color.LightGreen);

            spriteBatch.DrawString(
                debugFont,
                $"{opcode.OpCodeValue:X2}",
                new Vector2(x + 52, y),
                Color.Gray);

            if (decoded.Length > 1)
                spriteBatch.DrawString(debugFont, $"{decoded.Operand1:X2}", new Vector2(x + 80, y), Color.Gray);
            if (decoded.Length > 2)
                spriteBatch.DrawString(debugFont, $"{decoded.Operand2:X2}", new Vector2(x + 108, y), Color.Gray);

            spriteBatch.DrawString(
                debugFont,
                cpuTraceEntry.Mnemonic,
                new Vector2(x + 150, y),
                Color.White);

            spriteBatch.DrawString(
                debugFont,
                decoded.Display,
                new Vector2(x + 200, y),
                Color.White);

            // todo: lookup symbols here??!!
        }

        private void DrawLoresMode(int start, int count)
        {
            var loresBuffer = new LoresBuffer();
            loresMemoryReader.ReadLoresPage(loresBuffer);

            var cols = machineState.State[SoftSwitch.EightyColumnMode] ? 80 : 40;

            for (var row = start; row < start + count; row++)
            {
                for (var col = 0; col < cols; col++)
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
            var inverse = (ascii & 0x80) != 0;
            var flash = (ascii & 0x40) != 0;

            int glyph = machineState.State[SoftSwitch.EightyColumnMode] ?
                ascii & 0x7F :
                (ascii & 0x3F) | ((ascii & 0x40) != 0 ? 0x40 : 0x00);

            var fg = inverse ? Color.Black : Color.LightGreen;
            var bg = inverse ? Color.LightGreen : Color.Black;

            var srcX = (glyph % 16) * 8;
            var srcY = (glyph / 16) * 8;

            var src = new Rectangle(srcX, srcY, AppleCellWidth, AppleCellHeight);
            var dst = new Rectangle(col * AppleCellWidth, row * AppleCellHeight, AppleCellWidth, AppleCellHeight);

            // Background
            if (bg != Color.Transparent)
            {
                spriteBatch.Draw(whitePixel, dst, bg);
            }

            spriteBatch.Draw(
                charTexture,
                dst,
                src,
                fg);
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
            }

            disposed = true;
        }
    }
}
