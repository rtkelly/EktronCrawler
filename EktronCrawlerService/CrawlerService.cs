 using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawlerService
{
    public partial class CrawlerService : ServiceBase
    {
        private readonly PollingService _pollingService = new PollingService();

        public CrawlerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _pollingService.StartPolling();
        }

        protected override void OnStop()
        {
            _pollingService.StopPolling();
        }
    }
}
