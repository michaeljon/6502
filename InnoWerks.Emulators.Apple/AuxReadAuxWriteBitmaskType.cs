using System.Diagnostics;

namespace InnoWerks.Emulators.Apple
{

    public enum AuxReadAuxWriteBitmaskType
    {
        Undefined,

        NotReadNotWrite,
        NotReadOkWrite,
        OkReadNotWrite,
        OkReadOkWrite
    }
}
