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
            var configPath = JsonUtil.ReadJsonFile(ConfigurationManager.AppSettings["CrawlConfigFile"]);
            return JsonUtil.Deserialize<CrawlSettings>(configPath);
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
