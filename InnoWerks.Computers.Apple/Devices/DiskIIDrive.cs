
using System;
using System.Diagnostics;
using InnoWerks.Processors;

namespace InnoWerks.Computers.Apple
{
#pragma warning disable CA1822 // Mark members as static

    public class DiskIIDrive
    {
        DiskIIBitStream[] tracks;

        private int halfTrack;
        // private int trackStartOffset;
        private int nibbleOffset;
        private bool writeMode;
        private bool driveOn;
        private int magnets;
        private byte latch;
        private int spinCount;

        private readonly int[][] driveHeadStepDelta = new int[4][];

        public DiskIIDrive()
        {
            driveHeadStepDelta[0] = [0, 0, 1, 1, 0, 0, 1, 1, -1, -1, 0, 0, -1, -1, 0, 0];  // phase 0
            driveHeadStepDelta[1] = [0, -1, 0, -1, 1, 0, 1, 0, 0, -1, 0, -1, 1, 0, 1, 0];  // phase 1
            driveHeadStepDelta[2] = [0, 0, -1, -1, 0, 0, -1, -1, 1, 1, 0, 0, 1, 1, 0, 0];  // phase 2
            driveHeadStepDelta[3] = [0, 1, 0, 1, -1, 0, -1, 0, 0, 1, 0, 1, -1, 0, -1, 0];  // phase 3
        }

        public void InsertDisk(string path)
        {
            tracks = DiskIIBitStream.FromDsk(path);

            driveOn = false;
            magnets = 0;
        }

        public void Reset()
        {
            driveOn = false;
            magnets = 0;
        }

        public void Step(int register)
        {
            // switch drive head stepper motor magnets on/off
            int magnet = (register >> 1) & 0x3;
            magnets &= ~(1 << magnet);
            magnets |= ((register & 0x1) << magnet);

            // step the drive head according to stepper magnet changes
            if (driveOn)
            {
                int delta = driveHeadStepDelta[halfTrack & 0x3][magnets];
                if (delta != 0)
                {
                    int newHalfTrack = halfTrack + delta;
                    if (newHalfTrack < 0)
                    {
                        newHalfTrack = 0;
                    }
                    else if (newHalfTrack > FloppyDisk.HALF_TRACK_COUNT)
                    {
                        newHalfTrack = FloppyDisk.HALF_TRACK_COUNT;
                    }

                    if (newHalfTrack != halfTrack)
                    {
                        halfTrack = newHalfTrack;
                        // trackStartOffset = (halfTrack >> 1) * FloppyDisk.TRACK_NIBBLE_LENGTH;
                        // if (trackStartOffset >= FloppyDisk.DISK_NIBBLE_LENGTH)
                        // {
                        //     trackStartOffset = FloppyDisk.DISK_NIBBLE_LENGTH - FloppyDisk.TRACK_NIBBLE_LENGTH;
                        // }
                        nibbleOffset = 0;

                        SimDebugger.Info($"step {register}, new half track {halfTrack}\n");
                    }
                }
            }
        }

        public void SetOn(bool b)
        {
            driveOn = b;
        }

        public bool IsOn()
        {
            return driveOn;
        }

        public byte ReadLatch()
        {
            byte result = 0x7F;

            if (!writeMode)
            {
                spinCount = (spinCount + 1) & 0x0F;
                if (spinCount > 0)
                {
                    if (tracks != null)
                    {
                        result = tracks[halfTrack].ReadNibble(nibbleOffset);

                        // if (nibbleOffset == 1000)
                        // {
                        //     Debugger.Break();
                        // }

                        // if (nibbleOffset % 50 == 0)
                        // { Debug.WriteLine(""); }

                        // Debug.Write($"{result:X2} ");

                        if (IsOn())
                        {
                            nibbleOffset++;
                            if (nibbleOffset >= FloppyDisk.TRACK_NIBBLE_LENGTH)
                            {
                                nibbleOffset = 0;
                            }
                        }
                    }
                    else
                    {
                        result = (byte)0xff;
                    }
                }
            }
            else
            {
                spinCount = (spinCount + 1) & 0x0F;
                if (spinCount > 0)
                {
                    result = (byte)0x80;
                }
            }

            return result;
        }

        public void Write()
        {
        }

        public void SetReadMode()
        {
            writeMode = false;
        }

        public void SetWriteMode()
        {
            writeMode = true;
        }

        public void SetLatchValue(byte value)
        {
            if (writeMode)
            {
                latch = value;
            }
            else
            {
                latch = (byte)0xFF;
            }
        }
    }
}
