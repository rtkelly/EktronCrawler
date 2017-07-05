using MissionSearch;

namespace EktronCrawler
{
    public class SearchableContentItem : ISearchableContent
    {
        public string _ContentID { get; set; }

        public string Name { get; set; }

        public bool NotSearchable { get; set; }
    }
}
