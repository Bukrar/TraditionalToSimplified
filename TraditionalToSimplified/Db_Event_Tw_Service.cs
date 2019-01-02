using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace TraditionalToSimplified
{
    public class Db_Event_Tw_Service
    {
        Utility utility = new Utility();
        public void UpdateTableMy_Event_Category_Tw()
        {
            Console.WriteLine("處理DB_EVENT_TW資料庫 資料表:my_event_category_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Event_Category_Tw> twList = new List<Model.My_Event_Category_Tw>();
            List<string> decodeLastDataList = new List<string>();
            List<string> decodeTwDbDataList = new List<string>();
            List<string> cnDbPkList = new List<string>();
            List<string> twDbPkList = new List<string>();
            List<string> needUpdateData = new List<string>();
            List<string> needDeleteData = new List<string>();
            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:1:connectString").Value);
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
                    new MySqlCommand("SELECT Code,Name FROM DB_EVENT_TW.my_event_category_tw", Db_Tw_SqlConnection))
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
                                Model.My_Event_Category_Tw modelData = new Model.My_Event_Category_Tw();
                                modelData.Code = pk;
                                modelData.Name = simplifiedEncodeContent;
                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.Code + ";" + modelData.Name;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_category_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
                            }
                        }
                    }
                }
                Db_Tw_SqlConnection.Close();

                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:3:connectString").Value);
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
                  new MySqlCommand("SELECT Code,Name FROM DB_EVENT_CN.my_event_category_tw", Db_Cn_SqlConnection))
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
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Event_Category_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Event_Category_Tw.txt");
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
                       new MySqlCommand("DELETE From " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_event_category_tw").Key +
                                        " WHERE " + configuration.GetSection("db:3:tables:my_event_category_tw:0").Value +
                                        "= @Code ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@Code", mustBeDelete);
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
                           new MySqlCommand("update " + configuration.GetSection("db:3:dbname").Value +
                                            "." + configuration.GetSection("db:3:tables:my_event_category_tw").Key +
                                            " set " + configuration.GetSection("db:3:tables:my_event_category_tw:1").Value +
                                            " = @Name WHERE " + configuration.GetSection("db:3:tables:my_event_category_tw:0").Value +
                                            " = @Code ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@Code", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@Name", dataArray[1]);
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
                            Model.My_Event_Category_Tw my_Event_Category_Tw = new Model.My_Event_Category_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_EVENT_TW.my_event_category_tw WHERE " +
                                                configuration.GetSection("db:3:tables:my_event_category_tw:0").Value +
                                                " = @Code", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@Code", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_Event_Category_Tw.Code = myData.GetString(0);
                                    my_Event_Category_Tw.Name = utility.Big5ToGb18030(myData.GetString(1));
                                    my_Event_Category_Tw.File = myData.GetString(2);
                                    my_Event_Category_Tw.Sort = myData.GetInt32(3);
                                    my_Event_Category_Tw.Count = myData.GetInt32(4);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_event_category_tw").Key +
                                         "(Code,Name,File,Sort,Count)" +
                                         "VALUES (@Code,@Name,@File,@Sort,@Count);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@Code", my_Event_Category_Tw.Code);
                                createMySqlCommand.Parameters.AddWithValue("@Name", my_Event_Category_Tw.Name);
                                createMySqlCommand.Parameters.AddWithValue("@File", my_Event_Category_Tw.File);
                                createMySqlCommand.Parameters.AddWithValue("@Sort", my_Event_Category_Tw.Sort);
                                createMySqlCommand.Parameters.AddWithValue("@Count", my_Event_Category_Tw.Count);
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
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Event_Category_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Event_Category_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Event_Category_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Code + ";" + a.Name;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("DB_EVENT_TW資料庫 資料表:my_event_category_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Event_Press_Tw()
        {
            Console.WriteLine("處理DB_EVENT_TW資料庫 資料表:my_event_press_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Event_Press_Tw> twList = new List<Model.My_Event_Press_Tw>();
            List<string> decodeLastDataList = new List<string>();
            List<string> decodeTwDbDataList = new List<string>();
            List<int> cnDbPkList = new List<int>();
            List<int> twDbPkList = new List<int>();
            List<string> needUpdateData = new List<string>();
            List<int> needDeleteData = new List<int>();
            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:1:connectString").Value);
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
                    new MySqlCommand("SELECT Press_SEQ,Press_Title,Press_Content FROM DB_EVENT_TW.my_event_press_tw", Db_Tw_SqlConnection))
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
                            {
                                Model.My_Event_Press_Tw modelData = new Model.My_Event_Press_Tw();
                                int pk = myData.GetInt32(0);
                                modelData.Press_SEQ = pk;
                                //PK 存入twDbPkList
                                int datapk = myData.GetInt32(0);
                                twDbPkList.Add(datapk);
                                if (!myData.IsDBNull(1))
                                {
                                    string simplifiedEncodePressTitle = utility.Big5ToGb18030(myData.GetString(1));
                                    modelData.Press_Title = simplifiedEncodePressTitle;
                                }
                                else
                                {
                                    Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_press_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
                                }
                                if (!myData.IsDBNull(2))
                                {
                                    string simplifiedEncodePressContent = utility.Big5ToGb18030(myData.GetString(2));
                                    modelData.Press_Content = simplifiedEncodePressContent;
                                }
                                else
                                {
                                    Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_press_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
                                }

                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.Press_SEQ + ";" + modelData.Press_Title + ";" + modelData.Press_Content;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);
                            }
                        }

                    }
                }
                Db_Tw_SqlConnection.Close();

                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:3:connectString").Value);
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
                  new MySqlCommand("SELECT Press_SEQ,Press_Title,Press_Content FROM DB_EVENT_CN.my_event_press_tw", Db_Cn_SqlConnection))
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
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Event_Press_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Event_Press_Tw.txt");
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
                       new MySqlCommand("DELETE From " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_event_press_tw").Key +
                                        " WHERE " + configuration.GetSection("db:3:tables:my_event_press_tw:0").Value +
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
                           new MySqlCommand("update " + configuration.GetSection("db:3:dbname").Value +
                                            "." + configuration.GetSection("db:3:tables:my_event_press_tw").Key +
                                            " set " + configuration.GetSection("db:3:tables:my_event_press_tw:1").Value +
                                            " = @Press_Title ," + configuration.GetSection("db:3:tables:my_event_press_tw:2").Value +
                                            " = @Press_Content WHERE " + configuration.GetSection("db:3:tables:my_event_press_tw:0").Value +
                                            " = @Press_SEQ ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@Press_SEQ", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@Press_Title", dataArray[1]);
                                updatMySqlCommand.Parameters.AddWithValue("@Press_Content", dataArray[2]);
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
                            Model.My_Event_Press_Tw my_Event_Press_Tw = new Model.My_Event_Press_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_EVENT_TW.my_event_press_tw WHERE " +
                                                configuration.GetSection("db:3:tables:my_event_press_tw:0").Value +
                                                " = @Press_SEQ", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@Press_SEQ", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_Event_Press_Tw.Press_SEQ = myData.GetInt32(0);
                                    my_Event_Press_Tw.Press_Type = utility.Big5ToGb18030(myData.GetString(1));
                                    my_Event_Press_Tw.Press_Date = myData.GetDateTime(2);
                                    my_Event_Press_Tw.Press_File = myData.GetString(3);
                                    my_Event_Press_Tw.Press_Title = utility.Big5ToGb18030(myData.GetString(4));
                                    if (!myData.IsDBNull(5))
                                    {
                                        my_Event_Press_Tw.Press_Content = utility.Big5ToGb18030(myData.GetString(5));
                                    }
                                    my_Event_Press_Tw.Press_On = myData.GetInt32(6);
                                    if (!myData.IsDBNull(7))
                                    {
                                        my_Event_Press_Tw.Press_Prod_ID = myData.GetInt32(7);
                                    }
                                    my_Event_Press_Tw.Press_Prod_Title = myData.GetString(8);
                                    my_Event_Press_Tw.Press_Prod_Category = myData.GetString(9);
                                    my_Event_Press_Tw.Press_Prod_Discon = myData.GetInt32(10);
                                    my_Event_Press_Tw.Press_Pub_Code = myData.GetString(11);
                                    my_Event_Press_Tw.Press_Pub_Name = myData.GetString(12);
                                    my_Event_Press_Tw.Event_Start_Date = myData.GetDateTime(13);
                                    my_Event_Press_Tw.Event_End_Date = myData.GetDateTime(14);
                                    if (!myData.IsDBNull(15))
                                    {
                                        my_Event_Press_Tw.Event_Venue = myData.GetString(15);
                                    }
                                    if (!myData.IsDBNull(16))
                                    {
                                        my_Event_Press_Tw.Event_Country = myData.GetString(16);
                                    }
                                    my_Event_Press_Tw.Prod_WEB_OnOff = myData.GetInt32(17);
                                    my_Event_Press_Tw.Prod_WEB_URL = myData.GetString(18);
                                    my_Event_Press_Tw.Prod_WEB_JP = myData.GetInt32(19);
                                    my_Event_Press_Tw.Prod_WEB_EN = myData.GetInt32(20);
                                    my_Event_Press_Tw.Prod_WEB_TW = myData.GetInt32(21);
                                    my_Event_Press_Tw.Prod_WEB_KR = myData.GetInt32(22);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_event_press_tw").Key +
                                         "(Press_SEQ,Press_Type,Press_Date,Press_File,Press_Title,Press_Content,Press_On,Press_Prod_ID,Press_Prod_Title,Press_Prod_Category,Press_Prod_Discon,Press_Pub_Code,Press_Pub_Name,Event_Start_Date,Event_End_Date,Event_Venue,Event_Country,Prod_WEB_OnOff,Prod_WEB_URL,Prod_WEB_JP,Prod_WEB_EN,Prod_WEB_TW,Prod_WEB_KR)" +
                                         "VALUES (@Press_SEQ,@Press_Type,@Press_Date,@Press_File,@Press_Title,@Press_Content,@Press_On,@Press_Prod_ID,@Press_Prod_Title,@Press_Prod_Category,@Press_Prod_Discon,@Press_Pub_Code,@Press_Pub_Name,@Event_Start_Date,@Event_End_Date,@Event_Venue,@Event_Country,@Prod_WEB_OnOff,@Prod_WEB_URL,@Prod_WEB_JP,@Prod_WEB_EN,@Prod_WEB_TW,@Prod_WEB_KR);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@Press_SEQ", my_Event_Press_Tw.Press_SEQ);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Type", my_Event_Press_Tw.Press_Type);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Date", my_Event_Press_Tw.Press_Date);
                                createMySqlCommand.Parameters.AddWithValue("@Press_File", my_Event_Press_Tw.Press_File);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Title", my_Event_Press_Tw.Press_Title);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Content", my_Event_Press_Tw.Press_Content);
                                createMySqlCommand.Parameters.AddWithValue("@Press_On", my_Event_Press_Tw.Press_On);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Prod_ID", my_Event_Press_Tw.Press_Prod_ID);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Prod_Title", my_Event_Press_Tw.Press_Prod_Title);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Prod_Category", my_Event_Press_Tw.Press_Prod_Category);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Prod_Discon", my_Event_Press_Tw.Press_Prod_Discon);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Pub_Code", my_Event_Press_Tw.Press_Pub_Code);
                                createMySqlCommand.Parameters.AddWithValue("@Press_Pub_Name", my_Event_Press_Tw.Press_Pub_Name);
                                createMySqlCommand.Parameters.AddWithValue("@Event_Start_Date", my_Event_Press_Tw.Event_Start_Date);
                                createMySqlCommand.Parameters.AddWithValue("@Event_End_Date", my_Event_Press_Tw.Event_End_Date);
                                createMySqlCommand.Parameters.AddWithValue("@Event_Venue", my_Event_Press_Tw.Event_Venue);
                                createMySqlCommand.Parameters.AddWithValue("@Event_Country", my_Event_Press_Tw.Event_Country);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_WEB_OnOff", my_Event_Press_Tw.Prod_WEB_OnOff);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_WEB_URL", my_Event_Press_Tw.Prod_WEB_URL);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_WEB_JP", my_Event_Press_Tw.Prod_WEB_JP);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_WEB_EN", my_Event_Press_Tw.Prod_WEB_EN);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_WEB_TW", my_Event_Press_Tw.Prod_WEB_TW);
                                createMySqlCommand.Parameters.AddWithValue("@Prod_WEB_KR", my_Event_Press_Tw.Prod_WEB_KR);     
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
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Event_Press_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Event_Press_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Event_Press_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Press_SEQ + ";" + a.Press_Title + ";" + a.Press_Content;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("DB_EVENT_TW資料庫 資料表:my_event_press_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Event_Region_Tw()
        {
            Console.WriteLine("處理DB_EVENT_TW資料庫 資料表:my_event_region_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Event_Region_Tw> twList = new List<Model.My_Event_Region_Tw>();
            List<string> decodeLastDataList = new List<string>();
            List<string> decodeTwDbDataList = new List<string>();
            List<string> cnDbPkList = new List<string>();
            List<string> twDbPkList = new List<string>();
            List<string> needUpdateData = new List<string>();
            List<string> needDeleteData = new List<string>();
            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:1:connectString").Value);
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
                    new MySqlCommand("SELECT Code,Name FROM DB_EVENT_TW.my_event_region_tw", Db_Tw_SqlConnection))
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
                                Model.My_Event_Region_Tw modelData = new Model.My_Event_Region_Tw();
                                modelData.Code = pk;
                                modelData.Name = simplifiedEncodeContent;
                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.Code + ";" + modelData.Name;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_region_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
                            }
                        }

                    }
                }
                Db_Tw_SqlConnection.Close();

                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:3:connectString").Value);
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
                  new MySqlCommand("SELECT Code,Name FROM DB_EVENT_CN.my_event_region_tw", Db_Cn_SqlConnection))
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
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Event_Region_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Event_Region_Tw.txt");
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
                       new MySqlCommand("DELETE From " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_event_region_tw").Key +
                                        " WHERE " + configuration.GetSection("db:3:tables:my_event_region_tw:0").Value +
                                        "= @Code ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@Code", mustBeDelete);
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
                           new MySqlCommand("update " + configuration.GetSection("db:3:dbname").Value +
                                            "." + configuration.GetSection("db:3:tables:my_event_region_tw").Key +
                                            " set " + configuration.GetSection("db:3:tables:my_event_region_tw:1").Value +
                                            " = @Name WHERE " + configuration.GetSection("db:3:tables:my_event_region_tw:0").Value +
                                            " = @Code ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@Code", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@Name", dataArray[1]);
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
                            Model.My_Event_Region_Tw my_Event_Region_Tw = new Model.My_Event_Region_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_EVENT_TW.my_event_region_tw WHERE " +
                                                configuration.GetSection("db:3:tables:my_event_region_tw:0").Value +
                                                " = @Code", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@Code", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_Event_Region_Tw.Code = myData.GetString(0);
                                    my_Event_Region_Tw.Name = utility.Big5ToGb18030(myData.GetString(1));
                                    my_Event_Region_Tw.Count = myData.GetInt32(2);
                                    my_Event_Region_Tw.PCount = myData.GetInt32(3);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_event_region_tw").Key +
                                         "(Code,Name,Count,PCount)" +
                                         "VALUES (@Code,@Name,@Count,@PCount);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@Code", my_Event_Region_Tw.Code);
                                createMySqlCommand.Parameters.AddWithValue("@Name", my_Event_Region_Tw.Name);
                                createMySqlCommand.Parameters.AddWithValue("@Count", my_Event_Region_Tw.Count);
                                createMySqlCommand.Parameters.AddWithValue("@PCount", my_Event_Region_Tw.PCount);
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
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Event_Region_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Event_Region_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Event_Region_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Code + ";" + a.Name;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("DB_EVENT_TW資料庫 資料表:my_event_region_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Partnar_Category_Tw()
        {
            Console.WriteLine("處理DB_EVENT_TW資料庫 資料表:my_partnar_category_tw...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Partnar_Category_Tw> twList = new List<Model.My_Partnar_Category_Tw>();
            List<string> decodeLastDataList = new List<string>();
            List<string> decodeTwDbDataList = new List<string>();
            List<string> cnDbPkList = new List<string>();
            List<string> twDbPkList = new List<string>();
            List<string> needUpdateData = new List<string>();
            List<string> needDeleteData = new List<string>();
            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:1:connectString").Value);
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
                    new MySqlCommand("SELECT Code,Name,Title FROM DB_EVENT_TW.my_partnar_category_tw", Db_Tw_SqlConnection))
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
                            {
                                Model.My_Partnar_Category_Tw modelData = new Model.My_Partnar_Category_Tw();
                                string pk = myData.GetString(0);
                                modelData.Code = pk;
                                //PK 存入twDbPkList
                                string datapk = myData.GetString(0);
                                twDbPkList.Add(datapk);
                                if (!myData.IsDBNull(1))
                                {
                                    string simplifiedEncodeName = utility.Big5ToGb18030(myData.GetString(1));
                                    modelData.Name = simplifiedEncodeName;
                                }
                                else
                                {
                                    Console.WriteLine("DB:DB_EVENT_TW TABLE: my_partnar_category_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
                                }
                                if (!myData.IsDBNull(2))
                                {
                                    string simplifiedEncodePressTitle = utility.Big5ToGb18030(myData.GetString(2));
                                    modelData.Title = simplifiedEncodePressTitle;
                                }
                                else
                                {
                                    Console.WriteLine("DB:DB_EVENT_TW TABLE: my_partnar_category_tw 中欄位值為NULL 資料PK為: " + myData.GetString(0));
                                }

                                twList.Add(modelData);

                                //加密存進decodeDbDataList
                                string data = modelData.Code + ";" + modelData.Name + ";" + modelData.Title;
                                string encodeData = utility.Encode(data);
                                decodeTwDbDataList.Add(encodeData);
                            }
                        }

                    }
                }
                Db_Tw_SqlConnection.Close();

                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:3:connectString").Value);
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
                  new MySqlCommand("SELECT Code,Name,Title FROM DB_EVENT_CN.my_partnar_category_tw", Db_Cn_SqlConnection))
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
                if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Partnar_Category_Tw.txt"))
                {
                    StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/My_Partnar_Category_Tw.txt");
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
                       new MySqlCommand("DELETE From " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_partnar_category_tw").Key +
                                        " WHERE " + configuration.GetSection("db:3:tables:my_partnar_category_tw:0").Value +
                                        "= @Code ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@Code", mustBeDelete);
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
                           new MySqlCommand("update " + configuration.GetSection("db:3:dbname").Value +
                                            "." + configuration.GetSection("db:3:tables:my_partnar_category_tw").Key +
                                            " set " + configuration.GetSection("db:3:tables:my_partnar_category_tw:1").Value +
                                            " = @Name ," + configuration.GetSection("db:3:tables:my_partnar_category_tw:2").Value +
                                            " = @Title WHERE " + configuration.GetSection("db:3:tables:my_partnar_category_tw:0").Value +
                                            " = @Code ", Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@Code", dataArray[0]);
                                updatMySqlCommand.Parameters.AddWithValue("@Name", dataArray[1]);
                                updatMySqlCommand.Parameters.AddWithValue("@Title", dataArray[2]);
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
                            Model.My_Partnar_Category_Tw my_Partnar_Category_Tw = new Model.My_Partnar_Category_Tw();
                            Db_Tw_SqlConnection.Open();
                            using (MySqlCommand mySqlCommand =
                               new MySqlCommand("SELECT * FROM DB_EVENT_TW.my_partnar_category_tw WHERE " +
                                                configuration.GetSection("db:3:tables:my_partnar_category_tw:0").Value +
                                                " = @Code", Db_Cn_SqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@Code", dataArray[0]);
                                MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                while (myData.Read())
                                {
                                    my_Partnar_Category_Tw.Code = myData.GetString(0);
                                    my_Partnar_Category_Tw.Name = utility.Big5ToGb18030(myData.GetString(1));
                                    my_Partnar_Category_Tw.Title = utility.Big5ToGb18030(myData.GetString(2));
                                    my_Partnar_Category_Tw.File = myData.GetString(3);
                                    my_Partnar_Category_Tw.WCon = myData.GetString(4);
                                    my_Partnar_Category_Tw.Count = myData.GetInt32(5);
                                    my_Partnar_Category_Tw.Sort = myData.GetInt32(6);
                                }
                            }
                            Db_Tw_SqlConnection.Close();

                            using (MySqlCommand createMySqlCommand =
                                  new MySqlCommand("INSERT INTO " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_partnar_category_tw").Key +
                                         "(Code,Name,Title,File,WCon,Count,Sort)" +
                                         "VALUES (@Code,@Name,@Title,@File,@WCon,@Count,@Sort);"
                                         , Db_Cn_SqlConnection))
                            {
                                createMySqlCommand.Parameters.AddWithValue("@Code", my_Partnar_Category_Tw.Code);
                                createMySqlCommand.Parameters.AddWithValue("@Name", my_Partnar_Category_Tw.Name);
                                createMySqlCommand.Parameters.AddWithValue("@Title", my_Partnar_Category_Tw.Title);
                                createMySqlCommand.Parameters.AddWithValue("@File", my_Partnar_Category_Tw.File);
                                createMySqlCommand.Parameters.AddWithValue("@WCon", my_Partnar_Category_Tw.WCon);
                                createMySqlCommand.Parameters.AddWithValue("@Count", my_Partnar_Category_Tw.Count);
                                createMySqlCommand.Parameters.AddWithValue("@Sort", my_Partnar_Category_Tw.Sort);
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
                if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/My_Partnar_Category_Tw.txt"))
                {
                    FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/My_Partnar_Category_Tw.txt");
                    fs.Close();
                }
                StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/My_Partnar_Category_Tw.txt");
                foreach (var a in twList)
                {
                    string data = a.Code + ";" + a.Name + ";" + a.Title;
                    string encodeData = utility.Encode(data);
                    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("DB_EVENT_TW資料庫 資料表:my_partnar_category_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }
    }
}
