using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace TraditionalToSimplified
{
    public class Db_Tw_Service
    {
        Utility utility = new Utility();
        public void UpdateTableMy_Category_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Category_Tw> modelList = new List<Model.My_Category_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_category_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_category_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_category_tw").Key, mySqlConnection))
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

                                Model.My_Category_Tw modelData = new Model.My_Category_Tw();
                                modelData.Category_ID = pk;
                                modelData.Category_Name_TW = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_category_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }

                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_tw.my_category_tw set Category_Name_TW='" + s.Category_Name_TW +
                                         "' WHERE Category_ID ='" + s.Category_ID + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        // Console.WriteLine("處理KEY: " + s.Category_ID + " : " + s.Category_Name_TW);
                    }
                }
                Console.WriteLine("DB:DB_TW TABLE: my_category_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Country_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            //建構子
            List<Model.My_Country_Tw> modelList = new List<Model.My_Country_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_country_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_country_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_country_tw").Key, mySqlConnection))
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

                                Model.My_Country_Tw modelData = new Model.My_Country_Tw();
                                modelData.Country_ID = pk;
                                modelData.Country_Name_TW = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_country_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }

                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_tw.my_country_tw set Country_Name_TW='" + s.Country_Name_TW +
                                         "' WHERE Country_ID ='" + s.Country_ID + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        //Console.WriteLine("處理KEY: " + s.Country_ID + " : " + s.Country_Name_TW);
                    }
                }
                Console.WriteLine("DB:DB_TW TABLE: my_country_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Press_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Press_Tw> modelList = new List<Model.My_Press_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_press_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_press_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_press_tw").Key, mySqlConnection))
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

                                Model.My_Press_Tw modelData = new Model.My_Press_Tw();
                                modelData.Press_SEQ = pk;
                                modelData.Press_Title = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_press_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }

                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_tw.my_press_tw set Press_Title='" + s.Press_Title +
                                         "' WHERE Press_SEQ ='" + s.Press_SEQ + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        // Console.WriteLine("處理KEY: " + s.Press_SEQ + " : " + s.Press_Title);
                    }
                }
                Console.WriteLine("DB:DB_TW TABLE: my_press_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Product_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Product_Tw> modelList = new List<Model.My_Product_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_product_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_product_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_product_tw").Key, mySqlConnection))
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

                                Model.My_Product_Tw modelData = new Model.My_Product_Tw();
                                modelData.Prod_ID = pk;
                                modelData.Prod_Title_TW = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_product_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }

                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_tw.my_product_tw set Prod_Title_TW='" + s.Prod_Title_TW +
                                         "' WHERE Prod_ID ='" + s.Prod_ID + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        // Console.WriteLine("處理KEY: " + s.Prod_ID + " : " + s.Prod_Title_TW);
                    }
                }
                Console.WriteLine("DB:DB_TW TABLE: my_product_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Publisher_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Publisher_Tw> modelList = new List<Model.My_Publisher_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_publisher_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_publisher_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_publisher_tw").Key, mySqlConnection))
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

                                Model.My_Publisher_Tw modelData = new Model.My_Publisher_Tw();
                                modelData.Pub_ID = pk;
                                modelData.Pub_Intro_TW = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_publisher_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }

                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_tw.my_publisher_tw set Pub_Intro_TW='" + s.Pub_Intro_TW +
                                         "' WHERE Pub_ID ='" + s.Pub_ID + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        //Console.WriteLine("處理KEY: " + s.Pub_ID + " : " + s.Pub_Intro_TW);
                    }
                }
                Console.WriteLine("DB:DB_TW TABLE: my_publisher_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Region_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Region_Tw> modelList = new List<Model.My_Region_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_region_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_region_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_region_tw").Key, mySqlConnection))
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

                                Model.My_Region_Tw modelData = new Model.My_Region_Tw();
                                modelData.Region_ID = pk;
                                modelData.Region_Name_TW = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_region_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }

                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_tw.my_region_tw set Region_Name_TW='" + s.Region_Name_TW +
                                         "' WHERE Region_ID ='" + s.Region_ID + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        //Console.WriteLine("處理KEY: " + s.Region_ID + " : " + s.Region_Name_TW);
                    }
                }
                Console.WriteLine("DB:DB_TW TABLE: my_region_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Subtopic_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Subtopic_Tw> modelList = new List<Model.My_Subtopic_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_subtopic_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_subtopic_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_subtopic_tw").Key, mySqlConnection))
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

                                Model.My_Subtopic_Tw modelData = new Model.My_Subtopic_Tw();
                                modelData.SubTopic_ID = pk;
                                modelData.SubTopic_Name_TW = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_subtopic_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }

                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_tw.my_subtopic_tw set SubTopic_Name_TW='" + s.SubTopic_Name_TW +
                                         "' WHERE SubTopic_ID ='" + s.SubTopic_ID + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        //Console.WriteLine("處理KEY: " + s.SubTopic_ID + " : " + s.SubTopic_Name_TW);
                    }
                }
                Console.WriteLine("DB:DB_TW TABLE: my_subtopic_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Topic_Tw(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Topic_Tw> modelList = new List<Model.My_Topic_Tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_topic_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_topic_tw:1").Value
                                               + "," + configuration.GetSection("db:0:tables:my_topic_tw:2").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_topic_tw").Key, mySqlConnection))
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
                            string traditionalIntroContent, simplifiedIntroContent, simplifiedEncodeIntroContent = null;
                            string traditionalNameContent, simplifiedNameContent, simplifiedEncodeNameContent = null;
                            if (!myData.IsDBNull(1))
                            {
                                traditionalIntroContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                                simplifiedIntroContent = utility.ToSimplified(traditionalIntroContent, "ToSimplified");
                                simplifiedEncodeIntroContent = HttpUtility.UrlEncode(simplifiedIntroContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_topic_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }

                            if (myData.IsDBNull(1))
                            {
                                traditionalNameContent = HttpUtility.UrlDecode(myData.GetString(2), Encoding.GetEncoding("big5"));
                                simplifiedNameContent = utility.ToSimplified(traditionalNameContent, "ToSimplified");
                                simplifiedEncodeNameContent = HttpUtility.UrlEncode(simplifiedNameContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_topic_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                            Model.My_Topic_Tw modelData = new Model.My_Topic_Tw();
                            modelData.Topic_ID = pk;
                            modelData.Topic_Intro_TW = simplifiedEncodeIntroContent;
                            modelData.Topic_Name_TW = simplifiedEncodeNameContent;
                            modelList.Add(modelData); 
                           
                          
                        }
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_tw.my_topic_tw set Topic_Intro_TW='" + s.Topic_Intro_TW
                                        + "', Topic_Name_TW='" + s.Topic_Name_TW +
                                         "' WHERE Topic_ID ='" + s.Topic_ID + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        // Console.WriteLine("處理KEY: " + s.Topic_ID + " : " + s.Topic_Intro_TW);
                    }
                }
                Console.WriteLine("DB:DB_TW TABLE: my_subtopic_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }
    }
}
