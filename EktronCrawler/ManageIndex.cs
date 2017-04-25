using MissionSearch;
using MissionSearch.Clients;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler
{
    public class ManageIndex<T> where T : ISearchDocument
    {
        ISearchClient<T> SrchClient { get; set; }

        public ManageIndex(ISearchClient<T> client)
        {
            SrchClient = client;
        }

        public List<long> GetFolderIdsFromIndex()
        {
            var response = SrchClient.GetTerms("folderid", null);

            return response.Select(str => TypeParser.ParseLong(str))
                .Distinct()
                .Where(folderid => folderid != 0)
                .OrderBy(folderid => folderid)
                .ToList();

            
        }

       

        public List<T> GetFolderItemsFromIndex(long folderid)
        {
            // to do: handle pagination

            var response = SrchClient.Search(new MissionSearch.SearchRequest()
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
