using EktronCrawler.EktronWeb.TaxonomyApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler.EktronLayer
{
    public class TaxonomyApi
    {
        Taxonomy TaxonomyMgr = new Taxonomy();

        public TaxonomyApi()
        {
            TaxonomyMgr.Url = ConfigurationManager.AppSettings["EktronWeb_TaxonomyApi_Taxonomy"];
        }


        public TaxonomyBaseData[] ReadAllAssignedCategory(long contentId)
        {
            return TaxonomyMgr.ReadAllAssignedCategory(contentId);
        }
    }
}
