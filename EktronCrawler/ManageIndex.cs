using MissionSearch;
using MissionSearch.Clients;
using MissionSearch.Util;
using System.Collections.Generic;
using System.Linq;

namespace EktronCrawler
{
    public class ManageIndex
    {
        ISearchClient SrchClient { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public ManageIndex(ISearchClient client)
        {
            SrchClient = client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<long> GetFolderIdsFromIndex()
        {
            var response = SrchClient.GetTerms("folderid", null);

            return response.Select(str => TypeParser.ParseLong(str))
                .Distinct()
                .Where(folderid => folderid != 0)
                .OrderBy(folderid => folderid)
                .ToList();

            
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderid"></param>
        /// <returns></returns>
        public List<dynamic> GetFolderItemsFromIndex(long folderid)
        {
            // to do: handle pagination

            var response = SrchClient.Search(new SearchRequest()
            {
                QueryOptions = new List<MissionSearch.Search.Query.IQueryOption>()
                {
                    new FilterQuery("folderid", folderid.ToString())
                },
                PageSize = 10000,
                CurrentPage = 1,
            });

            return response.Results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void DeleteItem(long id)
        {
            try
            {
                SrchClient.Delete(string.Format("contentid:{0}", id));
            }
            catch
            {
                // ignore
            }
        }
    }
}
