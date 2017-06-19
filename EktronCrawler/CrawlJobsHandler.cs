using MissionSearch;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler
{
    public class CrawlJobsHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void ProcessJobs<T>() where T : ICMSSearchDocument
        {
            var crawler = new ContentCrawler<T>();

            var crawlSettings = CrawlerConfig.LoadCrawlSettings();
            var crawlStatus = CrawlerConfig.LoadCrawlStatus();

            var runCnt = 0;

            foreach(var job in crawlSettings.crawljobs)
            {
                var crawlStatusJob =  crawlStatus.crawlstatusjobs.FirstOrDefault(j => j.jobid == job.jobid);

                if(crawlStatusJob == null)
                {
                    crawlStatusJob = new CrawlStatusJob()
                    {
                        jobid = job.jobid,
                    };

                    crawlStatus.crawlstatusjobs.Add(crawlStatusJob);
                }

                var crawlConfig = crawlSettings.crawlconfigs.FirstOrDefault(c => c.configid == job.crawlconfigid);

                if(crawlConfig == null)
                    continue;

                var crawlSchema =  crawlSettings.crawlschemas.FirstOrDefault(c => c.crawlschemaid == job.crawlschemaid);

                if (crawlSchema == null)
                    continue;

                crawlConfig.crawlschemaitems = crawlSchema.crawlschemaitems;

                var lastrun = crawlStatusJob.lastrundate;
                var nextrun = ComputeNextRunDate(lastrun,job);
                    
                if (nextrun >= DateTime.Now)
                {
                    continue;
                }

                var results = crawler.RunJob(job, crawlConfig, lastrun);
                                
                crawlStatusJob.lastrundate = DateTime.Now;
                crawlStatusJob.totalcrawled = results.TotalCnt;
                crawlStatusJob.totalerrors = results.ErrorCnt;
                crawlStatusJob.Duration = results.Duration;

                runCnt++;
            }

            if (runCnt > 0)
            {
                CrawlerConfig.SaveCrawlStatus(crawlStatus);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private static DateTime ComputeNextRunDate(DateTime lastRun, CrawlJob job)
        {
          
            switch(job.crawlintervaltype)
            {
                case CrawlIntervalTypes.Minute:
                    return lastRun.AddMinutes(job.crawlinterval);
                    break;
                                    
                case CrawlIntervalTypes.Week:
                    return lastRun.AddDays(7 * job.crawlinterval).CustomAddTime(job.crawltime);
                    break;

                case CrawlIntervalTypes.Month:
                    return lastRun.AddMonths(job.crawlinterval).CustomAddTime(job.crawltime);
                    break;

                default:
                case CrawlIntervalTypes.Day:
                    return lastRun.AddDays(job.crawlinterval).CustomAddTime(job.crawltime);
                    break;

            }
        }

          

    }
}
