using System;

namespace InnoWerks.Assemblers
{
    public class InvalidInstructionFormatException : Exception
    {
        public InvalidInstructionFormatException() { }

        public InvalidInstructionFormatException(string message) : base(message) { }

        public InvalidInstructionFormatException(string message, Exception innerException) : base(message, innerException) { }

        public InvalidInstructionFormatException(int lineNumber)
            : base($"Instruction at line {lineNumber} is incorrectly formatted") { }
    }
}
