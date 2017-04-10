using EktronCrawler.EktronLayer;
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

        public static List<long> GetRecentContent(ContentRequest request)
        {
            var sql = string.Format("SELECT [content_id] FROM [content] WHERE content_status = 'A' ");
            
            if(request.LastUpdated != null)
            {
                sql += string.Format("AND last_edit_date > '{0:MM/dd/yyyy hh:mm:ss tt}' ", request.LastUpdated.Value);
            }

            if (request.FolderIds != null && request.FolderIds.Any())
            {
                sql += string.Format("AND folder_id in ({0})", string.Join(",", request.FolderIds));
            }

            if (request.XmlConfigIds != null && request.XmlConfigIds.Any())
            {
              sql += string.Format("AND xml_config_id in ({0})", string.Join(",", request.XmlConfigIds));
            }

            string connString = ConfigurationManager.ConnectionStrings["Ektron.DbConnection"].ConnectionString;

            var list = new List<long>();

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
                        try
                        {
                            while (reader.Read())
                            {
                                list.Add(reader.GetInt64(0));
                            }
                        }
                        catch
                        {

                        }
                    }

                    conn.Close();
                }
            }

            return list;
        }

    }
}
