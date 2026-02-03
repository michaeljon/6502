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
        // Command line options
        //
        private readonly CliOptions options;

        //
        // The Apple IIe itself
        //
        private AppleBus appleBus;
        private Memory128k memoryBlocks;
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

        private KeyboardState prevKeyboard;
        private MouseState prevMouse;

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

        public Emulator(CliOptions options)
        {
            this.options = options;

            graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1160,   // initial width
                PreferredBackBufferHeight = 780,   // initial height
                IsFullScreen = false
            };
            graphicsDeviceManager.ApplyChanges();

            hostLayout = HostLayout.ComputeLayout(
                graphicsDeviceManager.PreferredBackBufferWidth,
                graphicsDeviceManager.PreferredBackBufferHeight
            );

            // Make window resizable
            // Window.AllowUserResizing = true;
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

            var config = new AppleConfiguration(AppleModel.AppleIIe)
            {
                CpuClass = CpuClass.WDC65C02,
                HasAuxMemory = true,
                Has80Column = true,
                HasLowercase = true,
                RamSize = 128
            };

            machineState = new MachineState();
            memoryBlocks = new Memory128k(machineState);

            appleBus = new AppleBus(config, memoryBlocks, machineState);
            iou = new IOU(memoryBlocks, machineState, appleBus);
            mmu = new MMU(memoryBlocks, machineState, appleBus);

            var disk = new DiskIISlotDevice(appleBus, machineState, diskIIRom);
            disk.GetDrive(1).InsertDisk(options.Disk1);
            if (string.IsNullOrEmpty(options.Disk2) == false)
            {
                disk.GetDrive(2).InsertDisk(options.Disk2);
            }

            cpu = new Cpu65C02(
                appleBus,
                (cpu, programCounter) => { },
                (cpu) => { });

            appleBus.LoadProgramToRom(mainRom);

            // var audit = File.ReadAllBytes("tests/audit.o");
            // appleBus.LoadProgramToRam(audit, 0x6000);

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

            var mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                var cpuTraceEntry = display.HandleTraceClick(hostLayout, cpuTraceBuffer, mouse.Position);

                if (cpuTraceEntry != null)
                {
                    if (breakpoints.Add(cpuTraceEntry.Value.ProgramCounter) == false)
                    {
                        breakpoints.Remove(cpuTraceEntry.Value.ProgramCounter);
                    }
                }
            }
            prevMouse = mouse;

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
                var nextInstruction = cpu.PeekInstruction();

                if (breakpoints.Contains(nextInstruction.ProgramCounter))
                {
                    cpuPaused = true;
                    break;
                }

                StepCpuOnce();
            }
        }

        private void StepCpuOnce()
        {
            var nextInstruction = cpu.PeekInstruction();

            cpuTraceBuffer.Add(nextInstruction);

            cpu.Step();
        }

        protected override void Draw(GameTime gameTime)
        {
            display.Draw(hostLayout, cpuTraceBuffer, breakpoints, flashOn);
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

                    // Rebooet
                    if (state.IsKeyDown(Keys.F1) && !prevKeyboard.IsKeyDown(Keys.F1))
                    {
                        cpuPaused = true;
                        cpu.Reset();
                        cpuPaused = false;
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

            graphicsDeviceManager.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphicsDeviceManager.PreferredBackBufferHeight = Window.ClientBounds.Height;

            hostLayout = HostLayout.ComputeLayout(
                Window.ClientBounds.Width,
                Window.ClientBounds.Height
            );

            graphicsDeviceManager.ApplyChanges();

            Window.ClientSizeChanged += HandleResize;
        }
    }
}
