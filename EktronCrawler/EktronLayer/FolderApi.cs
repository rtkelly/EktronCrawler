using EktronCrawler.EktronWeb.FolderApi;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace EktronCrawler.EktronLayer
{
    public class FolderApi
    {
        private Folder FolderMgr; 

        public FolderApi()
        {
           FolderMgr = new Folder();
                       
           FolderMgr.Url = ConfigurationManager.AppSettings["EktronWeb_FolderApi_Folder"];
        }

        public FolderData Get(long folderId)
        {
            return FolderMgr.GetFolder(folderId);
        }

        public List<FolderData> GetChildFolders(long folderId, bool recursive=false)
        {
            var list = new List<FolderData>();

            var fData = FolderMgr.GetFolder(folderId);

            if (fData != null)
            {
                var childFolders = FolderMgr.GetChildFolders(fData.Id, false, FolderOrderBy.Name);

                if (childFolders != null)
                {
                    foreach (var childFolder in childFolders)
                    {
                        list.Add(childFolder);

                        var childSubFolders = GetChildFolders(childFolder.Id);

                        if (childSubFolders.Any())
                        {
                            list.AddRange(childSubFolders);
                        }
                    }
                }
            }

            return list;
        }
    }
}
