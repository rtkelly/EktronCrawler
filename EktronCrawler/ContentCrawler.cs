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
        EktronLayer.FolderApi FolderApi = new EktronLayer.FolderApi();
        EktronLayer.ContentApi ContentApi = new EktronLayer.ContentApi();
       
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

        }
        
        public IndexResults RunJob(CrawlJob crawlJob, CrawlConfig crawlConfig)
        {
            switch (crawlJob.crawltype)
            {
                case CrawlTypes.PartialCrawl:
                    return RunPartialCrawl(crawlConfig);
                    break;
                case CrawlTypes.FullCrawl:
                    return RunFullCrawlFolder(crawlConfig);
                    break;

                //case CrawlTypes.FullCrawl:
                 //   return RunFullCrawlRoot(crawlConfig);
                 //   break;
            }

            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawlConfig"></param>
        /// <returns></returns>
        public IndexResults RunPartialCrawl(CrawlConfig crawlConfig)
        {
            var startTime = DateTime.Now;

            var contentBuilder = new ContentBuilder<T>(SearchClient, Logger);

            var recentContent = EktronSQL.GetRecentContent(new ContentRequest()
            {
                //LastUpdated = request.LastUpdated.Value,
                XmlConfigIds = crawlConfig.crawlschemaitems.Select(i => i.xmlconfigid),
            });

            var updateList = new List<ContentCrawlParameters>();
            
            foreach(var contentId in recentContent)
            {
                var cData = ContentApi.GetContentItem(contentId);

                if (cData == null)
                    continue;

                var crawlContent = contentBuilder.BuildCrawlContentItem(cData, crawlConfig);
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

            var folder = FolderApi.Get(crawlConfig.rootfolderid);

            var indexResults = new IndexResults();

            if (folder != null)
            {
                var allSubFolders = FolderApi.GetChildFolders(crawlConfig.rootfolderid, true);

                var folderContent = ContentApi.GetFolderContent(crawlConfig.rootfolderid);

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
            Logger.Info(string.Format("Crawling folder {0} id:{1}", folder.Name, folder.Id));
                    
            var xmlConfigIds = crawlConfig.crawlschemaitems.Select(i => i.xmlconfigid).ToList();

            var content = ContentApi.GetFolderContent(folder.Id);

            content.ForEach(p => p.FolderId = folder.Id);
            content.ForEach(p => p.FolderName = folder.Name);
            content.ForEach(p => p.Path = folder.NameWithPath);

            var contentBuilder = new ContentBuilder<T>(SearchClient, Logger);

            var crawledItems = content
                .Where(p => xmlConfigIds.Contains(p.XmlConfiguration.Id))
                .Where(p => p.IsSearchable == true)
                .Select(cData => contentBuilder.BuildCrawlContentItem(cData, crawlConfig))
                .Where(item => item != null)
                .ToList();

            var results = Indexer.RunUpdate(crawledItems, null, null);

            var indexMgr = new ManageIndex<T>(SearchClient);

            // delete items from the index that have been deleted from the folder

            var indexedFolderItems = indexMgr.GetFolderItemsFromIndex(folder.Id);

            if(results.TotalCnt != indexedFolderItems.Count)
            {
                if(results.TotalCnt < indexedFolderItems.Count)
                {
                    foreach(var item in indexedFolderItems)
                    {
                        if(!crawledItems.Any(p => p.ContentItem._ContentID == item.id))
                        {
                            indexMgr.DeleteItem(TypeParser.ParseLong(item.contentid));
                        }
                    }
                }
            }

            return results;
        }

        

      
    }
}
