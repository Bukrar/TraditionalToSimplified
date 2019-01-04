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
            Console.WriteLine("處理Db_Tw資料庫");
            //呼叫json資源
            var configuration = Utility.GetJson();
            var jsonDbArray = configuration.GetSection("db").GetChildren();
            int dbCount = jsonDbArray.Count();

            int tWDbCount = 0;
            int cnDbCount = 1;
            int tablecount = 0;

            for (int z = 0; z < dbCount; z = z + 2)
            {
                var jsonTableArray = configuration.GetSection("db:" + z).GetChildren();
                int tableCount = jsonTableArray.Count();
                for (int y = 0; y < tableCount; y = y + 1)
                {
                    //int ss = aa.Length;
                    // 讀取TWDB資料
                    string[] twDataArray = configuration.GetSection("db:" + tWDbCount + ":tables:" + tablecount).Value.Split(',');
                    int selectNumber = twDataArray.Length - 1;
                    string twTablePkName = twDataArray[1];
                    string twDbConnection = configuration.GetSection("db:" + tWDbCount + ":connectString").Value;
                    string twTableName = twDataArray[0];
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
                    string cnTableName = cnDataArray[0];
                    string cnDbName = configuration.GetSection("db:" + cnDbCount + ":dbname").Value;
                    string cnSelectTable = "";
                    string updateTableQuery = "";
                    bool isfirst = true;
                    for (int i = 1; i < cnDataArray.Length; i++)
                    {
                        if (i == 1)
                        {
                            cnTablePkName = cnDataArray[i];
                        }

                        if (i != 1)
                        {
                            if (isfirst)
                            {
                                updateTableQuery += cnDataArray[i] + " = @" + cnDataArray[i] + " ";
                                isfirst = false;
                            }
                            else
                            {
                                updateTableQuery += "," + cnDataArray[i] + " = @" + cnDataArray[i] + " ";
                            }
                        }
                        cnSelectTable += cnDataArray[i] + ",";

                    }
                    updateTableQuery = updateTableQuery.TrimEnd(',');
                    cnSelectTable = cnSelectTable.TrimEnd(',');
                    string[] updateTable = cnSelectTable.Split(',');

                    List<string> twList = new List<string>();
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
                                    string Dbdata = "";
                                    //PK 存入twDbPkList
                                    for (int x = 0; x < selectNumber; x++)
                                    {
                                        //JSON TABLE欄位設定為第一個一定是PK
                                        if (x == 0)
                                        {
                                            twDbPkList.Add(myData.GetValue(x).ToString());
                                            Dbdata += myData.GetValue(x) + ";";

                                        }
                                        else
                                        {
                                            //有可能需要編碼的欄位為NULL
                                            if (myData.GetValue(x).ToString() == "")
                                            {
                                                Dbdata += " ;";
                                            }
                                            else if (!myData.IsDBNull(x))
                                            {
                                                Dbdata += utility.Big5ToGb18030(myData.GetValue(x).ToString()) + ";";
                                            }
                                            else
                                            {
                                                Dbdata += " ;";
                                            }
                                        }
                                    }

                                    Dbdata = Dbdata.TrimEnd(';');
                                    twList.Add(Dbdata);
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
                        if (File.Exists(Directory.GetCurrentDirectory() + "/HashData/"+ twTableName + ".txt"))
                        {
                            StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "/HashData/" + twTableName + ".txt");
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
                                string deleteSql = "DELETE From " + cnDbName +
                                                     "." + cnTableName +
                                                    " WHERE " + cnTablePkName +
                                                    " = @" + cnTablePkName;
                                deleteSql = deleteSql.Replace("@" + cnTablePkName, "'" + mustBeDelete + "'");
                                using (MySqlCommand deleteMySqlCommand =
                                   new MySqlCommand(deleteSql, Db_Cn_SqlConnection))
                                {
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
                                    string updateSql = "update " + cnDbName + "." + cnTableName +
                                               " set " + updateTableQuery +
                                               "WHERE " + cnTablePkName + " = @" + cnTablePkName;
                                    string[] updateField = cnSelectTable.Split(',');
                                    for (int x = 0; x < updateField.Length; x++)
                                    {
                                        updateSql = updateSql.Replace("@" + updateField[x], "'" + dataArray[x] + "'");
                                    }

                                    using (
                                    MySqlCommand updatMySqlCommand =
                               new MySqlCommand(updateSql, Db_Cn_SqlConnection))
                                    {
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
                                    Db_Tw_SqlConnection.Open();
                                    string tableRow = "";
                                    using (MySqlCommand mySqlCommand =
                                     new MySqlCommand("DESC " + twDbName + "." + twTableName, Db_Cn_SqlConnection))
                                    {
                                        MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                        while (myData.Read())
                                        {
                                            tableRow += myData.GetString(0) + ",";
                                        }
                                        tableRow = tableRow.TrimEnd(',');
                                    }

                                    string selectSql = "SELECT * FROM " + twDbName + "." + twTableName + " WHERE " +
                                                         twTablePkName + " = @" + twTablePkName;
                                    selectSql = selectSql.Replace("@" + twTablePkName, "'" + dataArray[0] + "'");
                                    string twDbData = "";
                                    string[] tableRowArray = tableRow.Split(',');
                                    using (MySqlCommand mySqlCommand =
                                       new MySqlCommand(selectSql, Db_Cn_SqlConnection))
                                    {
                                        MySqlDataReader myData = mySqlCommand.ExecuteReader();
                                        while (myData.Read())
                                        {
                                            for (int x = 0; x < tableRowArray.Length; x++)
                                            {
                                                twDbData += myData.GetValue(x).ToString() + ",";
                                            }
                                        }
                                        twDbData = twDbData.TrimEnd(',');
                                    }
                                    Db_Tw_SqlConnection.Close();

                                    string insertTableQuery = "";
                                    for (int x = 0; x < tableRowArray.Length; x++)
                                    {
                                        insertTableQuery += "@" + tableRowArray[x] + ",";
                                    }
                                    insertTableQuery = insertTableQuery.TrimEnd(',');
                                    string insertSql = "INSERT INTO " + cnDbName + "." + cnTableName +
                                                 "(" + tableRow + ") VALUES (" + insertTableQuery + ")";

                                    string[] insertDataArray = twDbData.Split(',');
                                    for (int x = 0; x < insertDataArray.Length; x++)
                                    {
                                        insertSql = insertSql.Replace("@" + tableRowArray[x], "'" + insertDataArray[x] + "'");
                                    }
                                    using (MySqlCommand createMySqlCommand =
                                          new MySqlCommand(insertSql, Db_Cn_SqlConnection))
                                    {
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
                        if (!File.Exists(Directory.GetCurrentDirectory() + "/HashData/" + twTableName + ".txt"))
                        {
                            FileStream fs = File.Create(Directory.GetCurrentDirectory() + "/HashData/" + twTableName + ".txt");
                            fs.Close();
                        }
                        StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "/HashData/" + twTableName + ".txt");
                        foreach (var a in twList)
                        {
                            string encodeData = utility.Encode(a);

                            sw.WriteLine(encodeData);
                        }
                        sw.Close();
                        Console.WriteLine("Db_Tw資料庫 資料表:" + twTableName + " 資料處理完成");
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
                    }

                    tablecount = tablecount + 1;
                }
                cnDbCount = cnDbCount + 2;
                tWDbCount = tWDbCount + 2;
            }
        }
    }
}
