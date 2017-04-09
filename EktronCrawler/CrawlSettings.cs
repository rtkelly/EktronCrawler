﻿using System;
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
        public CrawlTypes crawltype { get; set; }
                
        public CrawlIntervalTypes crawlintervaltype { get; set; }
        
        public int crawlinterval { get; set; }

        public string crawlconfigid { get; set; }
                
    }

    public class CrawlConfig
    {
        public string configid { get; set; }

        public long rootfolderid { get; set; }

        public List<CrawlSchemaItem> crawlschemaitems { get; set; }
    }

    public class CrawlSchemaItem
    {
        public long xmlconfigid { get; set; }

        public string[] indexfields { get; set; }

        public string[] storedfields { get; set; }

        public string[] secondarycontent { get; set; }

        public string[] metadata { get; set; }
    }
}