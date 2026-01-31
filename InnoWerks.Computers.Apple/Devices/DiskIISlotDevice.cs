using System;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Computers.Apple
{
#pragma warning disable CA1051, IDE1006

    public sealed class DiskIISlotDevice : SlotRomDevice
    {
        private readonly DiskIIDrive drive1 = new();

        private readonly DiskIIDrive drive2 = new();

        DiskIIDrive currentDrive;

        public DiskIISlotDevice(IBus bus, MachineState machineState, byte[] romImage)
            : base(6, "Disk II Controller", bus, machineState, romImage)
        {
            drive1 = new DiskIIDrive();
            drive2 = new DiskIIDrive();
        }

        protected override byte DoIo(CardIoType ioType, byte address, byte value)
        {
            SimDebugger.Info($"Write DiskII({address:X4}, {value:X2})\n");

            switch (address)
            {
                case 0x0:
                case 0x1:
                case 0x2:
                case 0x3:
                case 0x4:
                case 0x5:
                case 0x6:
                case 0x7:
                    currentDrive.step(address);
                    break;

                case 0x8:
                    // drive off
                    currentDrive.setOn(false);
                    currentDrive.removeIndicator();
                    break;

                case 0x9:
                    // drive on
                    currentDrive.setOn(true);
                    currentDrive.addIndicator();
                    break;

                case 0xA:
                    // drive 1
                    currentDrive = drive1;
                    break;

                case 0xB:
                    // drive 2
                    currentDrive = drive2;
                    break;

                case 0xC:
                    // read/write latch
                    currentDrive.write();
                    int latch = currentDrive.readLatch();
                    e.setNewValue(latch);
                    break;
                case 0xF:
                    // write mode
                    currentDrive.setWriteMode();
                case 0xD:
                    // set latch
                    if (e.getType() == RAMEvent.TYPE.WRITE)
                    {
                        currentDrive.setLatchValue((byte)e.getNewValue());
                    }
                    e.setNewValue(currentDrive.readLatch());
                    break;

                case 0xE:
                    // read mode
                    currentDrive.setReadMode();
                    if (currentDrive.disk != null && currentDrive.disk.writeProtected)
                    {
                        e.setNewValue(0x080);
                    }
                    else
                    {
                        //                    e.setNewValue((byte) (Math.random() * 256.0));
                        e.setNewValue(0);
                    }
                    break;
            }

            return machineState.FloatingValue;
        }

        protected override void DoCx(CardIoType ioType, byte address, byte value) { }

        protected override void DoC8(CardIoType ioType, byte address, byte value) { }

        public override void Tick(int cycles) {/* NO-OP */ }

        public override void Reset()
        {
            motorOn = false;
            driveSelect = false;
            phase = 0;
            shiftRegister = 0;
        }

        public DiskIIDrive GetDrive(int drive)
        {
            if (drive < 1 || drive > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(drive), "DiskII Controller support Drive 1 and Drive 2 only");
            }

            return drives[drive];
        }

        private void SetPhase(int p, bool on)
        {
            if (!motorOn) return;

            if (on)
            {
                if (p == (phase + 1) % 4) { CurrentDrive.CurrentTrack++; }
                else if (p == (phase + 3) % 4) { CurrentDrive.CurrentTrack--; }

                CurrentDrive.CurrentTrack = Math.Clamp(CurrentDrive.CurrentTrack, 0, 34);
                phase = p;
            }
        }

        private void ShiftRead()
        {
            if (!motorOn) return;

            var drive = CurrentDrive;
            var track = drive.Tracks[drive.CurrentTrack];

            int bytePos = drive.BitPosition >> 3;
            int bitPos = 7 - (drive.BitPosition & 7);

            int bit = (track[bytePos] >> bitPos) & 1;

            shiftRegister = (byte)((shiftRegister << 1) | bit);

            drive.BitPosition = (drive.BitPosition + 1) % (track.Length * 8);
        }
    }
}
