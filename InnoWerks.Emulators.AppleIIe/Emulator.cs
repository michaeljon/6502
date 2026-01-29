using System;
using System.Collections.Generic;
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

        //
        // debug, etc.
        //
        private CpuTraceBuffer cpuTraceBuffer = new(128);
        private bool cpuPaused;
        private bool stepRequested;
        private readonly HashSet<ushort> breakpoints = [];

        KeyboardState prevKeyboard;

        //
        // display renderer
        //
        private Display display;

        //
        // MonoGame stuff
        //
        private readonly GraphicsDeviceManager graphicsDeviceManager;

        //
        // layout stuff
        //
        private HostLayout hostLayout;

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
                PreferredBackBufferWidth = Display.AppleDisplayWidth * 4,     // initial width
                PreferredBackBufferHeight = Display.AppleDisplayHeight * 4,   // initial height
                IsFullScreen = false
            };
            graphicsDeviceManager.ApplyChanges();

            hostLayout = HostLayout.ComputeLayout(
                Display.AppleDisplayWidth * 4,
                Display.AppleDisplayWidth * 4
            );

            // Make window resizable
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += HandleResize;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            graphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            Window.Title = "Apple IIe";

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
            display = new Display(GraphicsDevice, cpu, appleBus, memoryBlocks, machineState);

            display.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            ArgumentNullException.ThrowIfNull(gameTime);

            HandleKeyboardInput();

            // var mouse = Mouse.GetState();

            // if (mouse.LeftButton == ButtonState.Pressed &&
            //     prevMouse.LeftButton == ButtonState.Released)
            // {
            //     HandleTraceClick(mouse.Position);
            // }

            // prevMouse = mouse;

            RunEmulator();

            // Toggle flashing every 500ms
            flashTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (flashTimer >= 500)
            {
                flashTimer = 0;
                flashOn = !flashOn;
            }

            base.Update(gameTime);
        }

        private void RunEmulator()
        {
            if (!cpuPaused)
            {
                RunCpuForFrame();
            }
            else if (stepRequested)
            {
                StepCpuOnce();
                stepRequested = false;
            }
        }

        private void RunCpuForFrame()
        {
            var targetCycles = appleBus.CycleCount + VideoTiming.FrameCycles;
            while (appleBus.CycleCount < targetCycles)
            {
                var peek = cpu.PeekInstruction();

                if (breakpoints.Contains(peek.ProgramCounter))
                {
                    cpuPaused = true;
                    break;
                }

                StepCpuOnce();
            }
        }

        private void StepCpuOnce()
        {
            var peek = cpu.PeekInstruction();

            if (breakpoints.Contains(peek.ProgramCounter))
            {
                cpuPaused = true;
                return;
            }

            cpuTraceBuffer.Add(peek);

            cpu.Step();
        }

        protected override void Draw(GameTime gameTime)
        {
            display.Draw(hostLayout, cpuTraceBuffer, flashOn);

            base.Draw(gameTime);
        }

        private void HandleKeyboardInput()
        {
            var state = Keyboard.GetState();

            foreach (var key in state.GetPressedKeys())
            {
                if (previousKeyboardState.IsKeyUp(key))
                {
                    // Toggle pause
                    if (state.IsKeyDown(Keys.F5) && !prevKeyboard.IsKeyDown(Keys.F5))
                    {
                        cpuPaused = !cpuPaused;
                    }

                    // Single-step
                    if (state.IsKeyDown(Keys.F6) && !prevKeyboard.IsKeyDown(Keys.F6))
                    {
                        if (cpuPaused)
                        {
                            stepRequested = true;
                        }
                    }

                    if (KeyMapper.TryMap(key, state, out byte ascii))
                    {
                        iou.InjectKey(ascii);
                        break; // Apple II only accepts one key at a time
                    }
                }
            }

            previousKeyboardState = state;
        }

        private void HandleResize(object sender, EventArgs e)
        {
            Window.ClientSizeChanged -= HandleResize;

            graphicsDeviceManager.PreferredBackBufferWidth =
                Math.Max(Window.ClientBounds.Width, Display.AppleDisplayWidth * 4);
            graphicsDeviceManager.PreferredBackBufferHeight =
                Math.Max(Window.ClientBounds.Height, Display.AppleDisplayHeight * 4);

            hostLayout = HostLayout.ComputeLayout(
                Math.Max(Window.ClientBounds.Width, Display.AppleDisplayWidth * 4),
                Math.Max(Window.ClientBounds.Height, Display.AppleDisplayHeight * 4)
            );

            graphicsDeviceManager.ApplyChanges();

            Window.ClientSizeChanged += HandleResize;
        }
    }
}
