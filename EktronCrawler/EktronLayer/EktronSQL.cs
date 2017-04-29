using EktronCrawler.EktronLayer;
using EktronCrawler.EktronWeb.ContentApi;
using EktronCrawler.EktronWeb.FolderApi;
using EktronCrawler.EktronWeb.MetaDataApi;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler
{
    public static class EktronSQL
    {
        public static string ConnectionString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<FolderData> GetFolders()
        {
            var sql = "SELECT * FROM [content_folder_tbl] where private_content = 0 and is_content_searchable = 1";

            var list = Read<FolderData>(sql, LoadFolderData)
                .Where(f => f.Name != "_meta_")
                .ToList();

            return list;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderid"></param>
        /// <returns></returns>
        public static FolderData GetFolder(long folderid)
        {
            var sql = string.Format("SELECT * FROM [content_folder_tbl] WHERE folder_id = {0}", folderid);

            var list = Read<FolderData>(sql, LoadFolderData);

            return list.FirstOrDefault();
            
        }

        public static ContentData GetContentItem(string contentid)
        {
            var sql = string.Format("SELECT * FROM [content] WHERE content_id = {0}", contentid);

            var list = Read<ContentData>(sql, LoadContentData);

            return list.FirstOrDefault();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderid"></param>
        /// <returns></returns>
        public static List<FolderData> GetSubFolders(long folderid)
        {
            var sql = string.Format("SELECT * FROM [content_folder_tbl] WHERE parent_id = {0} AND private_content = 0 AND folder_id <> 0", folderid);

            var list = Read<FolderData>(sql, LoadFolderData);

            return list.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderIdPath"></param>
        /// <returns></returns>
        public static List<FolderData> GetSubFolders(string folderIdPath)
        {
            var sql = string.Format("SELECT * FROM [content_folder_tbl] WHERE FolderIdPath like '{0}%'", folderIdPath);

            var list = Read<FolderData>(sql, LoadFolderData);

            return list.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static List<ContentData> GetContent(ContentRequest request)
        {
            var sql = string.Format("SELECT * FROM [content] WHERE content_status = 'A' AND searchable = 1 ");
            
            if(request.LastUpdated != null)
            {
                sql += string.Format("AND last_edit_date > '{0:MM/dd/yyyy hh:mm:ss tt}' ", request.LastUpdated.Value);
            }

            if (request.FolderIds != null && request.FolderIds.Any())
            {
                sql += string.Format("AND folder_id in ({0})", string.Join(",", request.FolderIds));
            }

            if (request.ContentTypes != null && request.ContentTypes.Any())
            {
                sql += string.Format("AND content_type in ({0})", string.Join(",", request.ContentTypes));
            }
            
            if (request.XmlConfigIds != null && request.XmlConfigIds.Any())
            {
              sql += string.Format("AND xml_config_id in ({0})", string.Join(",", request.XmlConfigIds));
            }

            var list = Read<ContentData>(sql, LoadContentData)
                    .Where(f => f != null)
                    .ToList();

            return list;
        }

        public static List<CustomAttribute> GetMetadata(long contentId)
        {
            var sql = string.Format("SELECT c.[meta_type_id],[content_id],[content_language],[meta_value],[meta_name],[active] FROM [content_meta_tbl] as c join [metadata_type] as m on m.meta_type_id = c.meta_type_id WHERE active = 1 AND content_id = {0}", contentId);

            var list = Read<CustomAttribute>(sql, LoadCustomAttribute);

            return list.ToList();
        }

        public static List<TaxonomyBaseData> GetTaxonomy(long contentId)
        {
            var sql = string.Format("SELECT t.[taxonomy_id],t.[taxonomy_language_id],[taxonomy_item_id],[taxonomy_item_language],[taxonomy_name], [taxonomy_path] FROM [taxonomy_item_tbl] as i join [taxonomy_tbl] as t on t.taxonomy_id = i.taxonomy_id Where t.taxonomy_language_id = 1033 AND taxonomy_item_id = {0}", contentId);

            var list = Read<TaxonomyBaseData>(sql, LoadTaxonomyBaseData);

            return list.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="make"></param>
        /// <returns></returns>
        public static IEnumerable<T> Read<T>(string sql, Func<IDataReader, T> make)
        {
            //string connString = ConfigurationManager.ConnectionStrings["Ektron.DbConnection"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return make(reader);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static TaxonomyBaseData LoadTaxonomyBaseData(IDataReader reader)
        {
            var id = reader.GetInt64(0);

            // t.[taxonomy_id],t.[taxonomy_language_id],[taxonomy_item_id],[taxonomy_item_language],[taxonomy_name], [taxonomy_path] 

            var cData = new TaxonomyBaseData()
            {
                TaxonomyId = reader.GetInt64(0),
                TaxonomyName = reader.GetString(4),
                TaxonomyPath = reader.GetString(5),
            };

            return cData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static CustomAttribute LoadCustomAttribute(IDataReader reader)
        {
            var id = reader.GetInt64(0);

            var cData = new CustomAttribute()
            {
                Id = reader.GetInt64(0),
                Value = reader.GetString(3),
                Name = reader.GetString(4),
            };

            return cData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static FolderData LoadFolderData(IDataReader reader)
        {
            var id = reader.GetInt64(0);

            var cData = new FolderData()
            {
                Id = id,
                Name = reader.GetString(2),
                FolderIdWithPath = reader.GetString(30),
            };

            return cData;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static ContentData LoadContentData(IDataReader reader)
        {
            try
            {
                var id = reader.GetInt64(0);

                var cData = new ContentData()
                {
                    Id = id,
                    LanguageId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    Teaser = reader.GetString(17),
                    Html = reader.GetString(3),
                    ContType = reader.GetInt32(22),
                    DateCreated = reader.GetDateTime(5),
                    DateModified = reader.GetDateTime(9),
                    FolderId = reader.GetInt64(11),
                    XmlConfiguration = new EktronWeb.ContentApi.XmlConfigData()
                    {
                        Id = reader.GetInt64(29),
                    },
                    AssetData = new AssetData()
                    {
                        Id = reader.GetString(27),
                        Version = reader.GetString(28),
                    },
                };

                return cData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
               

    }
}
