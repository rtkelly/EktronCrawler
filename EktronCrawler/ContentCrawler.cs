using EktronCrawler.EktronLayer;
using EktronCrawler.EktronWeb.ContentApi;
using EktronCrawler.EktronWeb.FolderApi;
using MissionSearch;
using MissionSearch.Clients;
using MissionSearch.Indexers;
using MissionSearch.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler
{
    public class ContentCrawler<T> where T : ICMSSearchDocument
    {
        FolderApi FolderMgr = new FolderApi();
        ContentApi ContentMgr = new ContentApi();
       
        ISearchClient<T> SearchClient { get; set; }
        IContentIndexer<T> Indexer { get; set; }
        ILogger Logger { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public ContentCrawler()
        {
            Logger = new MissionLogger(ConfigurationManager.AppSettings["CrawlLogFile"], MissionLogger.LoggerLevel.Info);
            SearchClient = new SolrClient<T>(ConfigurationManager.AppSettings["SearchConnectionString"]);
            Indexer = new DefaultContentIndexer<T>(SearchClient, 1, Logger);

            SearchClient.Timeout = 10000;
        }
        
        public IndexResults RunJob(CrawlJob crawlJob, CrawlConfig crawlConfig, DateTime lastRun)
        {
            switch (crawlJob.crawltype)
            {
                case CrawlTypes.PartialCrawl:
                    return RunPartialCrawl(crawlConfig, lastRun);
                    break;
                case CrawlTypes.FullCrawl:
                    return RunFullCrawlFolder(crawlConfig);
                    break;

                //case CrawlTypes.FullCrawl:
                 //   return RunFullCrawlRoot(crawlConfig);
                 //   break;
            }

            return new IndexResults();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawlConfig"></param>
        /// <returns></returns>
        public IndexResults RunPartialCrawl(CrawlConfig crawlConfig, DateTime lastUpdated)
        {
            var startTime = DateTime.Now;

            var contentBuilder = new ContentBuilder<T>(SearchClient, Logger);

            var recentContent = EktronSQL.GetRecentContent(new ContentRequest()
            {
                LastUpdated = lastUpdated,
                XmlConfigIds = crawlConfig.crawlschemaitems.Select(i => i.xmlconfigid),
            });

            var updateList = new List<ContentCrawlParameters>();
            
            foreach(var contentId in recentContent)
            {
                var cData = ContentMgr.GetContentItem(contentId);

                if (cData == null)
                    continue;

                var crawlContent = contentBuilder.BuildCrawlContentItem(cData, crawlConfig);
                
                if(crawlContent != null)
                    updateList.Add(crawlContent);
            }

            var indexResults = Indexer.RunUpdate(updateList, null, null);

            var duration = (DateTime.Now - startTime);

            Logger.Info(string.Format("Full Crawl Completed Total Content Crawled: {0} Total Errors: {1} Total Time: {2} hours {3} minutes", indexResults.TotalCnt, indexResults.ErrorCnt, duration.Hours, duration.Minutes));

            return indexResults;
        }
         


       /// <summary>
       /// 
       /// </summary>
       /// <param name="crawlConfig"></param>
       /// <returns></returns>
        public IndexResults RunFullCrawlFolder(CrawlConfig crawlConfig)
        {
            var startTime = DateTime.Now;

            var folder = FolderMgr.Get(crawlConfig.rootfolderid);

            var indexResults = new IndexResults();

            if (folder != null)
            {
                var allSubFolders = FolderMgr.GetChildFolders(crawlConfig.rootfolderid, true);

                var folderContent = ContentMgr.GetFolderContent(crawlConfig.rootfolderid);

                indexResults = CrawlAndIndexFolder(folder, crawlConfig);

                foreach (var subFolder in allSubFolders)
                {
                    indexResults.Combine(CrawlAndIndexFolder(subFolder, crawlConfig));
                }
            }

            var duration = (DateTime.Now - startTime);

            Logger.Info(string.Format("Full Crawl Completed Total Content Crawled: {0} Total Errors: {1} Total Time: {2} hours {3} minutes", indexResults.TotalCnt, indexResults.ErrorCnt, duration.Hours, duration.Minutes));
            
            return indexResults;
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="crawlConfig"></param>
       /// <returns></returns>
        public IndexResults RunFullCrawlRoot(CrawlConfig crawlConfig)
        {
            var startTime = DateTime.Now;

            var folders = EktronSQL.GetFolders();

            var indexResults = new IndexResults();

            foreach (var folder in folders)
            {
                indexResults.Combine(CrawlAndIndexFolder(folder, crawlConfig));
            }
            
            var indexMgr = new ManageIndex<T>(SearchClient);

            var allFolderIds = indexMgr.GetFolderIdsFromIndex();

            foreach(var folderid in allFolderIds)
            {
                if(!folders.Any(f => f.Id == folderid))
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
        /// <returns></returns>
        private IndexResults CrawlAndIndexFolder(FolderData folder, CrawlConfig crawlConfig)
        {
            var results = new IndexResults();

            Logger.Info(string.Format("Crawling folder {0} id:{1}", folder.Name, folder.Id));
            
            try
            {
                var indexMgr = new ManageIndex<T>(SearchClient);

                var indexedContentItems = indexMgr.GetFolderItemsFromIndex(folder.Id);

                var xmlConfigIds = crawlConfig.crawlschemaitems.Select(i => i.xmlconfigid).ToList();

                var contentItems = ContentMgr.GetFolderContent(folder.Id);

                contentItems.ForEach(p => p.FolderId = folder.Id);
                contentItems.ForEach(p => p.FolderName = folder.Name);
                contentItems.ForEach(p => p.Path = folder.NameWithPath);

                var contentBuilder = new ContentBuilder<T>(SearchClient, Logger);

                var crawledItems = new List<ContentCrawlParameters>();

                foreach (var contentItem in contentItems)
                {
                    if (!xmlConfigIds.Contains(contentItem.XmlConfiguration.Id))
                        continue;

                    if (!contentItem.IsSearchable)
                        continue;

                    if (!crawlConfig.forceoverwrite)
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

                /*
                var crawledItems = contentItems
                    .Where(p => xmlConfigIds.Contains(p.XmlConfiguration.Id))
                    .Where(p => p.IsSearchable == true)
                    //.Where(p => p.la)
                    .Select(cData => contentBuilder.BuildCrawlContentItem(cData, crawlConfig))
                    .Where(item => item != null)
                    .ToList();
                */
                                
                results = Indexer.RunUpdate(crawledItems, null, null);
                                
                // delete items from the index that have been deleted from the folder
                indexedContentItems = indexMgr.GetFolderItemsFromIndex(folder.Id);
                
                if (contentItems.Count() < indexedContentItems.Count)
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
