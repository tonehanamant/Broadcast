using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    [Serializable]
    public class WhosWatchingTvSearchResult
    {
        public Document document { get; set; }

        public class Document
        {
            public List<Title> searchableTitles { get; set; }
        }

        public class Title
        {
            public string type { get; set; }
            public TitleValue value { get; set; }
        }

        public class TitleValue
        {
            public string en { get; set; }
        }
    }
}
