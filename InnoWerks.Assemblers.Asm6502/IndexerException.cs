using System;

namespace InnoWerks.Assemblers
{
    public class IndexerException : Exception
    {
        public IndexerException() { }

        public IndexerException(string message) : base(message) { }

        public IndexerException(string message, Exception innerException) : base(message, innerException) { }

        public IndexerException(string indexer, int lineNumber) : base($"Indexer {indexer} at {lineNumber} must be ,X or ,Y.") { }
    }
}
