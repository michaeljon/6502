using System.Diagnostics;
using System.Globalization;

namespace InnoWerks.Processors
{
    public static class SimDebugger
    {
        public static void Info(string format, params object[] args)
        {
            Debug.Write("[Info] ");
            Debug.Write(
                string.Format(CultureInfo.InvariantCulture, format, args)
            );
        }

        public static void Warn(string format, params object[] args)
        {
            Debug.Write("[Warn] ");
            Debug.Write(
                string.Format(CultureInfo.InvariantCulture, format, args)
            );
        }

        public static void Error(string format, params object[] args)
        {
            Debug.Write("[Error] ");
            Debug.Write(
                string.Format(CultureInfo.InvariantCulture, format, args)
            );
        }
    }
}