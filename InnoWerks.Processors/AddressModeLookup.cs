using System.Collections.Generic;

namespace InnoWerks.Processors
{
    public static class AddressModeLookup
    {
        private static readonly Dictionary<AddressingMode, string> addressingModeLookup = new()
        {
            { AddressingMode.Unknown,                   "         " },
            { AddressingMode.Implicit,                   "    i    " },
            { AddressingMode.Accumulator,               "    A    " },
            { AddressingMode.Immediate,                 "    #    " },
            { AddressingMode.Absolute,                  "    a    " },
            { AddressingMode.ZeroPage,                  "    zp   " },
            { AddressingMode.Stack,                     "    s    " },
            { AddressingMode.AbsoluteXIndexed,          "    a,x  " },
            { AddressingMode.AbsoluteYIndexed,          "    a,y  " },
            { AddressingMode.ZeroPageXIndexed,          "   zp,x  " },
            { AddressingMode.ZeroPageYIndexed,          "   zp,y  " },
            { AddressingMode.Relative,                  "    r    " },
            { AddressingMode.ZeroPageIndirect,          "   (zp)  " },
            { AddressingMode.AbsoluteIndexedIndirect,   "   (a,x) " },
            { AddressingMode.XIndexedIndirect,          "  (zp,x) " },
            { AddressingMode.IndirectYIndexed,          "  (zp),y " },
            { AddressingMode.AbsoluteIndirect,          "   (a)   " },
        };

        public static string GetDisplay(AddressingMode addressingMode) => addressingModeLookup[addressingMode];
    }
}
