using System;
using System.Collections.Generic;

namespace EktronCrawler
{
    public class CrawlStatus
    {
        public List<CrawlStatusJob> crawlstatusjobs { get; set; }

        public CrawlStatus()
        {
            crawlstatusjobs = new List<CrawlStatusJob>();
        }
    }

    public class CrawlStatusJob
    {
        public string jobid { get; set; }

        public DateTime lastrundate { get; set; }

        public TimeSpan Duration { get; set; }
                
        public long totalcrawled { get; set; }
        
        public long totalerrors { get; set; }

    }
}
