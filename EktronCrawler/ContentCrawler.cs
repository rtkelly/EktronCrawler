using EktronCrawler.EktronLayer;
using EktronCrawler.EktronWeb.FolderApi;
using MissionSearch;
using MissionSearch.Clients;
using MissionSearch.Indexers;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace EktronCrawler
{
    public class ContentCrawler<T> where T : ICMSSearchDocument
    {
        private ISearchClient<T> SearchClient { get; set; }
        private IContentIndexer<T> Indexer { get; set; }
        private ILogger Logger { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public ContentCrawler()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawlJob"></param>
        /// <param name="crawlConfig"></param>
        /// <param name="lastRun"></param>
        /// <returns></returns>
        public IndexResults RunJob(CrawlJob crawlJob, CrawlConfig crawlConfig, DateTime lastRun)
        {
            MissionLogger.LoggerLevel logLevel;

            switch(crawlJob.logginglevel)
            {
                case "Debug":
                    logLevel = MissionLogger.LoggerLevel.Debug;
                    break;

                default:
                case "Error":
                    logLevel = MissionLogger.LoggerLevel.Error;
                    break;

                case "Info":
                    logLevel = MissionLogger.LoggerLevel.Info;
                    break;
            }

            var logFile = string.Format("{0}CrawlLog_{1}.log", ConfigurationManager.AppSettings["CrawlLogsPath"], crawlJob.jobid);

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }

            Logger = new MissionLogger(logFile, logLevel);

            Logger.Info(string.Format("Starting Crawl Job {0}", crawlJob.jobid));

            SearchClient = new SolrClient<T>(crawlConfig.searchconnstr);
            
            Indexer = new DefaultContentIndexer<T>(SearchClient, 1, Logger);

            SearchClient.Timeout = 10000;

            switch (crawlJob.crawltype)
            {
                case CrawlTypes.PartialCrawl:
                    return RunPartialCrawl(crawlJob, crawlConfig, lastRun);
                
                case CrawlTypes.FullCrawl:
                    return RunFullCrawl(crawlJob, crawlConfig);
            }

            return new IndexResults();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="crawlConfig"></param>
        /// <param name="lastUpdated"></param>
        /// <returns></returns>
        public IndexResults RunPartialCrawl(CrawlJob job, CrawlConfig crawlConfig, DateTime lastUpdated)
        {
            Logger.Info("Starting Partial Crawl");

            if(lastUpdated == DateTime.MinValue)
            {
                return RunFullCrawl(job, crawlConfig);
            }

            var startTime = DateTime.Now;
                        
            var contentBuilder = new ContentBuilder<T>(SearchClient, Logger);

            var req = new ContentRequest()
            {
                LastUpdated = lastUpdated,
                XmlConfigIds = job.xmlconfigids,
                ContentTypes = job.contenttypes,
            
            };

            EktronSQL.ConnectionString = crawlConfig.cmsconnstr;

            var recentContent = EktronSQL.GetContent(req);

            var folderids = new List<long>();

            if(job.rootfolderids != null)
            {
                foreach(var folderid in job.rootfolderids)
                {
                    var folder = EktronSQL.GetFolder(folderid);

                    var allSubFolders = EktronSQL.GetSubFolders(folder.FolderIdWithPath);

                    if(allSubFolders.Any())
                    {
                        folderids.AddRange(allSubFolders.Select(p => p.Id));
                    }
                }
            }

            var updateList = new List<ContentCrawlParameters>();
            
            foreach(var cData in recentContent)
            {
                if (cData == null)
                    continue;

                if (job.rootfolderids != null && !folderids.Contains(cData.FolderId))
                    continue;

                var crawlContent = contentBuilder.BuildCrawlContentItem(cData, crawlConfig);
                
                if(crawlContent != null)
                    updateList.Add(crawlContent);
            }

            var indexResults = Indexer.RunUpdate(updateList, null, null);

            var duration = (DateTime.Now - startTime);

            Logger.Info(string.Format("Partial Crawl Completed Total Content Crawled: {0} Total Errors: {1} Total Time: {2} hours {3} minutes", indexResults.TotalCnt, indexResults.ErrorCnt, duration.Hours, duration.Minutes));

            return indexResults;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="crawlConfig"></param>
        /// <returns></returns>
        public IndexResults RunFullCrawl(CrawlJob job, CrawlConfig crawlConfig)
        {
            Logger.Error("Starting Full Crawl");

            var startTime = DateTime.Now;

            var indexResults = new IndexResults();

            var folderIds = job.rootfolderids ?? new List<long>() { 0 };

            EktronSQL.ConnectionString = crawlConfig.cmsconnstr;

            foreach (var folderid in folderIds)
            {
                var folder = EktronSQL.GetFolder(folderid);
                
                if (folder != null)
                {
                    indexResults = CrawlAndIndexFolder(folder, crawlConfig, job);
                }
            }

            indexResults.Duration = (DateTime.Now - startTime);

            //RunFolderGarbageCollector();

            Logger.Error(string.Format("Full Crawl Completed Total Content Crawled: {0} Total Errors: {1} Total Time: {2} hours {3} minutes", indexResults.TotalCnt, indexResults.ErrorCnt, indexResults.Duration.Hours, indexResults.Duration.Minutes));
            
            return indexResults;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="crawlConfig"></param>
        /// <returns></returns>
        public void RunFolderGarbageCollector()
        {
            Logger.Info("Running Folder Garbage Collection");

            var indexMgr = new ManageIndex<T>(SearchClient);

            var foldersInCms = EktronSQL.GetFolders();
            var foldersInIndex = indexMgr.GetFolderIdsFromIndex();

            foreach (var folderid in foldersInIndex)
            {
                if (foldersInCms.All(f => f.Id != folderid))
                {
                    // if folder is no longer in cms then 
                    // delete all content in folder from index
                    SearchClient.Delete(string.Format("folderid:{0}", folderid));

                    Logger.Debug(string.Format("Deleted folderid:{0} content from index", folderid));
                }
            }
            
        }


       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="crawlConfig"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        private IndexResults CrawlAndIndexFolder(FolderData folder, CrawlConfig crawlConfig, CrawlJob job)
        {
            var results = new IndexResults();

            Logger.Debug(string.Format("Crawling folder {0} id:{1}", folder.Name, folder.Id));
            
            try
            {
                var indexMgr = new ManageIndex<T>(SearchClient);

                var indexedContentItems = indexMgr.GetFolderItemsFromIndex(folder.Id);
                                
                var contentItems = EktronSQL.GetContent(new ContentRequest()
                {
                    FolderIds = new List<long>() { folder.Id },
                    XmlConfigIds = job.xmlconfigids,
                    ContentTypes = job.contenttypes,
                });

                contentItems.ForEach(p => p.FolderName = folder.Name);
                contentItems.ForEach(p => p.Path = folder.NameWithPath);

                var contentBuilder = new ContentBuilder<T>(SearchClient, Logger);

                var crawledItems = new List<ContentCrawlParameters>();

                foreach (var contentItem in contentItems)
                {
                    if (!job.forceoverwrite)
                    {
                        var indexedContent = indexedContentItems.FirstOrDefault(p => p.contentid == contentItem.Id.ToString());

                        if (indexedContent != null)
                        {
                            var lastEditDate = TypeParser.ParseDateTime(contentItem.DisplayLastEditDate);

                            if (lastEditDate != null && indexedContent.lastcrawled < lastEditDate.Value)
                            {
                                Logger.Debug(string.Format("Skipping content id:\"{0}\"", contentItem.Id));
                                continue;
                            }
                        }
                    }
                    
                    var crawlItem = contentBuilder.BuildCrawlContentItem(contentItem, crawlConfig);
                    crawledItems.Add(crawlItem);
                }

                                
                results = Indexer.RunUpdate(crawledItems, null, null);
                                
                // delete items from the index that have been deleted from the folder
                indexedContentItems = indexMgr.GetFolderItemsFromIndex(folder.Id);
                
                if (contentItems.Count < indexedContentItems.Count)
                {
                    if (results.TotalCnt < indexedContentItems.Count)
                    {
                        foreach (var item in indexedContentItems)
                        {
                            if (!contentItems.Any(p => p.Id.ToString() == item.id))
                            {
                                indexMgr.DeleteItem(TypeParser.ParseLong(item.contentid));
                            }
                        }
                    }
                }

                var allSubFolders = EktronSQL.GetSubFolders(folder.Id);

                foreach (var subFolder in allSubFolders)
                {
                   results.Combine(CrawlAndIndexFolder(subFolder, crawlConfig, job));
                }

                
            }
            catch(Exception ex)
            {
                Logger.Error(string.Format("{0} {1}", ex.Message, ex.StackTrace));
            }

            return results;
            
        }

        

      
    }
}
