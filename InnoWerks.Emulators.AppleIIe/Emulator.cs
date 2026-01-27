using System.IO;
using InnoWerks.Computers.Apple;
using InnoWerks.Processors;
using InnoWerks.Simulators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace InnoWerks.Emulators.AppleIIe
{
    public class Emulator : Game
    {
        private AppleBus appleBus;
        private MemoryBlocks memoryBlocks;
        private MachineState machineState;
        private IOU iou;
        private MMU mmu;
        private Cpu65C02 cpu;

#pragma warning disable CA2213 // Disposable fields should be disposed
        private readonly GraphicsDeviceManager graphics;

#pragma warning disable CS0169 // Make field read-only
#pragma warning disable IDE0051 // Make field read-only
#pragma warning disable RCS1169 // Make field read-only
#pragma warning disable CA1823 // Avoid unused private fields
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable RCS1213 // Remove unused member declaration
        private SpriteBatch spriteBatch;
#pragma warning restore RCS1213 // Remove unused member declaration
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CA1823 // Avoid unused private fields
#pragma warning restore IDE0051 // Make field read-only
#pragma warning restore CS0169 // Make field read-only

        public Emulator()
        {
            graphics = new GraphicsDeviceManager(this);
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
            // spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var targetCycles = appleBus.CycleCount + VideoTiming.FrameCycles;

            while (appleBus.CycleCount < targetCycles)
            {
                cpu.Step();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
