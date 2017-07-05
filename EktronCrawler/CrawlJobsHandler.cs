using System;
using System.Linq;

namespace EktronCrawler
{
    public class CrawlJobsHandler
    {
        /// <summary>
        /// 
        /// </summary>
        public static void ProcessJobs()
        {
            var crawler = new ContentCrawler();

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
        /// <param name="lastRun"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        private static DateTime ComputeNextRunDate(DateTime lastRun, CrawlJob job)
        {
          
            switch(job.crawlintervaltype)
            {
                case CrawlIntervalTypes.Minute:
                    return lastRun.AddMinutes(job.crawlinterval);
                case CrawlIntervalTypes.Hour:
                    return lastRun.AddHours(job.crawlinterval);
                
                case CrawlIntervalTypes.Week:
                    return lastRun.AddDays(7 * job.crawlinterval).CustomAddTime(job.crawltime);

                case CrawlIntervalTypes.Month:
                    return lastRun.AddMonths(job.crawlinterval).CustomAddTime(job.crawltime);

                default:
                    return lastRun.AddDays(job.crawlinterval).CustomAddTime(job.crawltime);

            }
        }

          

    }
}
