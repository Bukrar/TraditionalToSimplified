using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace TraditionalToSimplified
{
    public class Db_Event_Tw_Service
    {
        Utility utility = new Utility();
        public void UpdateTableMy_Event_Category_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Event_Category_Tw> modelList = new List<Model.My_Event_Category_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:1:tables:my_event_category_tw:0").Value
                                               + "," + configuration.GetSection("db:1:tables:my_event_category_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:1:dbname").Value + "." +
                                               configuration.GetSection("db:1:tables:my_event_category_tw").Key, mySqlConnection))
                {
                    MySqlDataReader myData = mySqlCommand.ExecuteReader();
                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {
                        while (myData.Read())
                        {
                            if (!myData.IsDBNull(1))
                            {
                                string pk = myData.GetString(0);
                                string traditionalContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                                string simplifiedContent = utility.ToSimplified(traditionalContent, "ToSimplified");
                                string simplifiedEncodeContent = HttpUtility.UrlEncode(simplifiedContent, Encoding.GetEncoding("gb18030"));

                                Model.My_Event_Category_Tw modelData = new Model.My_Event_Category_Tw();
                                modelData.Code = pk;
                                modelData.Name = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_category_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_event_tw.my_event_category_tw set Name='" + s.Name +
                                         "' WHERE Code ='" + s.Code + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        // Console.WriteLine("處理KEY: " + s.Category_ID + " : " + s.Category_Name_TW);
                    }
                }
                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_category_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_My_Event_Press_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Event_Press_Tw> modelList = new List<Model.My_Event_Press_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:1:tables:my_event_press_tw:0").Value
                                               + "," + configuration.GetSection("db:1:tables:my_event_press_tw:1").Value
                                               + "," + configuration.GetSection("db:1:tables:my_event_press_tw:2").Value
                                               + " FROM " + configuration.GetSection("db:1:dbname").Value + "." +
                                               configuration.GetSection("db:1:tables:my_event_press_tw").Key, mySqlConnection))
                {
                    MySqlDataReader myData = mySqlCommand.ExecuteReader();
                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {
                        while (myData.Read())
                        {
                            string pk = myData.GetString(0);
                            string traditionalTitleContent, simplifiedTitleContent, simplifiedEncodeTitleContent = null;
                            string traditionalContent, simplifiedContent, simplifiedEncodeContent = null;
                            if (!myData.IsDBNull(1))
                            {
                                traditionalTitleContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                                simplifiedTitleContent = utility.ToSimplified(traditionalTitleContent, "ToSimplified");
                                simplifiedEncodeTitleContent = HttpUtility.UrlEncode(simplifiedTitleContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_press_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                            if (!myData.IsDBNull(2))
                            {
                                traditionalContent = HttpUtility.UrlDecode(myData.GetString(2), Encoding.GetEncoding("big5"));
                                simplifiedContent = utility.ToSimplified(traditionalContent, "ToSimplified");
                                simplifiedEncodeContent = HttpUtility.UrlEncode(simplifiedContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_press_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                            Model.My_Event_Press_Tw modelData = new Model.My_Event_Press_Tw();
                            modelData.Press_SEQ = pk;
                            modelData.Press_Title = simplifiedEncodeTitleContent;
                            modelData.Press_Content = simplifiedEncodeContent;
                            modelList.Add(modelData);
                        }
                    }
                }
                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_event_tw.my_event_press_tw set Press_Title='" + s.Press_Title
                                         + "', Press_Content='" + s.Press_Content +
                                         "' WHERE Press_SEQ ='" + s.Press_SEQ + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        // Console.WriteLine("處理KEY: " + s.Category_ID + " : " + s.Category_Name_TW);
                    }
                }
                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_press_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Event_Region_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Event_Region_Tw> modelList = new List<Model.My_Event_Region_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:1:tables:my_event_region_tw:0").Value
                                               + "," + configuration.GetSection("db:1:tables:my_event_region_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:1:dbname").Value + "." +
                                               configuration.GetSection("db:1:tables:my_event_region_tw").Key, mySqlConnection))
                {
                    MySqlDataReader myData = mySqlCommand.ExecuteReader();
                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {
                        while (myData.Read())
                        {
                            if (!myData.IsDBNull(1))
                            {
                                string pk = myData.GetString(0);
                                string traditionalContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                                string simplifiedContent = utility.ToSimplified(traditionalContent, "ToSimplified");
                                string simplifiedEncodeContent = HttpUtility.UrlEncode(simplifiedContent, Encoding.GetEncoding("gb18030"));

                                Model.My_Event_Region_Tw modelData = new Model.My_Event_Region_Tw();
                                modelData.Code = pk;
                                modelData.Name = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_region_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_event_tw.my_event_region_tw set Name='" + s.Name +
                                         "' WHERE Code ='" + s.Code + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        // Console.WriteLine("處理KEY: " + s.Category_ID + " : " + s.Category_Name_TW);
                    }
                }
                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_region_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Partnar_Category_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Partnar_Category_Tw> modelList = new List<Model.My_Partnar_Category_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:1:tables:my_partnar_category_tw:0").Value
                                               + "," + configuration.GetSection("db:1:tables:my_partnar_category_tw:1").Value
                                               + "," + configuration.GetSection("db:1:tables:my_partnar_category_tw:2").Value
                                               + " FROM " + configuration.GetSection("db:1:dbname").Value + "." +
                                               configuration.GetSection("db:1:tables:my_partnar_category_tw").Key, mySqlConnection))
                {
                    MySqlDataReader myData = mySqlCommand.ExecuteReader();
                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {
                        while (myData.Read())
                        {
                            string pk = myData.GetString(0);
                            string traditionalNameContent, simplifiedNameContent, simplifiedEncodeNameContent = null;
                            string traditionalTitleContent, simplifiedTitleContent, simplifiedEncodeTitleContent = null;
                            if (!myData.IsDBNull(1))
                            {
                                traditionalNameContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                                simplifiedNameContent = utility.ToSimplified(traditionalNameContent, "ToSimplified");
                                simplifiedEncodeNameContent = HttpUtility.UrlEncode(simplifiedNameContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_partnar_category_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                            if (myData.IsDBNull(2))
                            {
                                traditionalTitleContent = HttpUtility.UrlDecode(myData.GetString(2), Encoding.GetEncoding("big5"));
                                simplifiedTitleContent = utility.ToSimplified(traditionalTitleContent, "ToSimplified");
                                simplifiedEncodeTitleContent = HttpUtility.UrlEncode(simplifiedTitleContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_partnar_category_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                            Model.My_Partnar_Category_Tw modelData = new Model.My_Partnar_Category_Tw();
                            modelData.Code = pk;
                            modelData.Name = simplifiedEncodeNameContent;
                            modelData.Title = simplifiedEncodeTitleContent;
                            modelList.Add(modelData);
                        }
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_event_tw.my_partnar_category_tw set Name='" + s.Name +
                                         "', Title='" + s.Title +
                                         "' WHERE Code ='" + s.Code + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        // Console.WriteLine("處理KEY: " + s.Category_ID + " : " + s.Category_Name_TW);
                    }
                }
                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_partnar_category_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }
    }
}
