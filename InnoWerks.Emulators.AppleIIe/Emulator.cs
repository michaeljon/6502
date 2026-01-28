using System;
using System.IO;
using System.Net.NetworkInformation;
using InnoWerks.Computers.Apple;
using InnoWerks.Processors;
using InnoWerks.Simulators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA2213 // Disposable fields should be disposed
#pragma warning disable CS0169 // Make field read-only
#pragma warning disable IDE0051 // Make field read-only
#pragma warning disable RCS1169 // Make field read-only
#pragma warning disable CA1823 // Avoid unused private fields
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable RCS1213 // Remove unused member declaration

namespace InnoWerks.Emulators.AppleIIe
{
    public class Emulator : Game
    {
        // Fixed cell dimensions (logical Apple II cells)
        private const int AppleCellWidth = 7;
        private const int AppleCellHeight = 8;

        //
        // The Apple IIe itself
        //
        private AppleBus appleBus;
        private MemoryBlocks memoryBlocks;
        private MachineState machineState;
        private IOU iou;
        private MMU mmu;
        private Cpu65C02 cpu;

        //
        // MonoGame stuff
        //
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D whitePixel;
        private Texture2D[] loresPixels;

        private KeyboardState previousKeyboardState;

        TextMemoryReader textReader;
        LoresMemoryReader loresMemoryReader;

        public Emulator()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 800,   // initial width
                PreferredBackBufferHeight = 600,  // initial height
                IsFullScreen = false
            };
            graphics.ApplyChanges();

            // Make window resizable
            Window.AllowUserResizing = true;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            var mainRom = File.ReadAllBytes("roms/apple2e-16k.rom");
            var diskIIRom = File.ReadAllBytes("roms/DiskII.rom");
            var dos33 = File.ReadAllBytes("disks/dos33.dsk");
            var audit = File.ReadAllBytes("tests/audit.o");

            var config = new AppleConfiguration(AppleModel.AppleIIe)
            {
                CpuClass = CpuClass.WDC65C02,
                HasAuxMemory = true,
                Has80Column = true,
                HasLowercase = true,
                RamSize = 128
            };

            machineState = new MachineState();
            memoryBlocks = new MemoryBlocks(machineState);

            appleBus = new AppleBus(config, memoryBlocks, machineState);
            iou = new IOU(memoryBlocks, machineState, appleBus);
            mmu = new MMU(machineState, appleBus);

            var disk = new DiskIISlotDevice(appleBus, machineState, diskIIRom);
            DiskIINibble.LoadDisk(disk.GetDrive(1), dos33);

            cpu = new Cpu65C02(
                appleBus,
                (cpu, programCounter) => { },
                (cpu) => { });

            appleBus.LoadProgramToRom(mainRom);
            appleBus.LoadProgramToRam(audit, 0x6000);

            cpu.Reset();

            textReader = new TextMemoryReader(memoryBlocks, machineState);
            loresMemoryReader = new LoresMemoryReader(memoryBlocks, machineState);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("CourierFont");

            whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            whitePixel.SetData([Color.White]);

