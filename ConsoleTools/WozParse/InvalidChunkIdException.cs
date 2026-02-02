using System;

namespace WozParse
{
    public class InvalidChunkIdException : Exception
    {
        public InvalidChunkIdException() { }

        public InvalidChunkIdException(string message) : base(message) { }

        public InvalidChunkIdException(string message, Exception innerException) : base(message, innerException) { }

        public InvalidChunkIdException(ChunkId expected, uint got)
            : base($"Unexpected chunk_id, expected {expected}, but found {got:X8}") { }
    }
}
