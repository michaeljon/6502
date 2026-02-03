using System;

namespace WozParse
{
    public class CrcMismatchException : Exception
    {
        public CrcMismatchException() { }

        public CrcMismatchException(string message) : base(message) { }

        public CrcMismatchException(string message, Exception innerException) : base(message, innerException) { }

        public CrcMismatchException(uint expected, uint got)
            : base($"CRC mismatch, expected {expected} but got {got}") { }
    }
}
