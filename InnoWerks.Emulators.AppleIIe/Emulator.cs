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
        //
        // The Apple IIe itself
        //
        private AppleBus appleBus;
        private MemoryBlocks memoryBlocks;
        private MachineState machineState;
        private IOU iou;
        private MMU mmu;
        private Cpu65C02 cpu;
        private Display display;

        //
        // MonoGame stuff
        //
        private readonly GraphicsDeviceManager graphicsDeviceManager;

        //
        // state stuff
        //
        private KeyboardState previousKeyboardState;
        private double flashTimer;
        private bool flashOn = true;

        public Emulator()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Display.InternalWidth * 2,     // initial width
                PreferredBackBufferHeight = Display.InternalHeight * 2,   // initial height
                IsFullScreen = false
            };
            graphicsDeviceManager.ApplyChanges();

            // Make window resizable
            Window.AllowUserResizing = true;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            graphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
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
            display = new Display(GraphicsDevice, memoryBlocks, machineState);
            display.LoadContent();
        }

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
            display.Draw(flashOn);

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
