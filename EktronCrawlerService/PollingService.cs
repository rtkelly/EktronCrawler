using EktronCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EktronCrawlerService
{
    public class PollingService
    {
        private Thread _workerThread;
        private AutoResetEvent _finished;
        private const int _timeout = 60 * 1000;

        public void StartPolling()
        {
            _workerThread = new Thread(Poll);
            _finished = new AutoResetEvent(false);
            _workerThread.Start();
        }

        private void Poll()
        {
            while (!_finished.WaitOne(_timeout))
            {
                CrawlJobsHandler.ProcessJobs<SearchDocument>();
            }
        }

        public void StopPolling()
        {
            _finished.Set();
            _workerThread.Join();
        }
    }
}
