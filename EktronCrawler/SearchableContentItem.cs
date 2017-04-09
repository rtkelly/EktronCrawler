using MissionSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler
{
    public class SearchableContentItem : ISearchableContent
    {
        public string _ContentID { get; set; }

        public string Name { get; set; }

        public bool NotSearchable { get; set; }
    }
}
