using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TraditionalToSimplified
{
    public class Db_Tw_Service
    {
        Utility utility = new Utility();
        public void UpdateTableMy_Category_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_category_tw...");
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
                            //PK 存入twDbPkList
                            string datapk = myData.GetString(0);
                            twDbPkList.Add(datapk);

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
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_category_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
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
                            string pk = myData.GetString(0);
                            cnDbPkList.Add(pk);
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
                    try
                    {
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
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("刪除資料庫資料異常: " + ex.Message);
                    }
                }

                //修改簡體資料庫              
                foreach (var s in needUpdateData)
                {
                    string[] dataArray = utility.Decode(s).Split(';');

                    //修改 簡體資料庫資料
                    if (cnDbPkList.Contains(dataArray[0]))
                    {
                        try
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
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("修改資料庫資料異常: " + ex.Message);
                        }
                    }

                    //新增 簡體資料庫資料
                    else
                    {
                        try
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
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("新增資料庫資料異常: " + ex.Message);
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
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_country_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Country_Tw> twList = new List<Model.My_Country_Tw>();
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
                            //PK 存入twDbPkList
                            string datapk = myData.GetString(0);
                            twDbPkList.Add(datapk);

                            if (!myData.IsDBNull(1))
                            {
                                string pk = myData.GetString(0);
                                string simplifiedEncodeContent = utility.Big5ToGb18030(myData.GetString(1));
                                Model.My_Country_Tw modelData = new Model.My_Country_Tw();
                                modelData.Country_ID = pk;
                                modelData.Country_Name_TW = simplifiedEncodeContent;
                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.Country_ID + ";" + modelData.Country_Name_TW;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_country_tw中 中欄位值為NULL 資料PK為: " + myData.GetString(0));
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
                  new MySqlCommand("SELECT Country_ID,Country_Name_TW FROM DB_CN.my_country_tw", Db_Cn_SqlConnection))
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
                            cnDbPkList.Add(pk);
                        }
                    }
                }

                //讀取TXT進decodeLastDataList
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Country_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Country_Tw.txt");
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
                    try
                    {
                        using (MySqlCommand deleteMySqlCommand =
                       new MySqlCommand("DELETE From " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_country_tw").Key +
                                        " WHERE " + configuration.GetSection("db:2:tables:my_country_tw:0").Value +
                                        "= @Country_ID ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@Country_ID", mustBeDelete);
                            deleteMySqlCommand.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("刪除資料庫資料異常: " + ex.Message);
                    }

                }

                //修改簡體資料庫              
                foreach (var s in needUpdateData)
                {
                    string[] dataArray = utility.Decode(s).Split(';');

                    //修改 簡體資料庫資料
                    if (cnDbPkList.Contains(dataArray[0]))
                    {
                        try
                        {
                            using (
                                MySqlCommand updatMySqlCommand =
                           new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                            "." + configuration.GetSection("db:2:tables:my_country_tw").Key +
                                            " set " + configuration.GetSection("db:2:tables:my_country_tw:1").Value +
                                            " = @Country_Name_TW WHERE " + configuration.GetSection("db:2:tables:my_country_tw:0").Value +
                                            " = @Country_ID ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@Country_ID", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@Country_Name_TW", dataArray[1]);
                                updatMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("修改資料庫資料異常: " + ex.Message);
                        }
                    }

                    //新增 簡體資料庫資料
                    else
                    {
                        try
                        {
                            Model.My_Country_Tw my_Country_Tw = new Model.My_Country_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_TW.my_country_tw WHERE " +
                                                configuration.GetSection("db:2:tables:my_country_tw:0").Value +
                                                " = @Country_ID", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@Country_ID", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_Country_Tw.Country_ID = myData.GetString(0);
                                    my_Country_Tw.Country_Name_TW = utility.Big5ToGb18030(myData.GetString(1));
                                    my_Country_Tw.Country_report = myData.GetInt32(2);
                                    my_Country_Tw.Country_newsletter = myData.GetInt32(3);
                                    my_Country_Tw.Country_annual = myData.GetInt32(4);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_country_tw").Key +
                                         "(Country_ID,Country_Name_TW,Country_report,Country_newsletter,Country_annual)" +
                                         "VALUES (@Country_ID,@Country_Name_TW,@Country_report,@Country_newsletter,@Country_annual);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@Country_ID", my_Country_Tw.Country_ID);
                                createMySqlCommand.Parameters.AddWithValue("@Country_Name_TW", my_Country_Tw.Country_Name_TW);
                                createMySqlCommand.Parameters.AddWithValue("@Country_report", my_Country_Tw.Country_report);
                                createMySqlCommand.Parameters.AddWithValue("@Country_newsletter", my_Country_Tw.Country_newsletter);
                                createMySqlCommand.Parameters.AddWithValue("@Country_annual", my_Country_Tw.Country_annual);
                                createMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("新增資料庫資料異常: " + ex.Message);
                        }
                    }
                }
                Db_Cn_SqlConnection.Close();


                //寫入Db_Tw資料進.txt檔案
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/HashData"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/HashData");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Country_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Country_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Country_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Country_ID + ";" + a.Country_Name_TW;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_country_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Press_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_press_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Press_Tw> twList = new List<Model.My_Press_Tw>();
            List<string> decodeLastDataList = new List<string>();
            List<string> decodeTwDbDataList = new List<string>();
            List<int> cnDbPkList = new List<int>();
            List<int> twDbPkList = new List<int>();
            List<string> needUpdateData = new List<string>();
            List<int> needDeleteData = new List<int>();
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
                            //PK 存入twDbPkList
                            int datapk = myData.GetInt32(0);
                            twDbPkList.Add(datapk);

                            if (!myData.IsDBNull(1))
                            {
                                int pk = myData.GetInt32(0);
                                string simplifiedEncodeContent = utility.Big5ToGb18030(myData.GetString(1));
                                Model.My_Press_Tw modelData = new Model.My_Press_Tw();
                                modelData.Press_SEQ = pk;
                                modelData.Press_Title = simplifiedEncodeContent;
                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.Press_SEQ + ";" + modelData.Press_Title;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_press_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
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
                  new MySqlCommand("SELECT Press_SEQ,Press_Title FROM DB_CN.my_press_tw", Db_Cn_SqlConnection))
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
                            int pk = myData.GetInt32(0);
                            cnDbPkList.Add(pk);
                        }
                    }
                }

                //讀取TXT進decodeLastDataList
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Press_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Press_Tw.txt");
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
                    try
                    {
                        using (MySqlCommand deleteMySqlCommand =
                           new MySqlCommand("DELETE From " + configuration.GetSection("db:2:dbname").Value +
                                             "." + configuration.GetSection("db:2:tables:my_press_tw").Key +
                                            " WHERE " + configuration.GetSection("db:2:tables:my_press_tw:0").Value +
                                            "= @Press_SEQ ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@Press_SEQ", mustBeDelete);
                            deleteMySqlCommand.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("刪除資料庫資料異常: " + ex.Message);
                    }
                }

                //修改簡體資料庫              
                foreach (var s in needUpdateData)
                {
                    string[] dataArray = utility.Decode(s).Split(';');

                    //修改 簡體資料庫資料
                    if (cnDbPkList.Contains(int.Parse(dataArray[0])))
                    {
                        try
                        {
                            using (
                                MySqlCommand updatMySqlCommand =
                           new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                            "." + configuration.GetSection("db:2:tables:my_press_tw").Key +
                                            " set " + configuration.GetSection("db:2:tables:my_press_tw:1").Value +
                                            " = @Press_Title WHERE " + configuration.GetSection("db:2:tables:my_press_tw:0").Value +
                                            " = @Press_SEQ ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@Press_SEQ", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@Press_Title", dataArray[1]);
                                updatMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("修改資料庫資料異常: " + ex.Message);
                        }
                    }

                    //新增 簡體資料庫資料
                    else
                    {
                        try
                        {
                            Model.My_Press_Tw my_Press_Tw = new Model.My_Press_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_TW.my_press_tw WHERE " +
                                                configuration.GetSection("db:2:tables:my_press_tw:0").Value +
                                                " = @Press_SEQ", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@Press_SEQ", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_Press_Tw.Press_SEQ = myData.GetInt32(0);
                                    my_Press_Tw.Press_Type = myData.GetString(1);
                                    my_Press_Tw.Press_Date = myData.GetDateTime(2);
                                    my_Press_Tw.Press_File = myData.GetString(3);
                                    my_Press_Tw.Press_Title = utility.Big5ToGb18030(myData.GetString(4));
                                    if (!myData.IsDBNull(5))
                                    {
                                        my_Press_Tw.Press_Content = myData.GetString(5);
                                    }
                                    my_Press_Tw.Press_On = myData.GetInt32(6);
                                    if (!myData.IsDBNull(7))
                                    {
                                        my_Press_Tw.Press_Prod_ID = myData.GetInt32(7);
                                    }
                                    if (!myData.IsDBNull(8))
                                    {
                                        my_Press_Tw.Press_Prod_Title = myData.GetString(8);
                                    }
                                    if (!myData.IsDBNull(9))
                                    {
                                        my_Press_Tw.Press_Prod_Category = myData.GetString(9);
                                    }
                                    if (!myData.IsDBNull(10))
                                    {
                                        my_Press_Tw.Press_Prod_Discon = myData.GetInt32(10);
                                    }
                                    if (!myData.IsDBNull(11))
                                    {
                                        my_Press_Tw.Press_Pub_Code = myData.GetString(11);
                                    }
                                    if (!myData.IsDBNull(12))
                                    {
                                        my_Press_Tw.Press_Pub_Name = myData.GetString(12);
                                    }
                                    my_Press_Tw.Update = myData.GetDateTime(13);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_press_tw").Key +
                                         "(Press_SEQ,Press_Type,Press_Date,Press_File,Press_Title,Press_Content," +
                                         "Press_On,Press_Prod_ID,Press_Prod_Title,Press_Prod_Category,Press_Prod_Discon," +
                                         "Press_Pub_Code,Press_Pub_Name,`Update`)" +
                                         "VALUES (@Press_SEQ,@Press_Type,@Press_Date,@Press_File,@Press_Title," +
                                         "@Press_Content,@Press_On,@Press_Prod_ID,@Press_Prod_Title,@Press_Prod_Category," +
                                         "@Press_Prod_Discon,@Press_Pub_Code,@Press_Pub_Name,@Update);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@Press_SEQ", my_Press_Tw.Press_SEQ);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Type", my_Press_Tw.Press_Type);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Date", my_Press_Tw.Press_Date);
                                createMySqlCommand.Parameters.AddWithValue("@Press_File", my_Press_Tw.Press_File);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Title", my_Press_Tw.Press_Title);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Content", my_Press_Tw.Press_Content);
                                createMySqlCommand.Parameters.AddWithValue("@Press_On", my_Press_Tw.Press_On);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Prod_ID", my_Press_Tw.Press_Prod_ID);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Prod_Title", my_Press_Tw.Press_Prod_Title);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Prod_Category", my_Press_Tw.Press_Prod_Category);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Prod_Discon", my_Press_Tw.Press_Prod_Discon);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Pub_Code", my_Press_Tw.Press_Pub_Code);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Pub_Name", my_Press_Tw.Press_Pub_Name);
                                createMySqlCommand.Parameters.AddWithValue("@Update", my_Press_Tw.Update);
                                createMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("新增資料庫資料異常: " + ex.Message);
                        }
                    }
                }
                Db_Cn_SqlConnection.Close();

                //寫入Db_Tw資料進.txt檔案
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/HashData"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/HashData");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Press_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Press_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Press_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Press_SEQ + ";" + a.Press_Title;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_press_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Product_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_product_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Product_Tw> twList = new List<Model.My_Product_Tw>();
            List<string> decodeLastDataList = new List<string>();
            List<string> decodeTwDbDataList = new List<string>();
            List<int> cnDbPkList = new List<int>();
            List<int> twDbPkList = new List<int>();
            List<string> needUpdateData = new List<string>();
            List<int> needDeleteData = new List<int>();
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
                            //PK 存入twDbPkList
                            int datapk = myData.GetInt32(0);
                            twDbPkList.Add(datapk);

                            if (!myData.IsDBNull(1))
                            {
                                int pk = myData.GetInt32(0);
                                string simplifiedEncodeContent = utility.Big5ToGb18030(myData.GetString(1));
                                Model.My_Product_Tw modelData = new Model.My_Product_Tw();
                                modelData.Prod_ID = pk;
                                modelData.Prod_Title_TW = simplifiedEncodeContent;
                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.Prod_ID + ";" + modelData.Prod_Title_TW;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);


                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_product_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
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
                  new MySqlCommand("SELECT Prod_ID,Prod_Title_TW FROM DB_CN.my_product_tw", Db_Cn_SqlConnection))
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
                            int pk = myData.GetInt32(0);
                            cnDbPkList.Add(pk);
                        }
                    }
                }

                //讀取TXT進decodeLastDataList
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Product_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Product_Tw.txt");
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
                    try
                    {
                        using (MySqlCommand deleteMySqlCommand =
                           new MySqlCommand("DELETE From " + configuration.GetSection("db:2:dbname").Value +
                                             "." + configuration.GetSection("db:2:tables:my_product_tw").Key +
                                            " WHERE " + configuration.GetSection("db:2:tables:my_product_tw:0").Value +
                                            "= @Prod_ID ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@Prod_ID", mustBeDelete);
                            deleteMySqlCommand.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("刪除資料庫資料異常: " + ex.Message);
                    }
                }

                //修改簡體資料庫              
                foreach (var s in needUpdateData)
                {
                    string[] dataArray = utility.Decode(s).Split(';');

                    //修改 簡體資料庫資料
                    if (cnDbPkList.Contains(int.Parse(dataArray[0])))
                    {
                        try
                        {
                            using (
                                MySqlCommand updatMySqlCommand =
                           new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                            "." + configuration.GetSection("db:2:tables:my_product_tw").Key +
                                            " set " + configuration.GetSection("db:2:tables:my_product_tw:1").Value +
                                            " = @Prod_Title_TW WHERE " + configuration.GetSection("db:2:tables:my_product_tw:0").Value +
                                            " = @Prod_ID ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@Prod_ID", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@Prod_Title_TW", dataArray[1]);
                                updatMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("修改資料庫資料異常: " + ex.Message);
                        }
                    }

                    //新增 簡體資料庫資料
                    else
                    {
                        try
                        {
                            Model.My_Product_Tw my_product_tw = new Model.My_Product_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_TW.my_product_tw WHERE " +
                                                configuration.GetSection("db:2:tables:my_product_tw:0").Value +
                                                " = @Prod_ID", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@Prod_ID", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_product_tw.Prod_ID = myData.GetInt32(0);
                                    my_product_tw.Prod_Delete_Flg = myData.GetInt32(1);
                                    if (!myData.IsDBNull(2))
                                    {
                                        my_product_tw.Prod_Title_EN = myData.GetString(2);
                                    }
                                    if (!myData.IsDBNull(3))
                                    {
                                        my_product_tw.Prod_Title_TW = utility.Big5ToGb18030(myData.GetString(3));
                                    }
                                    if (!myData.IsDBNull(4))
                                    {
                                        my_product_tw.Prod_Type = myData.GetString(4);
                                    }
                                    if (!myData.IsDBNull(5))
                                    {
                                        my_product_tw.Prod_TypeCode = myData.GetString(5);
                                    }
                                    if (!myData.IsDBNull(6))
                                    {
                                        my_product_tw.Prod_MainCategory = myData.GetString(6);
                                    }
                                    if (!myData.IsDBNull(7))
                                    {
                                        my_product_tw.Prod_Category = myData.GetString(7);
                                    }
                                    if (!myData.IsDBNull(8))
                                    {
                                        my_product_tw.Prod_Topic = myData.GetString(8);
                                    }
                                    if (!myData.IsDBNull(9))
                                    {
                                        my_product_tw.Prod_SubTopic = myData.GetString(9);
                                    }
                                    if (!myData.IsDBNull(10))
                                    {
                                        my_product_tw.Prod_Country = myData.GetString(10);
                                    }
                                    if (!myData.IsDBNull(11))
                                    {
                                        my_product_tw.Prod_ID_Pub = myData.GetString(11);
                                    }
                                    if (!myData.IsDBNull(12))
                                    {
                                        my_product_tw.Prod_Pub_ID = myData.GetString(12);
                                    }
                                    if (!myData.IsDBNull(13))
                                    {
                                        my_product_tw.Prod_PubDate = myData.GetString(13);
                                    }
                                    if (!myData.IsDBNull(14))
                                    {
                                        my_product_tw.Prod_File = myData.GetString(14);
                                    }
                                    if (!myData.IsDBNull(15))
                                    {
                                        my_product_tw.Prod_SampleFile = myData.GetString(15);
                                    }
                                    my_product_tw.Prod_BrowseNG_Flg = myData.GetInt32(16);
                                    my_product_tw.Prod_FullRepLink_Flg = myData.GetInt32(17);
                                    if (!myData.IsDBNull(18))
                                    {
                                        my_product_tw.Prod_Intro_TW = myData.GetString(18);
                                    }
                                    my_product_tw.Prod_Update = myData.GetDateTime(19);
                                    if (!myData.IsDBNull(20))
                                    {
                                        my_product_tw.Prod_CurrencyCode = myData.GetString(20);
                                    }
                                    my_product_tw.Prod_LowPrice = myData.GetFloat(21);
                                    if (!myData.IsDBNull(22))
                                    {
                                        my_product_tw.Prod_BeforePrice = myData.GetFloat(22);
                                    }
                                    if (!myData.IsDBNull(23))
                                    {
                                        my_product_tw.Prod_ContentInfo = myData.GetString(23);
                                    }
                                    if (!myData.IsDBNull(24))
                                    {
                                        my_product_tw.Prod_Related_ID = myData.GetString(24);
                                    }
                                    if (!myData.IsDBNull(25))
                                    {
                                        my_product_tw.Prod_Related_Keywords = myData.GetString(25);
                                    }
                                    if (!myData.IsDBNull(26))
                                    {
                                        my_product_tw.Prod_IssueFrequency = myData.GetInt32(26);
                                    }
                                    my_product_tw.Prod_PrePub = myData.GetInt32(27);
                                    my_product_tw.Prod_CoverImage = myData.GetInt32(28);
                                    if (!myData.IsDBNull(29))
                                    {
                                        my_product_tw.Prod_WhatsNew = myData.GetDateTime(29);
                                    }
                                    my_product_tw.Prod_Link_EN = myData.GetInt32(30);
                                    my_product_tw.Prod_Link_JP = myData.GetInt32(31);
                                    my_product_tw.Prod_Link_KR = myData.GetInt32(32);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_product_tw").Key +
                                         "(Prod_ID,Prod_Delete_Flg,Prod_Title_EN,Prod_Title_TW,Prod_Type,Prod_TypeCode,Prod_MainCategory,Prod_Category,Prod_Topic,Prod_SubTopic,Prod_Country,Prod_ID_Pub,Prod_Pub_ID,Prod_PubDate,Prod_File,Prod_SampleFile,Prod_BrowseNG_Flg,Prod_FullRepLink_Flg,Prod_Intro_TW,Prod_Update,Prod_CurrencyCode,Prod_LowPrice,Prod_BeforePrice,Prod_ContentInfo,Prod_Related_ID,Prod_Related_Keywords,Prod_IssueFrequency,Prod_PrePub,Prod_CoverImage,Prod_WhatsNew,Prod_Link_EN,Prod_Link_JP,Prod_Link_KR)" +
                                         "VALUES (@Prod_ID,@Prod_Delete_Flg,@Prod_Title_EN,@Prod_Title_TW,@Prod_Type,@Prod_TypeCode,@Prod_MainCategory,@Prod_Category,@Prod_Topic,@Prod_SubTopic,@Prod_Country,@Prod_ID_Pub,@Prod_Pub_ID,@Prod_PubDate,@Prod_File,@Prod_SampleFile,@Prod_BrowseNG_Flg,@Prod_FullRepLink_Flg,@Prod_Intro_TW,@Prod_Update,@Prod_CurrencyCode,@Prod_LowPrice,@Prod_BeforePrice,@Prod_ContentInfo,@Prod_Related_ID,@Prod_Related_Keywords,@Prod_IssueFrequency,@Prod_PrePub,@Prod_CoverImage,@Prod_WhatsNew,@Prod_Link_EN,@Prod_Link_JP,@Prod_Link_KR);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@Prod_ID", my_product_tw.Prod_ID);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Delete_Flg", my_product_tw.Prod_Delete_Flg);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Title_EN", my_product_tw.Prod_Title_EN);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Title_TW", my_product_tw.Prod_Title_TW);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Type", my_product_tw.Prod_Type);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_TypeCode", my_product_tw.Prod_TypeCode);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Category", my_product_tw.Prod_Category);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_MainCategory", my_product_tw.Prod_MainCategory);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Topic", my_product_tw.Prod_Topic);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_SubTopic", my_product_tw.Prod_SubTopic);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Country", my_product_tw.Prod_Country);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_ID_Pub", my_product_tw.Prod_ID_Pub);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Pub_ID", my_product_tw.Prod_Pub_ID);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_PubDate", my_product_tw.Prod_PubDate);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_File", my_product_tw.Prod_File);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_SampleFile", my_product_tw.Prod_SampleFile);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_BrowseNG_Flg", my_product_tw.Prod_BrowseNG_Flg);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_FullRepLink_Flg", my_product_tw.Prod_FullRepLink_Flg);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Intro_TW", my_product_tw.Prod_Intro_TW);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Update", my_product_tw.Prod_Update);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_CurrencyCode", my_product_tw.Prod_CurrencyCode);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_LowPrice", my_product_tw.Prod_LowPrice);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_BeforePrice", my_product_tw.Prod_BeforePrice);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_ContentInfo", my_product_tw.Prod_ContentInfo);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Related_ID", my_product_tw.Prod_Related_ID);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Related_Keywords", my_product_tw.Prod_Related_Keywords);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_IssueFrequency", my_product_tw.Prod_IssueFrequency);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_PrePub", my_product_tw.Prod_PrePub);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_CoverImage", my_product_tw.Prod_CoverImage);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_WhatsNew", my_product_tw.Prod_WhatsNew);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Link_EN", my_product_tw.Prod_Link_EN);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Link_JP", my_product_tw.Prod_Link_JP);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_Link_KR", my_product_tw.Prod_Link_KR);
                                createMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("新增資料庫資料異常: " + ex.Message);
                        }
                    }
                }
                Db_Cn_SqlConnection.Close();


                //寫入Db_Tw資料進.txt檔案
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/HashData"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/HashData");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Product_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Product_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Product_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Prod_ID + ";" + a.Prod_Title_TW;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_product_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Publisher_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_publisher_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Publisher_Tw> twList = new List<Model.My_Publisher_Tw>();
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
                            //PK 存入twDbPkList
                            string datapk = myData.GetString(0);
                            twDbPkList.Add(datapk);

                            if (!myData.IsDBNull(1))
                            {
                                string pk = myData.GetString(0);
                                string simplifiedEncodeContent = utility.Big5ToGb18030(myData.GetString(1));
                                Model.My_Publisher_Tw modelData = new Model.My_Publisher_Tw();
                                modelData.Pub_ID = pk;
                                modelData.Pub_Intro_TW = simplifiedEncodeContent;
                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.Pub_ID + ";" + modelData.Pub_Intro_TW;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_publisher_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
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
                  new MySqlCommand("SELECT Pub_ID,Pub_Intro_TW FROM DB_CN.my_publisher_tw", Db_Cn_SqlConnection))
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
                            cnDbPkList.Add(pk);
                        }
                    }
                }

                //讀取TXT進decodeLastDataList
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Publisher_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Publisher_Tw.txt");
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
                    try
                    {
                        using (MySqlCommand deleteMySqlCommand =
                           new MySqlCommand("DELETE From " + configuration.GetSection("db:2:dbname").Value +
                                             "." + configuration.GetSection("db:2:tables:my_publisher_tw").Key +
                                            " WHERE " + configuration.GetSection("db:2:tables:my_publisher_tw:0").Value +
                                            "= @Pub_ID ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@Pub_ID", mustBeDelete);
                            deleteMySqlCommand.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("刪除資料庫資料異常: " + ex.Message);
                    }
                }

                //修改簡體資料庫              
                foreach (var s in needUpdateData)
                {
                    string[] dataArray = utility.Decode(s).Split(';');

                    //修改 簡體資料庫資料
                    if (cnDbPkList.Contains(dataArray[0]))
                    {
                        try
                        {
                            using (
                                MySqlCommand updatMySqlCommand =
                           new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                            "." + configuration.GetSection("db:2:tables:my_publisher_tw").Key +
                                            " set " + configuration.GetSection("db:2:tables:my_publisher_tw:1").Value +
                                            " = @Pub_Intro_TW WHERE " + configuration.GetSection("db:2:tables:my_publisher_tw:0").Value +
                                            " = @Pub_ID ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@Pub_ID", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@Pub_Intro_TW", dataArray[1]);
                                updatMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("修改資料庫資料異常: " + ex.Message);
                        }
                    }

                    //新增 簡體資料庫資料
                    else
                    {
                        try
                        {
                            Model.My_Publisher_Tw my_publisher_tw = new Model.My_Publisher_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_TW.my_publisher_tw WHERE " +
                                                configuration.GetSection("db:2:tables:my_publisher_tw:0").Value +
                                                " = @Pub_ID", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@Pub_ID", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_publisher_tw.Pub_ID = myData.GetString(0);
                                    my_publisher_tw.Pub_Name = myData.GetString(1);
                                    my_publisher_tw.Pub_Sample_Flg = myData.GetInt32(2);
                                    my_publisher_tw.Pub_Browse_Flg = myData.GetInt32(3);
                                    if (!myData.IsDBNull(4))
                                    {
                                        my_publisher_tw.Pub_Status = myData.GetString(4);
                                    }
                                    if (!myData.IsDBNull(5))
                                    {
                                        my_publisher_tw.Pub_ShippingRoute = myData.GetString(5);
                                    }
                                    my_publisher_tw.Pub_Organizer_Flg = myData.GetInt32(6);
                                    my_publisher_tw.Pub_report = myData.GetInt32(7);
                                    my_publisher_tw.Pub_annual = myData.GetInt32(8);
                                    my_publisher_tw.Pub_newsletter = myData.GetInt32(9);
                                    my_publisher_tw.Pub_Update = myData.GetDateTime(10);
                                    if (!myData.IsDBNull(11))
                                    {
                                        my_publisher_tw.Pub_Intro_TW = utility.Big5ToGb18030(myData.GetString(11));
                                    }
                                    if (!myData.IsDBNull(12))
                                    {
                                        my_publisher_tw.Pub_Name_TW = myData.GetString(12);
                                    }
                                    my_publisher_tw.Pub_Exclusive_TW = myData.GetInt32(13);
                                    my_publisher_tw.Pub_EN = myData.GetInt32(14);
                                    my_publisher_tw.Pub_KR = myData.GetInt32(15);
                                    my_publisher_tw.Pub_JP = myData.GetInt32(16);
                                    my_publisher_tw.Pub_TW = myData.GetInt32(17);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_publisher_tw").Key +
                                         "(Pub_ID,Pub_Name,Pub_Sample_Flg,Pub_Browse_Flg,Pub_Status,Pub_ShippingRoute,Pub_Organizer_Flg,Pub_report,Pub_annual,Pub_newsletter,Pub_Update,Pub_Intro_TW,Pub_Name_TW,Pub_Exclusive_TW,Pub_EN,Pub_KR,Pub_JP,Pub_TW)" +
                                         "VALUES (@Pub_ID,@Pub_Name,@Pub_Sample_Flg,@Pub_Browse_Flg,@Pub_Status,@Pub_ShippingRoute,@Pub_Organizer_Flg,@Pub_report,@Pub_annual,@Pub_newsletter,@Pub_Update,@Pub_Intro_TW,@Pub_Name_TW,@Pub_Exclusive_TW,@Pub_EN,@Pub_KR,@Pub_JP,@Pub_TW);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@Pub_ID", my_publisher_tw.Pub_ID);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_Name", my_publisher_tw.Pub_Name);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_Sample_Flg", my_publisher_tw.Pub_Sample_Flg);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_Browse_Flg", my_publisher_tw.Pub_Browse_Flg);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_Status", my_publisher_tw.Pub_Status);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_ShippingRoute", my_publisher_tw.Pub_ShippingRoute);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_Organizer_Flg", my_publisher_tw.Pub_Organizer_Flg);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_report", my_publisher_tw.Pub_report);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_annual", my_publisher_tw.Pub_annual);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_newsletter", my_publisher_tw.Pub_newsletter);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_Update", my_publisher_tw.Pub_Update);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_Intro_TW", my_publisher_tw.Pub_Intro_TW);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_Name_TW", my_publisher_tw.Pub_Name_TW);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_Exclusive_TW", my_publisher_tw.Pub_Exclusive_TW);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_EN", my_publisher_tw.Pub_EN);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_KR", my_publisher_tw.Pub_KR);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_JP", my_publisher_tw.Pub_JP);
                                createMySqlCommand.Parameters.AddWithValue("@Pub_TW", my_publisher_tw.Pub_TW);
                                createMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("新增資料庫資料異常: " + ex.Message);
                        }
                    }
                }
                Db_Cn_SqlConnection.Close();


                //寫入Db_Tw資料進.txt檔案
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/HashData"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/HashData");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Publisher_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Publisher_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Publisher_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Pub_ID + ";" + a.Pub_Intro_TW;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_publisher_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Region_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_region_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Region_Tw> twList = new List<Model.My_Region_Tw>();
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
                            //PK 存入twDbPkList
                            string datapk = myData.GetString(0);
                            twDbPkList.Add(datapk);

                            if (!myData.IsDBNull(1))
                            {
                                string pk = myData.GetString(0);
                                string simplifiedEncodeContent = utility.Big5ToGb18030(myData.GetString(1));
                                Model.My_Region_Tw modelData = new Model.My_Region_Tw();
                                modelData.Region_ID = pk;
                                modelData.Region_Name_TW = simplifiedEncodeContent;
                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.Region_ID + ";" + modelData.Region_Name_TW;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_region_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
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
                  new MySqlCommand("SELECT Region_ID,Region_Name_TW FROM DB_CN.my_region_tw", Db_Cn_SqlConnection))
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
                            cnDbPkList.Add(pk);
                        }
                    }
                }

                //讀取TXT進decodeLastDataList
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Region_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Region_Tw.txt");
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
                    try
                    {
                        using (MySqlCommand deleteMySqlCommand =
                       new MySqlCommand("DELETE From " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_region_tw").Key +
                                        " WHERE " + configuration.GetSection("db:2:tables:my_region_tw:0").Value +
                                        "= @Region_ID ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@Region_ID", mustBeDelete);
                            deleteMySqlCommand.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("刪除資料庫資料異常: " + ex.Message);
                    }

                }

                //修改簡體資料庫              
                foreach (var s in needUpdateData)
                {
                    string[] dataArray = utility.Decode(s).Split(';');

                    //修改 簡體資料庫資料
                    if (cnDbPkList.Contains(dataArray[0]))
                    {
                        try
                        {
                            using (
                                MySqlCommand updatMySqlCommand =
                           new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                            "." + configuration.GetSection("db:2:tables:my_region_tw").Key +
                                            " set " + configuration.GetSection("db:2:tables:my_region_tw:1").Value +
                                            " = @Region_Name_TW WHERE " + configuration.GetSection("db:2:tables:my_region_tw:0").Value +
                                            " = @Region_ID ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@Region_ID", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@Region_Name_TW", dataArray[1]);
                                updatMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("修改資料庫資料異常: " + ex.Message);
                        }
                    }

                    //新增 簡體資料庫資料
                    else
                    {
                        try
                        {
                            Model.My_Region_Tw my_region_tw = new Model.My_Region_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_TW.my_region_tw WHERE " +
                                                configuration.GetSection("db:2:tables:my_region_tw:0").Value +
                                                " = @Region_ID", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@Region_ID", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_region_tw.Region_ID = myData.GetString(0);
                                    my_region_tw.Region_Name_TW = utility.Big5ToGb18030(myData.GetString(1));
                                    my_region_tw.Region_report = myData.GetInt32(2);
                                    my_region_tw.Region_annual = myData.GetInt32(3);
                                    my_region_tw.Region_newsletter = myData.GetInt32(4);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_region_tw").Key +
                                         "(Region_ID,Region_Name_TW,Region_report,Region_annual,Region_newsletter)" +
                                         "VALUES (@Region_ID,@Region_Name_TW,@Region_report,@Region_annual,@Region_newsletter);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@Region_ID", my_region_tw.Region_ID);
                                createMySqlCommand.Parameters.AddWithValue("@Region_Name_TW", my_region_tw.Region_Name_TW);
                                createMySqlCommand.Parameters.AddWithValue("@Region_report", my_region_tw.Region_report);
                                createMySqlCommand.Parameters.AddWithValue("@Region_annual", my_region_tw.Region_annual);
                                createMySqlCommand.Parameters.AddWithValue("@Region_newsletter", my_region_tw.Region_newsletter);
                                createMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("新增資料庫資料異常: " + ex.Message);
                        }
                    }
                }
                Db_Cn_SqlConnection.Close();


                //寫入Db_Tw資料進.txt檔案
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/HashData"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/HashData");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Region_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Region_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Region_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Region_ID + ";" + a.Region_Name_TW;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_region_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Subtopic_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_subtopic_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Subtopic_Tw> twList = new List<Model.My_Subtopic_Tw>();
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
                            //PK 存入twDbPkList
                            string datapk = myData.GetString(0);
                            twDbPkList.Add(datapk);

                            if (!myData.IsDBNull(1))
                            {
                                string pk = myData.GetString(0);
                                string simplifiedEncodeContent = utility.Big5ToGb18030(myData.GetString(1));
                                Model.My_Subtopic_Tw modelData = new Model.My_Subtopic_Tw();
                                modelData.SubTopic_ID = pk;
                                modelData.SubTopic_Name_TW = simplifiedEncodeContent;
                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.SubTopic_ID + ";" + modelData.SubTopic_Name_TW;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_subtopic_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
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
                  new MySqlCommand("SELECT SubTopic_ID,SubTopic_Name_TW FROM DB_CN.my_subtopic_tw", Db_Cn_SqlConnection))
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
                            cnDbPkList.Add(pk);
                        }
                    }
                }

                //讀取TXT進decodeLastDataList
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Subtopic_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Subtopic_Tw.txt");
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
                    try
                    {
                        using (MySqlCommand deleteMySqlCommand =
                       new MySqlCommand("DELETE From " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_subtopic_tw").Key +
                                        " WHERE " + configuration.GetSection("db:2:tables:my_subtopic_tw:0").Value +
                                        "= @SubTopic_ID ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@SubTopic_ID", mustBeDelete);
                            deleteMySqlCommand.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("刪除資料庫資料異常: " + ex.Message);
                    }

                }

                //修改簡體資料庫              
                foreach (var s in needUpdateData)
                {
                    string[] dataArray = utility.Decode(s).Split(';');

                    //修改 簡體資料庫資料
                    if (cnDbPkList.Contains(dataArray[0]))
                    {
                        try
                        {
                            using (
                                MySqlCommand updatMySqlCommand =
                           new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                            "." + configuration.GetSection("db:2:tables:my_subtopic_tw").Key +
                                            " set " + configuration.GetSection("db:2:tables:my_subtopic_tw:1").Value +
                                            " = @SubTopic_Name_TW WHERE " + configuration.GetSection("db:2:tables:my_subtopic_tw:0").Value +
                                            " = @SubTopic_ID ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@SubTopic_ID", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@SubTopic_Name_TW", dataArray[1]);
                                updatMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("修改資料庫資料異常: " + ex.Message);
                        }
                    }

                    //新增 簡體資料庫資料
                    else
                    {
                        try
                        {
                            Model.My_Subtopic_Tw my_subtopic_tw = new Model.My_Subtopic_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_TW.my_subtopic_tw WHERE " +
                                                configuration.GetSection("db:2:tables:my_subtopic_tw:0").Value +
                                                " = @SubTopic_ID", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@SubTopic_ID", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_subtopic_tw.SubTopic_ID = myData.GetString(0);
                                    my_subtopic_tw.SubTopic_Name_TW = utility.Big5ToGb18030(myData.GetString(1));
                                    my_subtopic_tw.SubTopic_Intro_TW = myData.GetString(2);
                                    my_subtopic_tw.SubTopic_Topic_ID = myData.GetString(3);
                                    my_subtopic_tw.SubTopic_report = myData.GetInt32(4);
                                    my_subtopic_tw.SubTopic_newsletter = myData.GetInt32(5);
                                    my_subtopic_tw.SubTopic_annual = myData.GetInt32(6);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_subtopic_tw").Key +
                                         "(SubTopic_ID,SubTopic_Name_TW,SubTopic_Intro_TW,SubTopic_Topic_ID,SubTopic_report,SubTopic_newsletter,SubTopic_annual)" +
                                         "VALUES (@SubTopic_ID,@SubTopic_Name_TW,@SubTopic_Intro_TW,@SubTopic_Topic_ID,@SubTopic_report,@SubTopic_newsletter,@SubTopic_annual);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@SubTopic_ID", my_subtopic_tw.SubTopic_ID);
                                createMySqlCommand.Parameters.AddWithValue("@SubTopic_Name_TW", my_subtopic_tw.SubTopic_Name_TW);
                                createMySqlCommand.Parameters.AddWithValue("@SubTopic_Intro_TW", my_subtopic_tw.SubTopic_Intro_TW);
                                createMySqlCommand.Parameters.AddWithValue("@SubTopic_Topic_ID", my_subtopic_tw.SubTopic_Topic_ID);
                                createMySqlCommand.Parameters.AddWithValue("@SubTopic_report", my_subtopic_tw.SubTopic_report);
                                createMySqlCommand.Parameters.AddWithValue("@SubTopic_newsletter", my_subtopic_tw.SubTopic_newsletter);
                                createMySqlCommand.Parameters.AddWithValue("@SubTopic_annual", my_subtopic_tw.SubTopic_annual);
                                createMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("新增資料庫資料異常: " + ex.Message);
                        }
                    }
                }
                Db_Cn_SqlConnection.Close();


                //寫入Db_Tw資料進.txt檔案
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/HashData"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/HashData");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Subtopic_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Subtopic_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Subtopic_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.SubTopic_ID + ";" + a.SubTopic_Name_TW;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_subtopic_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Topic_Tw()
        {
            Console.WriteLine("處理Db_Tw資料庫 資料表:my_topic_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Topic_Tw> twList = new List<Model.My_Topic_Tw>();
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
                    new MySqlCommand("SELECT Topic_ID,Topic_Name_TW,Topic_Intro_TW FROM DB_TW.my_topic_tw", Db_Tw_SqlConnection))
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
                            Model.My_Topic_Tw modelData = new Model.My_Topic_Tw();
                            string pk = myData.GetString(0);
                            modelData.Topic_ID = pk;
                            //PK 存入twDbPkList
                            string datapk = myData.GetString(0);
                            twDbPkList.Add(datapk);
                            if (!myData.IsDBNull(1))
                            {
                                string simplifiedEncodeContentName = utility.Big5ToGb18030(myData.GetString(1));
                                modelData.Topic_Name_TW = simplifiedEncodeContentName;
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_topic_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
                            }
                            if (!myData.IsDBNull(2))
                            {
                                string simplifiedEncodeContent = utility.Big5ToGb18030(myData.GetString(2));
                                modelData.Topic_Intro_TW = simplifiedEncodeContent;
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_TW TABLE: my_topic_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
                            }

                            twList.Add(modelData);

                            //加密存進decodeDbDataList
                            string data = modelData.Topic_ID + ";" + modelData.Topic_Name_TW + ";" + modelData.Topic_Intro_TW;
                            string encodeData = utility.Encode(data);
                            decodeTwDbDataList.Add(encodeData);
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
                  new MySqlCommand("SELECT Topic_ID,Topic_Name_TW,Topic_Intro_TW FROM DB_CN.my_topic_tw", Db_Cn_SqlConnection))
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
                            cnDbPkList.Add(pk);
                        }
                    }
                }

                //讀取TXT進decodeLastDataList
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Topic_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Topic_Tw.txt");
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
                    try
                    {
                        using (MySqlCommand deleteMySqlCommand =
                           new MySqlCommand("DELETE From " + configuration.GetSection("db:2:dbname").Value +
                                             "." + configuration.GetSection("db:2:tables:my_topic_tw").Key +
                                            " WHERE " + configuration.GetSection("db:2:tables:my_topic_tw:0").Value +
                                            "= @Topic_ID ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@Topic_ID", mustBeDelete);
                            deleteMySqlCommand.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("刪除資料庫資料異常: " + ex.Message);
                    }
                }

                //修改簡體資料庫              
                foreach (var s in needUpdateData)
                {
                    string[] dataArray = utility.Decode(s).Split(';');

                    //修改 簡體資料庫資料
                    if (cnDbPkList.Contains(dataArray[0]))
                    {
                        try
                        {
                            using (
                            MySqlCommand updatMySqlCommand =
                       new MySqlCommand("update " + configuration.GetSection("db:2:dbname").Value +
                                        "." + configuration.GetSection("db:2:tables:my_topic_tw").Key +
                                        " set " + configuration.GetSection("db:2:tables:my_topic_tw:1").Value +
                                        " = @Topic_Name_TW ," + configuration.GetSection("db:2:tables:my_topic_tw:2").Value +
                                        " = @Topic_Intro_TW WHERE " + configuration.GetSection("db:2:tables:my_topic_tw:0").Value +
                                        " = @Topic_ID ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@Topic_ID", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@Topic_Name_TW", dataArray[1]);
                                updatMySqlCommand.Parameters.AddWithValue("@Topic_Intro_TW", dataArray[2]);
                                updatMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("修改資料庫資料異常: " + ex.Message);
                        }
                    }

                    //新增 簡體資料庫資料
                    else
                    {
                        try
                        {
                            Model.My_Topic_Tw my_Topic_Tw = new Model.My_Topic_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_TW.my_topic_tw WHERE " +
                                                configuration.GetSection("db:2:tables:my_topic_tw:0").Value +
                                                " = @Topic_ID", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@Topic_ID", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_Topic_Tw.Topic_ID = myData.GetString(0);
                                    my_Topic_Tw.Topic_Name_TW = utility.Big5ToGb18030(myData.GetString(1));
                                    my_Topic_Tw.Topic_Intro_TW = utility.Big5ToGb18030(myData.GetString(2));
                                    my_Topic_Tw.Topic_DisplayFlag = myData.GetInt32(3);
                                    my_Topic_Tw.Topic_Sort_Order = myData.GetInt32(4);
                                    my_Topic_Tw.Topic_report = myData.GetInt32(5);
                                    my_Topic_Tw.Topic_newsletter = myData.GetInt32(6);
                                    my_Topic_Tw.Topic_annual = myData.GetInt32(7);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:2:dbname").Value +
                                         "." + configuration.GetSection("db:2:tables:my_topic_tw").Key +
                                         "(Topic_ID,Topic_Name_TW,Topic_Intro_TW,Topic_DisplayFlag,Topic_Sort_Order,Topic_report,Topic_newsletter,Topic_annual)" +
                                         " VALUES (@Topic_ID,@Topic_Name_TW,@Topic_Intro_TW,@Topic_DisplayFlag,@Topic_Sort_Order,@Topic_report,@Topic_newsletter,@Topic_annual);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@Topic_ID", my_Topic_Tw.Topic_ID);
                                createMySqlCommand.Parameters.AddWithValue("@Topic_Name_TW", my_Topic_Tw.Topic_Name_TW);
                                createMySqlCommand.Parameters.AddWithValue("@Topic_Intro_TW", my_Topic_Tw.Topic_Intro_TW);
                                createMySqlCommand.Parameters.AddWithValue("@Topic_DisplayFlag", my_Topic_Tw.Topic_DisplayFlag);
                                createMySqlCommand.Parameters.AddWithValue("@Topic_Sort_Order", my_Topic_Tw.Topic_Sort_Order);
                                createMySqlCommand.Parameters.AddWithValue("@Topic_report", my_Topic_Tw.Topic_report);
                                createMySqlCommand.Parameters.AddWithValue("@Topic_newsletter", my_Topic_Tw.Topic_newsletter);
                                createMySqlCommand.Parameters.AddWithValue("@Topic_annual", my_Topic_Tw.Topic_annual);
                                createMySqlCommand.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("新增資料庫資料異常: " + ex.Message);
                        }
                    }
                }
                Db_Cn_SqlConnection.Close();


                //寫入Db_Tw資料進.txt檔案
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/HashData"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/HashData");
                }
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Topic_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Topic_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Topic_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Topic_ID + ";" + a.Topic_Name_TW + ";" + a.Topic_Intro_TW;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_topic_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }
    }
}
