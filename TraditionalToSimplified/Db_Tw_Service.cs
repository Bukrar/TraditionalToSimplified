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
        public void UpdateTableMy_Category_Tw()
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Category_Tw> modelList = new List<Model.My_Category_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:0:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }

            //DB資料DECODE 繁體=>簡體 再把簡體ENCODE
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_category_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_category_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_category_tw").Key, Db_Tw_SqlConnection))
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
                Db_Tw_SqlConnection.Close();

                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:2:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }
                //修改簡體資料庫              
                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_category_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_category_tw:1").Value +
                                         "='" + s.Category_Name_TW +
                                         "' WHERE " + configuration.GetSection("db:2:tables:my_category_tw:0").Value +
                                         "='" + s.Category_ID + "'", Db_Cn_SqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB:DB_CN TABLE: my_category_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Country_Tw()
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            //建構子
            List<Model.My_Country_Tw> modelList = new List<Model.My_Country_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:0:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }
            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_country_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_country_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_country_tw").Key, Db_Tw_SqlConnection))
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

                Db_Tw_SqlConnection.Close();
                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:2:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }
                //修改簡體資料庫  
                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_country_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_country_tw:1").Value +
                                         "='" + s.Country_Name_TW +
                                         "' WHERE " + configuration.GetSection("db:2:tables:my_country_tw:0").Value +
                                         "='" + s.Country_ID + "'", Db_Cn_SqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB:DB_CN TABLE: my_country_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Press_Tw()
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Press_Tw> modelList = new List<Model.My_Press_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:0:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }
            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_press_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_press_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_press_tw").Key, Db_Tw_SqlConnection))
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

                Db_Tw_SqlConnection.Close();
                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:2:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_press_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_press_tw:1").Value +
                                         "='" + s.Press_Title +
                                         "' WHERE " + configuration.GetSection("db:2:tables:my_press_tw:0").Value +
                                         "='" + s.Press_SEQ + "'", Db_Cn_SqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB:DB_CN TABLE: my_press_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Product_Tw()
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Product_Tw> modelList = new List<Model.My_Product_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:0:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }
            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_product_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_product_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_product_tw").Key, Db_Tw_SqlConnection))
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

                Db_Tw_SqlConnection.Close();
                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:2:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                       new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_product_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_product_tw:1").Value +
                                         "='" + s.Prod_Title_TW +
                                         "' WHERE " + configuration.GetSection("db:2:tables:my_product_tw:0").Value +
                                         "='" + s.Prod_ID + "'", Db_Cn_SqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB:DB_CN TABLE: my_product_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Publisher_Tw()
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Publisher_Tw> modelList = new List<Model.My_Publisher_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:0:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_publisher_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_publisher_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_publisher_tw").Key, Db_Tw_SqlConnection))
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

                Db_Tw_SqlConnection.Close();
                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:2:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                         new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_publisher_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_publisher_tw:1").Value +
                                         "='" + s.Pub_Intro_TW +
                                         "' WHERE " + configuration.GetSection("db:2:tables:my_publisher_tw:0").Value +
                                         "='" + s.Pub_ID + "'", Db_Cn_SqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB:DB_CN TABLE: my_publisher_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Region_Tw()
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Region_Tw> modelList = new List<Model.My_Region_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:0:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_region_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_region_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_region_tw").Key, Db_Tw_SqlConnection))
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

                Db_Tw_SqlConnection.Close();
                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:2:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                           new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_region_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_region_tw:1").Value +
                                         "='" + s.Region_Name_TW +
                                         "' WHERE " + configuration.GetSection("db:2:tables:my_region_tw:0").Value +
                                         "='" + s.Region_ID + "'", Db_Cn_SqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB:DB_CN TABLE: my_region_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Subtopic_Tw()
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Subtopic_Tw> modelList = new List<Model.My_Subtopic_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:0:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_subtopic_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_subtopic_tw:1").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_subtopic_tw").Key, Db_Tw_SqlConnection))
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

                Db_Tw_SqlConnection.Close();
                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:2:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_subtopic_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_subtopic_tw:1").Value +
                                         "='" + s.SubTopic_Name_TW +
                                         "' WHERE " + configuration.GetSection("db:2:tables:my_subtopic_tw:0").Value +
                                         "='" + s.SubTopic_ID + "'", Db_Cn_SqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB:DB_CN TABLE: my_subtopic_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Topic_Tw()
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            List<Model.My_Topic_Tw> modelList = new List<Model.My_Topic_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:0:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("db:0:tables:my_topic_tw:0").Value
                                               + "," + configuration.GetSection("db:0:tables:my_topic_tw:1").Value
                                               + "," + configuration.GetSection("db:0:tables:my_topic_tw:2").Value
                                               + " FROM " + configuration.GetSection("db:0:dbname").Value + "." +
                                               configuration.GetSection("db:0:tables:my_topic_tw").Key, Db_Tw_SqlConnection))
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

                            if (!myData.IsDBNull(2))
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

                Db_Tw_SqlConnection.Close();
                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:2:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand UPDATmySqlCommand =
                         new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_topic_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_topic_tw:1").Value +
                                         "='" + s.Topic_Intro_TW +
                                         "',"+ configuration.GetSection("db:2:tables:my_topic_tw:2").Value +
                                         "='" + s.Topic_Name_TW +
                                         " 'WHERE " + configuration.GetSection("db:2:tables:my_topic_tw:0").Value +
                                         "='" + s.Topic_ID + "'", Db_Cn_SqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB:DB_CN TABLE: my_subtopic_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }
    }
}
