using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler
{
    public enum CrawlTypes
    {
        FullCrawl,
        PartialCrawl,
        
    }

    public enum CrawlIntervalTypes
    {
        Minute,
        Hour,
        Day,
        Week,
        Month
        
    }

    public class CrawlSettings
    {
        public List<CrawlJob> crawljobs { get; set; }
        
        public List<CrawlConfig> crawlconfigs { get; set; }
    }

    public class CrawlJob
    {
        public string jobid { get; set; }

        public CrawlTypes crawltype { get; set; }
                
        public CrawlIntervalTypes crawlintervaltype { get; set; }
        
        public int crawlinterval { get; set; }

        public string crawltime { get; set; }

        public string crawlconfigid { get; set; }

        public List<long> rootfolderids { get; set; }

        public List<long> xmlconfigids { get; set; }

        public List<int> contenttypes { get; set; }

        public bool forceoverwrite { get; set; }

        public string logginglevel { get; set; }
                
    }

    public class CrawlConfig
    {
        public string configid { get; set; }

        public string searchconnstr { get; set; }

        public string cmsconnstr { get; set; }

        public string assettransferservice { get; set; }

        public string assetlibrarypath { get; set; }
                        
        public List<CrawlSchemaItem> crawlschemaitems { get; set; }
    }

    public class CrawlSchemaItem
    {
        public bool defaultschema { get; set; }
                
        public long xmlconfigid { get; set; }

        public string[] indexfields { get; set; }

        public string[] storedfields { get; set; }

        public string[] secondarycontent { get; set; }

        public string[] metadata { get; set; }
    }
}
