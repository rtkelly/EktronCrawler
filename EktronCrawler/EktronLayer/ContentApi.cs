using EktronCrawler.EktronWeb.ContentApi;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler.EktronLayer
{
    public class ContentApi
    {
        Content ContentMgr = new Content();

        /// <summary>
        /// 
        /// </summary>
        public ContentApi()
        {
            ContentMgr.Url = ConfigurationManager.AppSettings["EktronWeb_ContentApi_Content"];
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ContentId"></param>
        /// <returns></returns>
        public ContentData GetContentItem(long contentId)
        {
            return ContentMgr.GetContent(contentId, ContentResultType.Published);

        }

        public ContentData GetContentItem(string contentId)
        {
            var id = TypeParser.ParseLong(contentId);

            if (id == 0)
                return null;

            return ContentMgr.GetContent(id, ContentResultType.Published);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public List<ContentData> GetFolderContent(long folderId)
        {
            var content = new List<ContentData>();

            var childContent = ContentMgr.GetChildContent(folderId, false, "Name");

            if (childContent != null)
            {
                content = childContent.ToList();
            }
            
            return content;
        }
    }
}
