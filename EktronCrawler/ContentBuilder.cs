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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EktronCrawler
{
    public class ContentBuilder<T> where T : ISearchDocument
    {
        MetaDataApi MetadataMgr = new MetaDataApi();
        TaxonomyApi TaxonomyMgr = new TaxonomyApi();

        EktronLayer.ContentApi ContentApi = new EktronLayer.ContentApi();
        ISearchClient<T> SearchClient { get; set; }
       
        ILogger Logger;

        string AssetLibraryPath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ContentBuilder(ISearchClient<T> client, ILogger logger)
        {
            SearchClient = client;
            Logger = logger;

            AssetLibraryPath = ConfigurationManager.AppSettings["AssetLibraryFolder"];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="crawlConfig"></param>
        /// <returns></returns>
        public ContentCrawlParameters BuildCrawlContentItem(ContentData cData, CrawlConfig crawlConfig)
        {
            try
            {
                var lastCrawledDate = DateTime.Now;

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
                crawlItem.Content.Add(new CrawlerContent() { Name = "folder", Value = cData.FolderName });
                crawlItem.Content.Add(new CrawlerContent() { Name = "language", Value = cData.LanguageId.ToString() });
                crawlItem.Content.Add(new CrawlerContent() { Name = "mimetype", Value = "" });
                crawlItem.Content.Add(new CrawlerContent() { Name = "categories", Value = new List<string>() });
                crawlItem.Content.Add(new CrawlerContent() { Name = "pagetype", Value = "" });
                crawlItem.Content.Add(new CrawlerContent() { Name = "paths", Value = new List<string>() });
                crawlItem.Content.Add(new CrawlerContent() { Name = "hostname", Value = "" });
                crawlItem.Content.Add(new CrawlerContent() { Name = "lastcrawled", Value = lastCrawledDate });
                
                if(cData.XmlConfiguration.Id > 0)
                {
                    var configItem = crawlConfig.crawlschemaitems.FirstOrDefault(c => c.xmlconfigid == cData.XmlConfiguration.Id);

                    if (configItem != null)
                    {
                        var crawlContent = ProcessSmartForm(cData, configItem);

                        if(crawlContent.Any())
                        {
                            crawlItem.Content.AddRange(crawlContent);
                        }
                    }
                    else
                    {
                       crawlItem.Content.Add(new CrawlerContent() { Name = "content", Value = HtmlParser.StripHTML(cData.Html) });
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
        /// <param name="configItem"></param>
        /// <returns></returns>
        private List<CrawlerContent> ProcessSmartForm(ContentData cData, CrawlSchemaItem configItem)
        {
            var crawlItems = new List<CrawlerContent>();

            var smartForm = new XmlParser(cData.Html);

            if (configItem.indexfields != null)
            {
                foreach (var fieldXPath in configItem.indexfields.Where(p => p.StartsWith("/root/")))
                {
                    var value = HtmlParser.StripHTML(smartForm.ParseString(fieldXPath));
                    crawlItems.Add(new CrawlerContent() { Name = "content", Value = value });
                }
            }

            if (configItem.storedfields != null)
            {
                foreach (var fieldXPath in configItem.storedfields.Where(p => p.StartsWith("/root/")))
                {
                    var value = HtmlParser.StripHTML(smartForm.ParseString(fieldXPath));
                    var fieldName = fieldXPath.Split('/').Last().ToLower();

                    crawlItems.Add(new CrawlerContent() { Name = fieldName, Value = value });
                }
            }

            if (configItem.secondarycontent != null)
            {
                foreach (var fieldXPath in configItem.secondarycontent)
                {
                    var contentIds = smartForm.ParseToList(fieldXPath);

                    foreach (var contentId in contentIds)
                    {
                        var secondaryCData = ContentApi.GetContentItem(contentId);

                        if (secondaryCData != null)
                        {
                            var extractedContent = ExtractContent(secondaryCData);

                            if (extractedContent.Any())
                            {
                                crawlItems.AddRange(extractedContent);
                            }
                        }

                    }
                }
            }

            return crawlItems;
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
                case 101: // office
                case 102: // pdf
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
                var assetPath = string.Format("{0}\\{1}\\{2}", AssetLibraryPath, cData.AssetData.Id, cData.AssetData.Version);
                //var bytes = File.ReadAllBytes(assetPath);
                var bytes = AssetTransfer.GetAsset(assetPath);

                return ExtractAsset(bytes);
            }
            catch(Exception ex)
            {
                return string.Empty;
            }
        }

        public string ExtractAsset(byte[] bytes)
        {
            try
            {
                var responseXml = SearchClient.FileExtract(bytes);

                var xmlParser = new XmlParser(responseXml);
                var xhtml = xmlParser.ParseHTML("/response/str");
                var htmlParser = new HtmlParser(WebUtility.HtmlDecode(xhtml));

                return htmlParser.ParseStripInnerHtml("//body");
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

            var metadataList = MetadataMgr.GetContentMetadataList(contentId);

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

            var taxonomyList = TaxonomyMgr.ReadAllAssignedCategory(contentId);

            if (taxonomyList != null)
            {
                foreach (var taxonomy in taxonomyList)
                {
                    mapList.Add(string.Format("{0}", taxonomy.TaxonomyPath.Replace("\\", "/")));
                    list.Add(string.Format("{0}", taxonomy.TaxonomyName));
                }
            }
            return new Tuple<List<string>, List<string>>(list, mapList);
        }

    }
}
