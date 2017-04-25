using EktronCrawler.EktronLayer;
using EktronCrawler.EktronWeb.ContentApi;
using EktronCrawler.EktronWeb.FolderApi;
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
        public static List<FolderData> GetFolders()
        {
            string connString = ConfigurationManager.ConnectionStrings["Ektron.DbConnection"].ConnectionString;

            var list = new List<FolderData>();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = 0;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT [folder_id],[folder_name],[FolderPath],[FolderIdPath],[site_id] FROM [content_folder_tbl] where private_content = 0 and is_content_searchable = 1";

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var folderName = reader.GetString(1);
                        
                        if (folderName == "_meta_")
                            continue;

                        var folder = new FolderData()
                        {
                            Id = reader.GetInt64(0),
                            Name = folderName,
                            NameWithPath = reader.GetString(2),
                            FolderIdWithPath = reader.GetString(3),
                        };

                        list.Add(folder);
                    }
                }
            }

            return list;
        }

        public static FolderData GetFolder(long folderid)
        {
            var sql = string.Format("SELECT * FROM [content_folder_tbl] WHERE folder_id = {0}", folderid);

            var connString = ConfigurationManager.ConnectionStrings["Ektron.DbConnection"].ConnectionString;

           
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 0;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
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

                    }
                }
            }
            
            return null;
        }

        public static FolderData GetFolder(long folderid)
        {
            var sql = string.Format("SELECT * FROM [content_folder_tbl] WHERE folder_id = {0}", folderid);

            var connString = ConfigurationManager.ConnectionStrings["Ektron.DbConnection"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;

                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {

                            while (reader.Read())
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

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

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

            string connString = ConfigurationManager.ConnectionStrings["Ektron.DbConnection"].ConnectionString;

            //var list = new List<long>();
            var listData = new List<ContentData>();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 0;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                       
                        while (reader.Read())
                        {
                            var id = reader.GetInt64(0);

                            //list.Add(id);

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

                            listData.Add(cData);
                        }
                    }
                }
            }

            return listData;
        }

    }
}
