using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EktronCrawler;
using MissionSearch.Util;
using System.Configuration;
using System.Linq;



namespace EktronCrawlerTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCrawler()
        {
            var crawler = new ContentCrawler<SearchDocument>();

            var configJson = JsonUtil.ReadJsonFile(ConfigurationManager.AppSettings["CrawlConfigJson"]);
            var crawlSettings = JsonUtil.Deserialize<CrawlSettings>(configJson);

            var job = crawlSettings.crawljobs.First();
            var crawlConfig = crawlSettings.crawlconfigs.First();
            
            crawler.RunJob(job, crawlConfig);
            
            
        }

        
    }
}
