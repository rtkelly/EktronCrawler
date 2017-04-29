using MissionSearch.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler
{
    public class CrawlerConfig
    {

        public static CrawlSettings LoadCrawlSettings()
        {
            var configPath = ConfigurationManager.AppSettings["CrawlConfigsPath"];
                        
            var masterSettings = new CrawlSettings();

            masterSettings.crawlconfigs = new List<CrawlConfig>();
            masterSettings.crawljobs = new List<CrawlJob>();

            foreach (string file in Directory.EnumerateFiles(configPath, "*.json"))
            {
                string json = JsonUtil.ReadJsonFile(file);

                var settings = JsonUtil.Deserialize<CrawlSettings>(json);

                if(settings != null)
                {
                    if (settings.crawlconfigs != null && settings.crawlconfigs.Any())
                        masterSettings.crawlconfigs.AddRange(settings.crawlconfigs);

                    if (settings.crawljobs != null && settings.crawljobs.Any())
                        masterSettings.crawljobs.AddRange(settings.crawljobs);
                }
            }


            return masterSettings;
        }

        public static CrawlStatus LoadCrawlStatus()
        {
            try
            {
                var configPath = JsonUtil.ReadJsonFile(ConfigurationManager.AppSettings["CrawlStatusFile"]);
                return JsonUtil.Deserialize<CrawlStatus>(configPath);
            }
            catch
            {
                return new CrawlStatus();
            }
        }


        public static void SaveCrawlStatus(CrawlStatus data)
        {
            var path = ConfigurationManager.AppSettings["CrawlStatusFile"];
                        
            var json = JsonConvert.SerializeObject(data);

            using (var r = new StreamWriter(path))
            {
                r.Write(json);
            }
        }
    }
}
