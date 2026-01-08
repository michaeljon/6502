#pragma warning disable CA1822, IDE0060, RCS1163

using System.Text;
using InnoWerks.Simulators;

namespace Emu6502
{
    public class IOHandler
    {
        public byte Read(IBus memory, ushort addr)
        {
            if (0xc000 <= addr && addr <= 0xc00f)
            {
                // read keyboard
                return (byte)(Encoding.ASCII.GetBytes(['A'])[0] | 0x80);
            }
            else if (0xc010 <= addr && addr <= 0xc01f)
            {
                // strobe keyboard
            }
            else if (0xc020 <= addr && addr <= 0xc02f)
            {
                // write cassette
            }
            else if (0xc030 <= addr && addr <= 0xc03f)
            {
                // toggle speaker
            }


            return 0x00;
        }

        public byte Write(IBus memory, ushort addr, byte value)
        {
            return 0x00;
        }
    }
}
