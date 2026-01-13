using System;
using System.Diagnostics;
using System.Globalization;

namespace InnoWerks.Processors
{
    public static class SimDebugger
    {
        public static void Info(string format, params object[] args)
        {
            Msg("[Info] ", false, format, args);
        }

        public static void Warn(string format, params object[] args)
        {
            Msg("[Warn] ", false, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            Msg("[Error] ", false, format, args);
        }

        private static void Msg(string msg, bool nl, string format, params object[] args)
        {
            Debug.Write(msg);
            Debug.Write(string.Format(CultureInfo.InvariantCulture, format, args));
            if (nl == true)
            {
                Debug.Write(Environment.NewLine);
            }
        }
    }
}