            // this is a hack for now
            loresPixels = new Texture2D[LoresCell.PaletteSize];
            for (var p = 0; p < LoresCell.PaletteSize; p++)
            {
                loresPixels[p] = new Texture2D(GraphicsDevice, 1, 1);
                loresPixels[p].SetData([LoresCell.GetPaletteColor(p)]);
            }
        }

        private double flashTimer;
        private bool flashOn = true;

        protected override void Update(GameTime gameTime)
        {
            ArgumentNullException.ThrowIfNull(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            HandleKeyboardInput();

            var targetCycles = appleBus.CycleCount + VideoTiming.FrameCycles;
            while (appleBus.CycleCount < targetCycles)
            {
                cpu.Step();
            }

            // Toggle flashing every 500ms
            flashTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (flashTimer >= 500)
            {
                flashTimer = 0;
                flashOn = !flashOn;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (machineState.State[SoftSwitch.TextMode])
            {
                DrawTextMode(gameTime);
            }
            else if (machineState.State[SoftSwitch.TextMode] == false && machineState.State[SoftSwitch.MixedMode] == false)
            {
                DrawLoresMode(gameTime);
            }
            else if (machineState.State[SoftSwitch.MixedMode] == true)
            {
                DrawMixedMode(gameTime);
            }

            base.Draw(gameTime);
        }

        void HandleKeyboardInput()
        {
            var state = Keyboard.GetState();

            foreach (var key in state.GetPressedKeys())
            {
                if (previousKeyboardState.IsKeyUp(key))
                {
                    if (KeyMapper.TryMap(key, state, out byte ascii))
                    {
                        iou.InjectKey(ascii);
                        break; // Apple II only accepts one key at a time
                    }
                }
            }

            previousKeyboardState = state;
        }

        private void DrawLoresMode(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            var loresBuffer = new LoresBuffer();

            loresMemoryReader.ReadLoresPage(loresBuffer);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            var charWidth = font.MeasureString("W").X;
            var charHeight = font.LineSpacing / 2;

            var scaleX = GraphicsDevice.Viewport.Width / (40.0F * charWidth);
            var scaleY = GraphicsDevice.Viewport.Height / (48.0F * charHeight);
            var scale = MathF.Min(scaleX, scaleY);

            var blockWidth = charWidth * scale;
            var blockHeight = charHeight * scale;

            for (var row = 0; row < 24; row++)
            {
                for (var col = 0; col < 40; col++)
                {
                    var cell = loresBuffer.Get(row, col);

                    var topPixel = loresPixels[cell.TopIndex];
                    var posTop = new Vector2(
                        MathF.Floor(col * blockWidth),
                        MathF.Floor(row * 2 * blockHeight)
                    );
                    spriteBatch.Draw(
                        topPixel,
                        new Rectangle(
                            (int)posTop.X,
                            (int)posTop.Y,
                            (int)blockWidth,
                            (int)blockHeight
                        ),
                        cell.Top
                    );

                    var bottomPixel = loresPixels[cell.BottomIndex];
                    var posBottom = new Vector2(
                        MathF.Floor(col * blockWidth),
                        MathF.Floor(((row * 2) + 1) * blockHeight)
                    );
                    spriteBatch.Draw(
                        bottomPixel,
                        new Rectangle(
                            (int)posBottom.X,
                            (int)posBottom.Y,
                            (int)blockWidth,
                            (int)blockHeight
                        ),
                        cell.Bottom
                    );
                }
            }

            spriteBatch.End();
        }

        private void DrawTextMode(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            var textBuffer = new TextBuffer();

            textReader.ReadTextPage(textBuffer);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            var cols = machineState.State[SoftSwitch.EightyColumnMode] ? 80 : 40;

            var charWidth = font.MeasureString("W").X;
            var charHeight = font.LineSpacing;

            var scaleX = GraphicsDevice.Viewport.Width / (cols * charWidth);
            var scaleY = GraphicsDevice.Viewport.Height / (24.0F * charHeight);
            var scale = MathF.Min(scaleX, scaleY);

            for (var row = 0; row < 24; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    var cell = textBuffer.Get(row, col);
                    var c = cell.ToChar();

                    var pos = new Vector2(
                        MathF.Floor(col * charWidth * scale),
                        MathF.Floor(row * charHeight * scale)
                    );

                    var fg = Color.LightGreen;
                    var bg = Color.Black;

                    // Handle inverse
                    if (cell.Attr.HasFlag(TextAttributes.Inverse))
                    {
                        if (cell.Attr.HasFlag(TextAttributes.Flash) && !flashOn)
                        {
                            // Flashing off - draw normally
                            fg = Color.LightGreen;
                            bg = Color.Black;
                        }
                        else
                        {
                            // Draw inverse
                            fg = Color.Black;
                            bg = Color.LightGreen;
                        }
                    }

                    // Draw background rectangle for inverse / flashing
                    if (fg != Color.LightGreen)
                    {
                        spriteBatch.Draw(
                            whitePixel, // a 1x1 white texture
                            new Rectangle(
                                (int)pos.X,
                                (int)pos.Y,
                                (int)(charWidth * scale),
                                (int)(charHeight * scale)
                            ),
                            bg
                        );
                    }

                    spriteBatch.DrawString(
                        font,
                        c.ToString(),
                        pos,
                        fg,
                        rotation: 0f,
                        origin: Vector2.Zero,
                        scale: scale,
                        effects: SpriteEffects.None,
                        layerDepth: 0f
                    );
                }
            }

            spriteBatch.End();
        }

        private void DrawMixedMode(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            var loresBuffer = new LoresBuffer();
            loresMemoryReader.ReadLoresPage(loresBuffer);

            var textBuffer = new TextBuffer();
            textReader.ReadTextPage(textBuffer);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            var charWidth = font.MeasureString("W").X;
            var charHeight = font.LineSpacing / 2;

            var scaleX = GraphicsDevice.Viewport.Width / (40.0F * charWidth);
            var scaleY = GraphicsDevice.Viewport.Height / (48.0F * charHeight);
            var scale = MathF.Min(scaleX, scaleY);

            var blockWidth = charWidth * scale;
            var blockHeight = charHeight * scale;

            for (var row = 0; row < 20; row++)
            {
                for (var col = 0; col < 40; col++)
                {
                    var cell = loresBuffer.Get(row, col);

                    var topPixel = loresPixels[cell.TopIndex];
                    var posTop = new Vector2(
                        MathF.Floor(col * blockWidth),
                        MathF.Floor(row * 2 * blockHeight)
                    );
                    spriteBatch.Draw(
                        topPixel,
                        new Rectangle(
                            (int)posTop.X,
                            (int)posTop.Y,
                            (int)blockWidth,
                            (int)blockHeight
                        ),
                        cell.Top
                    );

                    var bottomPixel = loresPixels[cell.BottomIndex];
                    var posBottom = new Vector2(
                        MathF.Floor(col * blockWidth),
                        MathF.Floor(((row * 2) + 1) * blockHeight)
                    );
                    spriteBatch.Draw(
                        bottomPixel,
                        new Rectangle(
                            (int)posBottom.X,
                            (int)posBottom.Y,
                            (int)blockWidth,
                            (int)blockHeight
                        ),
                        cell.Bottom
                    );
                }
            }

            var cols = machineState.State[SoftSwitch.EightyColumnMode] ? 80 : 40;

            charWidth = font.MeasureString("W").X;
            charHeight = font.LineSpacing;

            scaleX = GraphicsDevice.Viewport.Width / (cols * charWidth);
            scaleY = GraphicsDevice.Viewport.Height / (24.0F * charHeight);
            scale = MathF.Min(scaleX, scaleY);

            for (var row = 20; row < 24; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    var cell = textBuffer.Get(row, col);
                    var c = cell.ToChar();

                    var pos = new Vector2(
                        MathF.Floor(col * charWidth * scale),
                        MathF.Floor(row * charHeight * scale)
                    );

                    var fg = Color.LightGreen;
                    var bg = Color.Black;

                    // Handle inverse
                    if (cell.Attr.HasFlag(TextAttributes.Inverse))
                    {
                        if (cell.Attr.HasFlag(TextAttributes.Flash) && !flashOn)
                        {
                            // Flashing off - draw normally
                            fg = Color.LightGreen;
                            bg = Color.Black;
                        }
                        else
                        {
                            // Draw inverse
                            fg = Color.Black;
                            bg = Color.LightGreen;
                        }
                    }

                    // Draw background rectangle for inverse / flashing
                    if (fg != Color.LightGreen)
                    {
                        spriteBatch.Draw(
                            whitePixel, // a 1x1 white texture
                            new Rectangle(
                                (int)pos.X,
                                (int)pos.Y,
                                (int)(charWidth * scale),
                                (int)(charHeight * scale)
                            ),
                            bg
                        );
                    }

                    spriteBatch.DrawString(
                        font,
                        c.ToString(),
                        pos,
                        fg,
                        rotation: 0f,
                        origin: Vector2.Zero,
                        scale: scale,
                        effects: SpriteEffects.None,
                        layerDepth: 0f
                    );
                }
            }

            spriteBatch.End();
        }
    }
}
