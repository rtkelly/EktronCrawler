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
            CrawlJobsHandler.ProcessJobs<SearchDocument>();
        }

        
    }
}
