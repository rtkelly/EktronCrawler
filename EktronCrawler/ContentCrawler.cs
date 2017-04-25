using EktronCrawler.EktronLayer;
using EktronCrawler.EktronWeb.FolderApi;
using MissionSearch;
using MissionSearch.Clients;
using MissionSearch.Indexers;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace EktronCrawler
{
    public class ContentCrawler<T> where T : ICMSSearchDocument
    {
        private FolderApi FolderMgr { get; set; }
        //private ContentApi ContentMgr { get; set; }
        private ISearchClient<T> SearchClient { get; set; }
        private IContentIndexer<T> Indexer { get; set; }
        private ILogger Logger { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public ContentCrawler()
        {
            Logger = new MissionLogger(ConfigurationManager.AppSettings["CrawlLogFile"], MissionLogger.LoggerLevel.Info);
            SearchClient = new SolrClient<T>(ConfigurationManager.AppSettings["SearchConnectionString"]);
            Indexer = new DefaultContentIndexer<T>(SearchClient, 1, Logger);

            FolderMgr = new FolderApi();
            //ContentMgr = new ContentApi();

            SearchClient.Timeout = 10000;
        }
        
        public IndexResults RunJob(CrawlJob crawlJob, CrawlConfig crawlConfig, DateTime lastRun)
        {
            switch (crawlJob.crawltype)
            {
                case CrawlTypes.PartialCrawl:
                    return RunPartialCrawl(crawlJob, crawlConfig, lastRun);
                case CrawlTypes.FullCrawl:
                    return RunFullCrawlFolder(crawlJob, crawlConfig);
                //case CrawlTypes.FullCrawl:
                 //   return RunFullCrawlRoot(crawlConfig);
                 //   break;
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
            var startTime = DateTime.Now;
                        
            var contentBuilder = new ContentBuilder<T>(SearchClient, Logger);

            var req = new ContentRequest()
            {
                LastUpdated = lastUpdated,
                XmlConfigIds = job.xmlconfigids,
                ContentTypes = job.contenttypes,
            
            };

            var recentContent = EktronSQL.GetContent(req);

            var folderids = new List<long>();

            if(job.rootfolderids != null)
            {
                foreach(var folderid in job.rootfolderids)
                {
                    var allSubFolders = FolderMgr.GetChildFolders(folderid, true);

                    if(allSubFolders.Any())
                    {
                        folderids.AddRange(allSubFolders.Select(p => p.Id));
                    }
                }
            }

            var updateList = new List<ContentCrawlParameters>();
            
            foreach(var cData in recentContent)
            {
                //var cData = ContentMgr.GetContentItem(contentId);

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
        public IndexResults RunFullCrawlFolder(CrawlJob job, CrawlConfig crawlConfig)
        {
            var startTime = DateTime.Now;

            var indexResults = new IndexResults();

            var folderIds = job.rootfolderids ?? new List<long>() { 0 };

            foreach (var folderid in folderIds)
            {
                var folder = FolderMgr.Get(folderid);
                                
                if (folder != null)
                {
                    var allSubFolders = FolderMgr.GetChildFolders(folderid, true);

                    //var folderContent = ContentMgr.GetFolderContent(folderid);

                    indexResults = CrawlAndIndexFolder(folder, crawlConfig, job);

                    foreach (var subFolder in allSubFolders)
                    {
                        indexResults.Combine(CrawlAndIndexFolder(subFolder, crawlConfig, job));
                    }
                }
            }

            indexResults.Duration = (DateTime.Now - startTime);

            Logger.Info(string.Format("Full Crawl Completed Total Content Crawled: {0} Total Errors: {1} Total Time: {2} hours {3} minutes", indexResults.TotalCnt, indexResults.ErrorCnt, indexResults.Duration.Hours, indexResults.Duration.Minutes));
            
            return indexResults;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="crawlConfig"></param>
        /// <returns></returns>
        public IndexResults RunFullCrawlRoot(CrawlJob job, CrawlConfig crawlConfig)
        {
            var startTime = DateTime.Now;

            var folders = EktronSQL.GetFolders();

            var indexResults = new IndexResults();

            foreach (var folder in folders)
            {
                indexResults.Combine(CrawlAndIndexFolder(folder, crawlConfig, job));
            }
            
            var indexMgr = new ManageIndex<T>(SearchClient);

            var allFolderIds = indexMgr.GetFolderIdsFromIndex();

            foreach(var folderid in allFolderIds)
            {
                if(folders.All(f => f.Id != folderid))
                {
                    // if folder is no longer is cms then 
                    // delete all content in folder from index
                    //SearchClient.Delete(string.Format("folderid:{0}", folderid));
                }
            }

            var duration  = (DateTime.Now - startTime);

            Logger.Info(string.Format("Full Crawl Completed Total Content Crawled: {0} Total Errors: {1} Total Time: {2} hours {3} minutes", indexResults.TotalCnt, indexResults.ErrorCnt, duration.Hours, duration.Minutes));

            return indexResults;
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

            Logger.Info(string.Format("Crawling folder {0} id:{1}", folder.Name, folder.Id));
            
            try
            {
                var indexMgr = new ManageIndex<T>(SearchClient);

                var indexedContentItems = indexMgr.GetFolderItemsFromIndex(folder.Id);

                //var xmlConfigIds = crawlConfig.crawlschemaitems.Select(i => i.xmlconfigid).ToList();
                //var contentItems = ContentMgr.GetFolderContent(folder.Id);

                var contentItems = EktronSQL.GetContent(new ContentRequest()
                {
                    FolderIds = new List<long>() { folder.Id },
                    XmlConfigIds = job.xmlconfigids,
                    ContentTypes = job.contenttypes,
                });

                //contentItems.ForEach(p => p.FolderId = folder.Id);
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

                            if (lastEditDate != null && indexedContent.lastcrawled > lastEditDate.Value)
                            {
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

                
            }
            catch(Exception ex)
            {
                Logger.Error(string.Format("{0}", ex.Message));
            }

            return results;
            
        }

        

      
    }
}
