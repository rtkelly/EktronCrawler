using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EktronCrawler;
using MissionSearch.Util;
using System.Configuration;
using System.Linq;
using EktronCrawler.EktronLayer;
using System.Collections.Generic;
using System.IO;
using MissionSearch.Clients;
using MissionSearch.Indexers;


namespace EktronCrawlerTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCrawler()
        {
            CrawlJobsHandler.ProcessJobs();
        }
         
        [TestMethod]
        public void TestAssetTransfer()
        {
           //var location = @"C:\assetlibrary\idsa\00A9DBC5-9A34-44D1-A8DC-615FAA365770\8ecaadde0951444bbd725f4bfa5df1831.pdf";
           //var data = AssetTransfer.GetAsset(location);
        }

        [TestMethod]
        public void TestCreateSearchJsonDoc()
        {
            var indexer = new ContentIndexer("http://localhost:8983/solr/EktronDemo_shard1_replica1", 1, null);
            
            var prop1 = new CrawlerContent()
            {
                Value = new List<string>() {"en", "sv"},
                Name = "Language",
            };

            var prop2 = new CrawlerContent()
            {
                Value = "Test Content",
                Name = "Content",
            };

            var prop3 = new CrawlerContent()
            {
                Value = "Test Content",
                Name = "Content",
            };

            var contentItem = new ContentCrawlProxy();
            
            contentItem.Content.Add(prop1);
            contentItem.Content.Add(prop2);
            contentItem.Content.Add(prop3);
                        
            var result = indexer.CreateSearchJsonDoc("1", contentItem);

        }

        [TestMethod]
        public void TestCrawlDocument()
        {
            //http://www.idsociety.org/uploadedFiles/IDSA/Guidelines-Patient_Care/PDF_Library/2013%20Surgical%20Prophylaxis%20ASHP,%20IDSA,%20SHEA,%20SIS(1).pdf


        }



        [TestMethod]
        public void TestMethod2()
        {
            var req = new ContentRequest()
            {
                FolderIds = new List<long>() {  72 },
            };

            var results = EktronSQL.GetContent(req);

            //CrawlJobsHandler.ProcessJobs<SearchDocument>();
            
            //var client = new EktronCrawler.AssetTransferServiceReference.AssetTransferServerClient();

            //client.GetAsset(()
        }

        [TestMethod]
        public void TestMethod3()
        {
            var results = EktronSQL.GetFolder(72);


        }
        
        
    }
}
