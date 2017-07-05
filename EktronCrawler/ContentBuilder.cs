using EktronCrawler.EktronLayer;
using EktronCrawler.EktronWeb.ContentApi;
using MissionSearch;
using MissionSearch.Clients;
using MissionSearch.Indexers;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EktronCrawler.EktronWeb.MetaDataApi;
using System.Text.RegularExpressions;

namespace EktronCrawler
{
    public class ContentBuilder
    {
        private ISearchClient SearchClient;

        private ILogger Logger;

        private string AssetLibraryPath;

        private CrawlConfig _crawlConfig;

        AssetTransfer AssetTransferService { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        /// <param name="crawlConfig"></param>
        public ContentBuilder(ISearchClient client, ILogger logger, CrawlConfig crawlConfig)
        {
            SearchClient = client;
            
            Logger = logger;

            _crawlConfig = crawlConfig;
            
            AssetLibraryPath = _crawlConfig.assetlibrarypath;

            AssetTransferService = new AssetTransfer(_crawlConfig.assettransferservice);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cData"></param>
        /// <returns></returns>
        public ContentCrawlParameters BuildCrawlContentItem(ContentData cData)
        {
            var quickLink = EktronSQL.GetQuickLink(cData.Id);
            var lastCrawledDate = DateTime.Now;
            var defaultSchema = _crawlConfig.crawlschemaitems.FirstOrDefault(c => c.defaultschema);

            Logger.Info(string.Format("Processing content item \"{0}\"", cData.Title));
            Logger.Debug(string.Format("contentid:{0}", cData.Id));
            Logger.Debug(string.Format("contenttype:{0}", cData.ContType));
            Logger.Debug(string.Format("xmlconfigid:{0}", cData.XmlConfiguration.Id));
            Logger.Debug(string.Format("language:{0}", cData.LanguageId));
            Logger.Debug(string.Format("quicklink:{0}", quickLink));

            var crawlItem = new ContentCrawlParameters
            {
                ContentItem = new SearchableContentItem()
            };
                
            crawlItem.ContentItem._ContentID = string.Format("{0}|{1}", cData.Id, cData.LanguageId);

            var language = new List<string>() {cData.LanguageId.ToString()};

            crawlItem.Content.Add(new CrawlerContent() { Name = "contentid", Value = cData.Id.ToString() });
            crawlItem.Content.Add(new CrawlerContent() { Name = "url", Value = quickLink });
            crawlItem.Content.Add(new CrawlerContent() { Name = "title", Value = cData.Title });
            crawlItem.Content.Add(new CrawlerContent() { Name = "summary", Value = HtmlParser.StripHTML(cData.Teaser) });
            crawlItem.Content.Add(new CrawlerContent() { Name = "xmlconfigid", Value = cData.XmlConfiguration.Id });
            crawlItem.Content.Add(new CrawlerContent() { Name = "folderid", Value = cData.FolderId });
            crawlItem.Content.Add(new CrawlerContent() { Name = "contenttypeid", Value = cData.ContType });
            crawlItem.Content.Add(new CrawlerContent() { Name = "timestamp", Value = cData.DateCreated });
            crawlItem.Content.Add(new CrawlerContent() { Name = "publisheddate", Value = cData.DateModified });
            //crawlItem.Content.Add(new CrawlerContent() { Name = "path", Value = cData.Path ?? "" });
            crawlItem.Content.Add(new CrawlerContent() { Name = "folder", Value = cData.FolderName });
            crawlItem.Content.Add(new CrawlerContent() { Name = "language", Value = language });
            crawlItem.Content.Add(new CrawlerContent() { Name = "mimetype", Value = "" });
            //crawlItem.Content.Add(new CrawlerContent() { Name = "categories", Value = new List<string>() });
            crawlItem.Content.Add(new CrawlerContent() { Name = "pagetype", Value = "" });
            //crawlItem.Content.Add(new CrawlerContent() { Name = "paths", Value = new List<string>() });
            crawlItem.Content.Add(new CrawlerContent() { Name = "hostname", Value = "" });
            crawlItem.Content.Add(new CrawlerContent() { Name = "lastcrawled", Value = lastCrawledDate });

            ProcessMetadata(cData, defaultSchema, crawlItem);
            ProcessTaxonomy(cData, crawlItem);
            
            if(cData.XmlConfiguration.Id > 0)
            {
                ProcessSmartForm(cData, crawlItem);
            }
            else
            {
                ProcessMainContent(cData, crawlItem);
            }

            return crawlItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="crawlItem"></param>
        /// <returns></returns>
        private void ProcessSmartForm(ContentData cData, ContentCrawlParameters crawlItem)
        {
            Logger.Debug(string.Format("Processing Smartform Id: {0}", cData.XmlConfiguration.Id));

            var configItem = _crawlConfig.crawlschemaitems.FirstOrDefault(c => c.xmlconfigid == cData.XmlConfiguration.Id);

            if (configItem == null)
            {
                crawlItem.Content.Add(new CrawlerContent() {Name = "content", Value = HtmlParser.StripHTML(cData.Html)});
                return;
            }

            var content = new List<CrawlerContent>();

            var smartForm = new XmlParser(cData.Html);

            if (configItem.indexfields != null)
            {
                foreach (var fieldXPath in configItem.indexfields.Where(p => p.StartsWith("/root/")))
                {
                    var value = HtmlParser.StripHTML(smartForm.ParseString(fieldXPath));
                    content.Add(new CrawlerContent() { Name = "content", Value = value });
                }
            }

            if (configItem.storedfields != null)
            {
                foreach (var fieldXPath in configItem.storedfields.Where(p => p.StartsWith("/root/")))
                {
                    if (fieldXPath.Contains('~'))
                    {
                        var split = fieldXPath.Split('~');
                                                
                        var ftype = split[1];
                        var xpath = split[0];

                        var strValue = HtmlParser.StripHTML(smartForm.ParseString(xpath));
                        var fieldName = xpath.Split('/').Last().ToLower();

                        switch(ftype)
                        {
                            case "int":
                                                                
                                if(Regex.IsMatch(strValue, @"^\d*$"))
                                {
                                    var intValue = TypeParser.ParseInt(strValue);
                                    content.Add(new CrawlerContent() { Name = fieldName, Value = intValue });
                                }
                                
                                break;

                            case "datetime":

                                    var dateValue = TypeParser.ParseDateTime(strValue);
                                    content.Add(new CrawlerContent() { Name = fieldName, Value = dateValue });
                            
                                break;

                            default:
                                content.Add(new CrawlerContent() { Name = fieldName, Value = strValue });
                                break;
                        }
                        
                    }
                    else
                    {
                        var value = HtmlParser.StripHTML(smartForm.ParseString(fieldXPath));
                        var fieldName = fieldXPath.Split('/').Last().ToLower();

                        content.Add(new CrawlerContent() { Name = fieldName, Value = value });
                    }
                }
            }

            if (configItem.secondarycontent != null)
            {
                foreach (var fieldXPath in configItem.secondarycontent)
                {
                    var contentIds = smartForm.ParseToList(fieldXPath);

                    foreach (var contentId in contentIds)
                    {
                        var secondaryCData = EktronSQL.GetContentItem(contentId);

                        if (secondaryCData != null)
                        {
                            ProcessMainContent(secondaryCData, crawlItem);
                        }

                    }
                }
            }

            if(content.Any())
            {
                crawlItem.Content.AddRange(content);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="crawlItem"></param>
        /// <returns></returns>
        private void ProcessMainContent(ContentData cData, ContentCrawlParameters crawlItem)
        {
            switch(cData.ContType)
            {
                case 101: // office
                case 102: // pdf
                    var assetContent = ExtractAsset(cData);
                    crawlItem.Content.Add(new CrawlerContent() { Name = "content", Value = assetContent });
                    break;

                default:
                    crawlItem.Content.Add(new CrawlerContent() { Name = "content", Value = HtmlParser.StripHTML(cData.Html) });
                    break;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cData"></param>
        /// <returns></returns>
        private string ExtractAsset(ContentData cData)
        {
            var assetPath = string.Format("{0}\\{1}\\{2}", AssetLibraryPath, cData.AssetData.Id, cData.AssetData.Version);

            Logger.Debug("Calling Asset Transfer Service");
            Logger.Debug(string.Format("Asset Path: {0}", assetPath));

            //var bytes = File.ReadAllBytes(assetPath);
            var asset = AssetTransferService.GetAsset(assetPath);
                        
            Logger.Debug(string.Format("Asset Success: {0}", asset.Success));
            Logger.Debug(string.Format("Asset Status: {0}", asset.Status));
            Logger.Debug(string.Format("Asset Size: {0}", asset.Size));

            if(!asset.Success)
            {
                throw new Exception(string.Format("Error Asset Not Found: {0}", assetPath));

            }

            return ExtractAsset(asset.AssetData);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string ExtractAsset(byte[] bytes)
        {
            var responseXml = SearchClient.FileExtract(bytes);

            var xmlParser = new XmlParser(responseXml);
            var xhtml = xmlParser.ParseHTML("/response/str");
            var htmlParser = new HtmlParser(WebUtility.HtmlDecode(xhtml));

            return htmlParser.ParseStripInnerHtml("//body");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="defaultSchema"></param>
        /// <param name="crawlItem"></param>
        private void ProcessMetadata(ContentData cData, CrawlSchemaItem defaultSchema, ContentCrawlParameters crawlItem)
        {
            var contentMetadataList = EktronSQL.GetMetadata(cData.Id, cData.LanguageId);

            if (defaultSchema != null && defaultSchema.metadata_mapping != null)
            {
                foreach (var map in defaultSchema.metadata_mapping)
                {
                    var metaData = contentMetadataList.FirstOrDefault(m => m.Name == map.metadataname);

                    if (metaData != null)
                    {
                        if (metaData.ValueType == CustomAttributeValueTypes.Date)
                        {
                            var date = TypeParser.ParseDateTime(metaData.Value.ToString());
                            
                            if(date == null)
                                date = DateTime.MinValue;

                            crawlItem.Content.Add(new CrawlerContent() { Name = map.searchfieldname, Value = date.Value });    
                        }
                        else
                        {
                            crawlItem.Content.Add(new CrawlerContent() { Name = map.searchfieldname, Value = metaData.Value });    
                        }
                        
                    }
                }
            }

            var metadataLists = BuildMetaDataProperties(contentMetadataList, defaultSchema);

            if (metadataLists.Item1 != null && metadataLists.Item2 != null)
            {
                crawlItem.Content.Add(new CrawlerContent() { Name = "metadata", Value = metadataLists.Item1 });
                crawlItem.Content.Add(new CrawlerContent() { Name = "metadata_map", Value = metadataLists.Item2 });
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="metadataList"></param>
        /// <param name="defaultConfigItem"></param>
        /// <returns></returns>
        private Tuple<List<string>, List<string>> BuildMetaDataProperties(List<CustomAttribute> metadataList, CrawlSchemaItem defaultConfigItem)
        {
            Logger.Debug("Building Metadata");

            var list = new List<string>();
            var mapList = new List<string>();

            if (defaultConfigItem != null && defaultConfigItem.metadata != null)
            {
                if (metadataList != null)
                {
                    foreach (var metadata in metadataList.Where(m => defaultConfigItem.metadata.Contains(m.Name)))
                    {
                        list.Add(metadata.Value.ToString());
                        mapList.Add(string.Format("{0}/{1}", metadata.Name, metadata.Value));
                    }
                }
            }

            return new Tuple<List<string>, List<string>>(list, mapList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="crawlItem"></param>
        private void ProcessTaxonomy(ContentData cData, ContentCrawlParameters crawlItem)
        {
            var taxonomyLists = GetTaxonomy(cData.Id);

            if (taxonomyLists.Item1 != null)
            {
                crawlItem.Content.Add(new CrawlerContent() { Name = "taxonomy", Value = taxonomyLists.Item1 });
                crawlItem.Content.Add(new CrawlerContent() { Name = "taxonomy_map", Value = taxonomyLists.Item2 });
            }    
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        private Tuple<List<string>, List<string>> GetTaxonomy(long contentId)
        {
            Logger.Debug("Building taxonomy");

            var list = new List<string>();
            var mapList = new List<string>();

            //var taxonomyList = TaxonomyMgr.ReadAllAssignedCategory(contentId);
            var taxonomyList = EktronSQL.GetTaxonomy(contentId);

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
