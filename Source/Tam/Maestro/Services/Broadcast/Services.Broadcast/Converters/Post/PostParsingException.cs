using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services.Broadcast.Converters.Post
{
    public class PostParsingException : Exception
    {
        private readonly string _Header;

        public override string Message 
        { 
            get 
            {
                var messageBuilder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(_Header))
                {
                    messageBuilder.AppendLine(_Header);
                }
                if (ParsingErrors?.Any() == true)
                {
                    ParsingErrors.ForEach(e => messageBuilder.AppendLine(e));
                }
                var message = messageBuilder.ToString();
                return message;
            }
        }
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
