using System;
using System.IO;
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

        private KeyboardState previousKeyboardState;

        TextMemoryReader textReader;

        TextCell[,] textBuffer = new TextCell[24, 80];


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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("CourierFont");

            textReader = new TextMemoryReader(memoryBlocks, machineState);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var targetCycles = appleBus.CycleCount + VideoTiming.FrameCycles;

            while (appleBus.CycleCount < targetCycles)
            {
                cpu.Step();

                HandleKeyboardInput();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            textReader.ReadTextPage(textBuffer);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            var cols = machineState.State[SoftSwitch.EightyColumnMode] ? 80 : 40;

            var charWidth = font.MeasureString("W").X;
            var charHeight = font.LineSpacing;

            var scaleX = GraphicsDevice.Viewport.Width / (cols * charWidth);
            var scaleY = GraphicsDevice.Viewport.Height / (24 * charHeight);
            var scale = MathF.Max(1f, MathF.Min(scaleX, scaleY));

            for (var row = 0; row < 24; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    var c = textBuffer[row, col].ToChar();

                    var pos = new Vector2(
                        col * charWidth * scale,
                        row * charHeight * scale
                    );

                    spriteBatch.DrawString(
                        font,
                        c.ToString(),
                        pos,
                        Color.LightGreen,
                        rotation: 0f,
                        origin: Vector2.Zero,
                        scale: scale,
                        effects: SpriteEffects.None,
                        layerDepth: 0f
                    );
                }
            }

            spriteBatch.End();

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

    }
}
