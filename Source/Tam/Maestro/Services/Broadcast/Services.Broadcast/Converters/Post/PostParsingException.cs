using System;
using System.Collections.Generic;

namespace Services.Broadcast.Converters.Post
{
    public class PostParsingException : Exception
    {
        private readonly string _Header;

        public override string Message { get { return _Header + string.Join("\n", ParsingErrors); } }
        public readonly List<string> ParsingErrors;

        public PostParsingException(string header, List<string> parsingErrors)
            : this(parsingErrors)
        {
            _Header = header;
        }

        public PostParsingException(List<string> parsingErrors)
        {
            ParsingErrors = parsingErrors;
        }
    }
}
