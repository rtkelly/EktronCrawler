using EktronCrawler.EktronLayer;
using EktronCrawler.EktronWeb.ContentApi;
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
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EktronCrawler
{
    public class ContentBuilder<T> where T : ISearchDocument
    {
        EktronWeb.MetaDataApi.Metadata MetadataApi = new EktronWeb.MetaDataApi.Metadata();
        EktronWeb.TaxonomyApi.Taxonomy TaxonomyApi = new EktronWeb.TaxonomyApi.Taxonomy();

        EktronLayer.ContentApi ContentApi = new EktronLayer.ContentApi();
        ISearchClient<T> SearchClient { get; set; }
       
        ILogger Logger;

        string AssetLibraryPath;

        public ContentBuilder(ISearchClient<T> client, ILogger logger)
        {
            SearchClient = client;
            Logger = logger;

            AssetLibraryPath = ConfigurationManager.AppSettings["AssetLibraryFolder"];
        }

        public ContentCrawlParameters BuildCrawlContentItem(ContentData cData, CrawlConfig crawlConfig)
        {
            try
            {
                var lastCrawledDate = new DateTime();

                var crawlItem = new ContentCrawlParameters();

                crawlItem.ContentItem = new SearchableContentItem();

                var metadata = GetMetaData(cData.Id);

                if (metadata.Item1 != null && metadata.Item2 != null)
                {
                    crawlItem.Content.Add(new CrawlerContent() { Name = "metadata", Value = metadata.Item1 });
                    crawlItem.Content.Add(new CrawlerContent() { Name = "metadata_map", Value = metadata.Item2 });
                }

                var taxonomy = GetTaxonomy(cData.Id);

                if (taxonomy.Item1 != null)
                {
                    crawlItem.Content.Add(new CrawlerContent() { Name = "taxonomy", Value = taxonomy.Item1 });
                    crawlItem.Content.Add(new CrawlerContent() { Name = "taxonomy_map", Value = taxonomy.Item2 });
                }

                crawlItem.ContentItem._ContentID = string.Format("{0}|{1}", cData.Id, cData.LanguageId);
                crawlItem.Content.Add(new CrawlerContent() { Name = "contentid", Value = cData.Id.ToString() });
                crawlItem.Content.Add(new CrawlerContent() { Name = "title", Value = cData.Title });
                crawlItem.Content.Add(new CrawlerContent() { Name = "url", Value = cData.Quicklink });
                crawlItem.Content.Add(new CrawlerContent() { Name = "summary", Value = HtmlParser.StripHTML(cData.Teaser) });
                crawlItem.Content.Add(new CrawlerContent() { Name = "xmlconfigid", Value = cData.XmlConfiguration.Id });
                crawlItem.Content.Add(new CrawlerContent() { Name = "folderid", Value = cData.FolderId });
                crawlItem.Content.Add(new CrawlerContent() { Name = "contenttypeid", Value = cData.ContType });
                crawlItem.Content.Add(new CrawlerContent() { Name = "timestamp", Value = cData.DateCreated });
                crawlItem.Content.Add(new CrawlerContent() { Name = "path", Value = cData.Path });
                crawlItem.Content.Add(new CrawlerContent() { Name = "foldername", Value = cData.FolderName });
                crawlItem.Content.Add(new CrawlerContent() { Name = "folder", Value = "" });
                crawlItem.Content.Add(new CrawlerContent() { Name = "language", Value = cData.LanguageId.ToString() });
                crawlItem.Content.Add(new CrawlerContent() { Name = "mimetype", Value = "" });
                crawlItem.Content.Add(new CrawlerContent() { Name = "categories", Value = new List<string>() });
                crawlItem.Content.Add(new CrawlerContent() { Name = "pagetype", Value = "" });
                crawlItem.Content.Add(new CrawlerContent() { Name = "paths", Value = new List<string>() });
                crawlItem.Content.Add(new CrawlerContent() { Name = "hostname", Value = "" });
                crawlItem.Content.Add(new CrawlerContent() { Name = "lastcrawled", Value = lastCrawledDate });
                lastCrawledDate

                if(cData.XmlConfiguration.Id > 0)
                {
                    var smartForm = new XmlParser(cData.Html);

                    var configItem = crawlConfig.crawlschemaitems.FirstOrDefault(c => c.xmlconfigid == cData.XmlConfiguration.Id);

                    if (configItem != null)
                    {
                        foreach (var fieldXPath in configItem.indexfields)
                        {
                            var value = HtmlParser.StripHTML(smartForm.ParseString(fieldXPath));
                            crawlItem.Content.Add(new CrawlerContent() { Name = "content", Value = value });
                        }

                        foreach (var fieldXPath in configItem.secondarycontent)
                        {
                            var contentIds = smartForm.ParseToList(fieldXPath);

                            foreach(var contentId in contentIds)
                            {
                                var secondaryCData = ContentApi.GetContentItem(contentId);

                                if(secondaryCData != null)
                                {
                                    var extractedContent = ExtractContent(secondaryCData);
                                    
                                    if (extractedContent.Any())
                                    {
                                        crawlItem.Content.AddRange(extractedContent);
                                    }
                                }

                            }
                        }
                    }
                }
                else
                {
                    var extractedContent = ExtractContent(cData);

                    if (extractedContent.Any())
                    {
                        crawlItem.Content.AddRange(extractedContent);
                    }
                }

                return crawlItem;
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("{0}", ex.Message));
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cData"></param>
        /// <returns></returns>
        private List<CrawlerContent> ExtractContent(ContentData cData)
        {
            var list = new List<CrawlerContent>();
                        
            switch(cData.ContType)
            {
                case 102:
                    var assetContent = ExtractAsset(cData);
                    list.Add(new CrawlerContent() { Name = "content", Value = assetContent });
                    break;

                default:
                    list.Add(new CrawlerContent() { Name = "content", Value = HtmlParser.StripHTML(cData.Html) });
                    break;
            }
            

            return list;
        }

        private string ExtractAsset(ContentData cData)
        {
            try
            {
                if (cData.AssetData == null)
                    return string.Empty;

                var assetPath = string.Format("{0}\\{1}\\{2}", AssetLibraryPath, cData.AssetData.Id, cData.AssetData.Version);

                var bytes = File.ReadAllBytes(assetPath);
                var result = SearchClient.FileExtract(bytes);

                return HtmlParser.StripHTML(result);
            }
            catch
            {
                return string.Empty;
            }
        }

        private Tuple<List<string>, List<string>> GetMetaData(long contentId)
        {
            var list = new List<string>();
            var mapList = new List<string>();

            var metadataList = MetadataApi.GetContentMetadataList(contentId);

            if (metadataList != null)
            {
                foreach (var metadata in metadataList.Where(m => !string.IsNullOrEmpty(m.Value.ToString())))
                {
                    list.Add(metadata.Value.ToString());
                    mapList.Add(string.Format("{0}/{1}", metadata.Name, metadata.Value));
                }
            }

            return new Tuple<List<string>, List<string>>(list, mapList);
        }

        private Tuple<List<string>, List<string>> GetTaxonomy(long contentId)
        {
            var list = new List<string>();
            var mapList = new List<string>();

            var taxonomyList = TaxonomyApi.ReadAllAssignedCategory(contentId);

            if (taxonomyList != null)
            {
                foreach (var taxonomy in taxonomyList)
                {
                    mapList.Add(string.Format("{0}", taxonomy.TaxonomyPath));
                    list.Add(string.Format("{0}", taxonomy.TaxonomyName));
                }
            }
            return new Tuple<List<string>, List<string>>(list, mapList);
        }

    }
}
