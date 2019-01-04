using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TraditionalToSimplified
{
    public class TESTDB
    {
        Utility utility = new Utility();
        public void ch()
        {
            int tWDbCount = 0;
            int cnDbCount = 1;
            int tablecount = 0;
            Console.WriteLine("處理Db_Tw資料庫");
            //呼叫json資源
            var configuration = Utility.GetJson();
            // 讀取TWDB資料
            string[] twDataArray = configuration.GetSection("db:" + tWDbCount + ":tables:" + tablecount).Value.Split(',');
            int selectNumber = twDataArray.Length - 1;
            string twDbConnection = configuration.GetSection("db:" + tWDbCount + ":connectString").Value;
            string twTableName = twDataArray[tablecount];
            string twDbName = configuration.GetSection("db:" + tWDbCount + ":dbname").Value;
            string twSelectTable = "";
            for (int i = 1; i < twDataArray.Length; i++)
            {
                twSelectTable += twDataArray[i] + ",";
            }
            twSelectTable = twSelectTable.TrimEnd(',');

            // 讀取CNDB資料
            string[] cnDataArray = configuration.GetSection("db:" + cnDbCount + ":tables:" + tablecount).Value.Split(',');
            string cnDbConnection = configuration.GetSection("db:" + cnDbCount + ":connectString").Value;
            string cnTablePkName = "";
            string cnTableName = cnDataArray[tablecount];
            string cnDbName = configuration.GetSection("db:" + cnDbCount + ":dbname").Value;
            string cnSelectTable = "";
            for (int i = 1; i < cnDataArray.Length; i++)
            {
                if (i == 1)
                {
                    cnTablePkName = cnDataArray[i];
                }
                else
                {
                    cnSelectTable += cnDataArray[i] + ",";
                }
            }
            string updateSql = "update " + cnDbName + "." + cnTableName +
                                       " set " + cnSelectTable +
                                       " = @Category_Name_TW " +
                                       "WHERE " + cnTablePkName + " = @PkID ";
            cnSelectTable = cnSelectTable.TrimEnd(',');

            List<string> decodeLastDataList = new List<string>();
            List<string> decodeTwDbDataList = new List<string>();
            List<string> cnDbPkList = new List<string>();
            List<string> twDbPkList = new List<string>();
            List<string> needUpdateData = new List<string>();
            List<string> needDeleteData = new List<string>();
            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(twDbConnection);
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
                    new MySqlCommand("SELECT " + twSelectTable + " FROM "
                                               + twDbName + "."
                                               + twTableName, Db_Tw_SqlConnection))
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
                            string test = myData.GetString(1);

                            int count = 0;
                            string Dbdata = "";
                            //PK 存入twDbPkList
                            for (int x = 0; x < selectNumber; x++)
                            {
                                //JSON TABLE欄位設定為第一個一定是PK
                                if (count == 0)
                                {
                                    twDbPkList.Add(myData.GetValue(count).ToString());
                                    Dbdata += myData.GetValue(count) + ";";
                                    twDataArray[count] = Dbdata;
                                    count++;
                                }
                                else
                                {
                                    //有可能需要編碼的欄位為NULL
                                    if (myData.IsDBNull(count))
                                    {
                                        Dbdata += utility.Big5ToGb18030(myData.GetValue(count).ToString()) + ";";
                                        twDataArray[count] = Dbdata;
                                        count++;
                                    }
                                    else
                                    {
                                        Dbdata += "";
                                        twDataArray[count] = Dbdata;
                                        count++;
                                    }
                                }
                            }

                            Dbdata.TrimEnd(',');
                            string encodeData = utility.Encode(Dbdata);
                            decodeTwDbDataList.Add(encodeData);
                        }

                    }
                }

                Db_Tw_SqlConnection.Close();

                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(cnDbConnection);
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
                  new MySqlCommand("SELECT " + cnSelectTable + " FROM "
                                               + cnDbName + "."
                                               + cnTableName, Db_Cn_SqlConnection))
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
                           new MySqlCommand("DELETE From " + cnDbName +
                                             "." + cnTableName +
                                            " WHERE " + cnTablePkName +
                                            "= @PkID ", Db_Cn_SqlConnection))
                        {
                            deleteMySqlCommand.Parameters.AddWithValue("@PkID", mustBeDelete);
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
                       new MySqlCommand(, Db_Cn_SqlConnection))
                            {
                                updatMySqlCommand.Parameters.AddWithValue("@PkID", dataArray[0]);
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
                foreach (var a in twDataArray)
                {
                    //  string data = a.Category_ID + ";" + a.Category_Name_TW;
                    //   string encodeData = utility.Encode(data);
                    //    sw.WriteLine(encodeData);
                }
                sw.Close();
                Console.WriteLine("Db_Tw資料庫 資料表:my_category_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }
    }
}
