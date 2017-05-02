using MissionSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawlerService
{
    public class SearchDocument : ICMSSearchDocument
    {
        public string id { get; set; }

        public string contentid { get; set; }
        
        public int sourceid { get; set; }

        public string title { get; set; }

        public string summary { get; set; }

        public string url { get; set; }

        public List<string> content { get; set; }

        public List<string> metadata { get; set; }

        public List<string> metadata_map { get; set; }

        public List<string> taxonomy { get; set; }

        public List<string> taxonomy_map { get; set; }
        
        public DateTime timestamp { get; set; }
        
        public string highlightsummary { get; set; }

        public long xmlconfigid { get; set; }

        public long folderid { get; set; }

        public string foldername { get; set; }

        public string path { get; set; }

        public int contenttypeid { get; set; }
                
        public string mimetype { get; set; }

        public string hostname { get; set; }

        public List<string> paths { get; set; }

        public List<string> categories { get; set; }

        public string folder { get; set; }

        public List<string> language { get; set; }

        public string pagetype { get; set; }

        public string published { get; set; }

        public DateTime lastcrawled { get; set; }
        
        public DateTime publisheddate { get; set; }

        public string author { get; set; }
    }
}
