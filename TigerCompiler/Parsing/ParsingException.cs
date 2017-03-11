using System;
using Antlr.Runtime;

namespace TigerCompiler.Parsing
{
    class ParsingException : Exception
    {
        public ParsingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public RecognitionException RecognitionError
        {
            get
            {
                return InnerException as RecognitionException;
            }
        }
    }
}
