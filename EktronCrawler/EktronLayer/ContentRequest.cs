using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler.EktronLayer
{
    public class ContentRequest
    {
        public DateTime? LastUpdated { get; set; }

        public IEnumerable<long> FolderIds { get; set; }

        public IEnumerable<long> XmlConfigIds { get; set; }

        public IEnumerable<long> ContentTypes { get; set; }
        
    }
}
