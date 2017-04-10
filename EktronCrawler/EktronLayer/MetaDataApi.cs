using EktronCrawler.EktronWeb.MetaDataApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler.EktronLayer
{
    public class MetaDataApi
    {
        Metadata MetadataMgr = new Metadata();

        public MetaDataApi()
        {
            MetadataMgr.Url = ConfigurationManager.AppSettings["EktronWeb_MetaDataApi_Metadata"];
        }

        public CustomAttribute[] GetContentMetadataList(long contentId)
        {
            return MetadataMgr.GetContentMetadataList(contentId);
        }
    }
}
