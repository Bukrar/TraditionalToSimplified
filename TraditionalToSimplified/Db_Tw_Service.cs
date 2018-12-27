using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TraditionalToSimplified
{
    public class Db_Tw_Service
    {
        Utility utility = new Utility();
        public void UpdateTableMy_Category_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_category_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Category_Tw> twList = new List<Model.My_Category_Tw>();
            List<string> decodeLastDataList = new List<string>();
            List<string> decodeTwDbDataList = new List<string>();
            List<string> cnDbPkList = new List<string>();
            List<string> twDbPkList = new List<string>();
            List<string> needUpdateData = new List<string>();
            List<string> needDeleteData = new List<string>();
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

            //讀取TW資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT Category_ID,Category_Name_TW FROM DB_TW.my_category_tw", Db_Tw_SqlConnection))
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
                                string simplifiedEncodeContent = utility.Big5ToGb18030(myData.GetString(1));
                                Model.My_Category_Tw modelData = new Model.My_Category_Tw();
                                modelData.Category_ID = pk;
                                modelData.Category_Name_TW = simplifiedEncodeContent;
                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.Category_ID + ";" + modelData.Category_Name_TW;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);

                                //PK 存入twDbPkList
                                string datapk = modelData.Category_ID;
                                twDbPkList.Add(datapk);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_category_tw 中欄位值為NULL 資料PK值為: " + myData.GetString(0));
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

                //讀取CN資料庫
                using (MySqlCommand mySqlCommand =
                  new MySqlCommand("SELECT Category_ID,Category_Name_TW FROM DB_CN.my_category_tw", Db_Cn_SqlConnection))
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
                                cnDbPkList.Add(pk);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_CN TABLE: my_category_tw 中欄位值為NULL 資料PK值為: " + myData.GetString(0));
                            }
                        }
                    }
                }

                //讀取TXT進decodeLastDataList
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Category_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Category_Tw.txt");
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        decodeLastDataList.Add(line);
                    }
                    sr.Close();
                }

                //對比資料
                needUpdateData = decodeTwDbDataList.Except(decodeLastDataList).ToList();
                needDeleteData = cnDbPkList.Except(twDbPkList).ToList();

                //刪除 簡體資料庫資料
                foreach (var mustBeDelete in needDeleteData)
                {
                    //string key = utility.Decode(mustBeDelete).Split(';')[0];
                    using (MySqlCommand deleteMySqlCommand =
                       new MySqlCommand("DELETE From " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_category_tw").Key +
                                        " WHERE " + configuration.GetSection("db:2:tables:my_category_tw:0").Value +
                                        "= @Category_ID ", Db_Cn_SqlConnection))
                    {
                        deleteMySqlCommand.Parameters.AddWithValue("@Category_ID", mustBeDelete);
                        deleteMySqlCommand.ExecuteNonQuery();
                    }
                }

                //修改簡體資料庫              
                foreach (var s in needUpdateData)
                {
                    string[] dataArray = utility.Decode(s).Split(';');

                    //修改 簡體資料庫資料
                    if (cnDbPkList.Contains(dataArray[0]))
                    {
                        using (
                            MySqlCommand updatMySqlCommand =
                       new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                        "." + configuration.GetSection("db:2:tables:my_category_tw").Key +
                                        " set " + configuration.GetSection("db:2:tables:my_category_tw:1").Value +
                                        " = @Category_Name_TW WHERE " + configuration.GetSection("db:2:tables:my_category_tw:0").Value +
                                        " = @Category_ID ", Db_Cn_SqlConnection))
                        {
                            updatMySqlCommand.Parameters.AddWithValue("@Category_ID", dataArray[0]);
                            updatMySqlCommand.Parameters.AddWithValue("@Category_Name_TW", dataArray[1]);
                            updatMySqlCommand.ExecuteNonQuery();
                        }
                    }

                    //新增 簡體資料庫資料
                    else
                    {
                        Model.My_Category_Tw my_Category_Tw = new Model.My_Category_Tw();
                        Db_Tw_SqlConnection.Open();
                        using (MySqlCommand mySqlCommand =
                           new MySqlCommand("SELECT * FROM DB_TW.my_category_tw WHERE " +
                                            configuration.GetSection("db:2:tables:my_category_tw:0").Value +
                                            " = @Category_ID", Db_Cn_SqlConnection))
                        {
                            mySqlCommand.Parameters.AddWithValue("@Category_ID", dataArray[0]);
                            MySqlDataReader myData = mySqlCommand.ExecuteReader();
                            while (myData.Read())
                            {
                                my_Category_Tw.Category_ID = myData.GetString(0);
                                my_Category_Tw.Category_Name_TW = utility.Big5ToGb18030(myData.GetString(1));
                                my_Category_Tw.Category_File = myData.GetString(2);
                                my_Category_Tw.Category_Sort_Order = myData.GetInt32(3);
                                my_Category_Tw.Category_report = myData.GetInt32(4);
                                my_Category_Tw.Category_newsletter = myData.GetInt32(5);
                                my_Category_Tw.Category_annual = myData.GetInt32(6);
                                my_Category_Tw.Category_company = myData.GetInt32(7);
                            }
                        }
                        Db_Tw_SqlConnection.Close();

                        using (MySqlCommand createMySqlCommand =
                              new MySqlCommand("INSERT INTO " + configuration.GetSection("db:2:dbname").Value +
                                     "." + configuration.GetSection("db:2:tables:my_category_tw").Key +
                                     "(Category_ID,Category_Name_TW,Category_File,Category_Sort_Order,Category_report,Category_newsletter,Category_annual,Category_company)" +
                                     "VALUES (@Category_ID,@Category_Name_TW,@Category_File,@Category_Sort_Order,@Category_report,@Category_newsletter,@Category_annual,@Category_company);"
                                     , Db_Cn_SqlConnection))
                        {
                            createMySqlCommand.Parameters.AddWithValue("@Category_ID", my_Category_Tw.Category_ID);
                            createMySqlCommand.Parameters.AddWithValue("@Category_Name_TW", my_Category_Tw.Category_Name_TW);
                            createMySqlCommand.Parameters.AddWithValue("@Category_File", my_Category_Tw.Category_File);
                            createMySqlCommand.Parameters.AddWithValue("@Category_Sort_Order", my_Category_Tw.Category_Sort_Order);
                            createMySqlCommand.Parameters.AddWithValue("@Category_report", my_Category_Tw.Category_report);
                            createMySqlCommand.Parameters.AddWithValue("@Category_newsletter", my_Category_Tw.Category_newsletter);
                            createMySqlCommand.Parameters.AddWithValue("@Category_annual", my_Category_Tw.Category_annual);
                            createMySqlCommand.Parameters.AddWithValue("@Category_company", my_Category_Tw.Category_company);
                            createMySqlCommand.ExecuteNonQuery();
                        }
                    }
                }
                Db_Cn_SqlConnection.Close();


                //寫入Db_Tw資料進.txt檔案
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/HashData"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/HashData");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Category_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Category_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Category_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Category_ID + ";" + a.Category_Name_TW;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_category_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Country_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_country_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

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
                    new MySqlCommand("SELECT Country_ID,Country_Name_TW FROM DB_TW.my_country_tw", Db_Tw_SqlConnection))
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
                                Console.WriteLine("DB:DB_TW TABLE: my_country_tw 中欄位值為NULL 資料PK值為: " + myData.GetString(0));
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
                    using (MySqlCommand updatMySqlCommand =
                        new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_country_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_country_tw:1").Value +
                                         "= @Country_Name_TW WHERE " + configuration.GetSection("db:2:tables:my_country_tw:0").Value +
                                         "= @Country_ID", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@Country_Name_TW", s.Country_Name_TW);
                        updatMySqlCommand.Parameters.AddWithValue("@Country_ID", s.Country_ID);
                        updatMySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_country_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Press_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_press_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

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
                    new MySqlCommand("SELECT Press_SEQ,Press_Title FROM DB_TW.my_press_tw", Db_Tw_SqlConnection))
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
                                Console.WriteLine("DB:DB_TW TABLE: my_press_tw 中欄位值為NULL 資料PK值為: " + myData.GetString(0));
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
                    using (MySqlCommand updatMySqlCommand =
                        new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_press_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_press_tw:1").Value +
                                         "= @Press_Title WHERE " + configuration.GetSection("db:2:tables:my_press_tw:0").Value +
                                         "= @Press_SEQ", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@Press_Title", s.Press_Title);
                        updatMySqlCommand.Parameters.AddWithValue("@Press_SEQ", s.Press_SEQ);
                        updatMySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_press_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Product_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_product_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

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
                    new MySqlCommand("SELECT Prod_ID,Prod_Title_TW FROM DB_TW.my_product_tw", Db_Tw_SqlConnection))
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
                                Console.WriteLine("DB:DB_TW TABLE: my_product_tw 中欄位值為NULL 資料PK值為: " + myData.GetString(0));
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
                    using (MySqlCommand updatMySqlCommand =
                       new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_product_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_product_tw:1").Value +
                                         "= @Prod_Title_TW WHERE " + configuration.GetSection("db:2:tables:my_product_tw:0").Value +
                                         "= @Prod_ID", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@Prod_Title_TW", s.Prod_Title_TW);
                        updatMySqlCommand.Parameters.AddWithValue("@Prod_ID", s.Prod_ID);
                        updatMySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_product_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Publisher_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_publisher_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

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
                    new MySqlCommand("SELECT Pub_ID,Pub_Intro_TW FROM DB_TW.my_publisher_tw", Db_Tw_SqlConnection))
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
                                Console.WriteLine("DB:DB_TW TABLE: my_publisher_tw 中欄位值為NULL 資料PK值為: " + myData.GetString(0));
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
                    using (MySqlCommand updatMySqlCommand =
                         new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_publisher_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_publisher_tw:1").Value +
                                         "= @Pub_Intro_TW WHERE " + configuration.GetSection("db:2:tables:my_publisher_tw:0").Value +
                                         "= @Pub_ID", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@Pub_Intro_TW", s.Pub_Intro_TW);
                        updatMySqlCommand.Parameters.AddWithValue("@Pub_ID", s.Pub_ID);
                        updatMySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_publisher_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Region_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_region_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

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
                    new MySqlCommand("SELECT Region_ID,Region_Name_TW FROM DB_TW.my_region_tw", Db_Tw_SqlConnection))
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
                                Console.WriteLine("DB:DB_TW TABLE: my_region_tw 中欄位值為NULL 資料PK值為: " + myData.GetString(0));
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
                    using (MySqlCommand updatMySqlCommand =
                           new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_region_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_region_tw:1").Value +
                                         "= @Region_Name_TW WHERE " + configuration.GetSection("db:2:tables:my_region_tw:0").Value +
                                         "= @Region_ID", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@Region_Name_TW", s.Region_Name_TW);
                        updatMySqlCommand.Parameters.AddWithValue("@Region_ID", s.Region_ID);
                        updatMySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_region_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Subtopic_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_subtopic_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

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
                    new MySqlCommand("SELECT SubTopic_ID,SubTopic_Name_TW FROM DB_TW.my_subtopic_tw", Db_Tw_SqlConnection))
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
                                Console.WriteLine("DB:DB_TW TABLE: my_subtopic_tw 中欄位值為NULL 資料PK值為: " + myData.GetString(0));
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
                    using (MySqlCommand updatMySqlCommand =
                        new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_subtopic_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_subtopic_tw:1").Value +
                                         "= @SubTopic_Name_TW WHERE " + configuration.GetSection("db:2:tables:my_subtopic_tw:0").Value +
                                         "= @SubTopic_ID", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@SubTopic_Name_TW", s.SubTopic_Name_TW);
                        updatMySqlCommand.Parameters.AddWithValue("@SubTopic_ID", s.SubTopic_ID);
                        updatMySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_subtopic_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Topic_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_topic_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

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
                    new MySqlCommand("SELECT Topic_ID,Topic_Intro_TW,Topic_Name_TW FROM DB_TW.my_topic_tw", Db_Tw_SqlConnection))
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
                                Console.WriteLine("DB:DB_TW TABLE: my_topic_tw 中欄位值為NULL 資料PK值為: " + myData.GetString(0));
                            }

                            if (!myData.IsDBNull(2))
                            {
                                traditionalNameContent = HttpUtility.UrlDecode(myData.GetString(2), Encoding.GetEncoding("big5"));
                                simplifiedNameContent = utility.ToSimplified(traditionalNameContent, "ToSimplified");
                                simplifiedEncodeNameContent = HttpUtility.UrlEncode(simplifiedNameContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_topic_tw 中欄位值為NULL 資料PK值為: " + myData.GetString(0));
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
                    using (MySqlCommand updatMySqlCommand =
                         new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_topic_tw").Key +
                                         " set " + configuration.GetSection("db:2:tables:my_topic_tw:1").Value +
                                         "= @Topic_Intro_TW," + configuration.GetSection("db:2:tables:my_topic_tw:2").Value +
                                         "= @Topic_Name_TW WHERE " + configuration.GetSection("db:2:tables:my_topic_tw:0").Value +
                                         "= @Topic_ID", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@Topic_Intro_TW", s.Topic_Intro_TW);
                        updatMySqlCommand.Parameters.AddWithValue("@Topic_Name_TW", s.Topic_Name_TW);
                        updatMySqlCommand.Parameters.AddWithValue("@Topic_ID", s.Topic_ID);
                        updatMySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_subtopic_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }
    }
}
