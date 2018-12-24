using Microsoft.Extensions.Configuration;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;

namespace TraditionalToSimplified
{
    class Program
    {
        static void Main(string[] args)
        {
            Utility utility = new Utility();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            string connectString = "server=127.0.0.1;uid=root;pwd=H1242457411qaz;database=db_tw";
            string showTableSql = "SHOW TABLES;";
            string sqlSelectSql = "SELECT * FROM db_tw.my_product_tw;";
            string descSql = "DESC db_tw.my_product_tw;";
            StreamWriter sw = new StreamWriter(@"D:\BBBBBBBBBBBBBBBBB.txt");
            List<string> dblist = new List<string>();
            List<string> isvarchar = new List<string>();
            List<int> listInt = new List<int>();

            List<Model.product> list = new List<Model.product>();

            MySqlConnection mySqlConnection = new MySqlConnection(connectString);

            // 連線到資料庫
            utility.StartDb();

            
           

            try
            {
                using (MySqlCommand mySqlcommand = new MySqlCommand(showTableSql, mySqlConnection))
                {
                    MySqlDataReader myData = mySqlcommand.ExecuteReader();

                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {
                        while (myData.Read())
                        {
                            dblist.Add(myData.GetString(0));
                        }
                    }
                }
                mySqlConnection.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
                Console.ReadLine();
            }

            //
            try
            {
                mySqlConnection.Open();
                using (MySqlCommand mySqlCommand = new MySqlCommand(descSql, mySqlConnection))
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
                            isvarchar.Add(myData.GetString(1));
                        }
                    }
                }
                mySqlConnection.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
                Console.ReadLine();
            }

            //
            try
            {
                mySqlConnection.Open();
                using (MySqlCommand mySqlCommand = new MySqlCommand(sqlSelectSql, mySqlConnection))
                {
                    MySqlDataReader myData = mySqlCommand.ExecuteReader();
                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {

                        for (int x = 0; x < isvarchar.Count; x++)
                        {
                            if (isvarchar[x].Contains("varchar"))
                            {
                                listInt.Add(x);
                            }
                        }
                        int count = 0;
                        while (myData.Read())
                        {
                            var countNO = myData.GetInt32(0);
                            var temp = myData.GetValue(3);
                            string traditionalContent = HttpUtility.UrlDecode(temp.ToString(), Encoding.GetEncoding("big5"));
                            // Console.WriteLine(traditionalContent);
                           // sw.WriteLine(traditionalContent);
                            string simplifiedContent = utility.ToSimplified(traditionalContent, "ToSimplified");
                            // Console.WriteLine(simplifiedContent);
                            //sw.WriteLine(simplifiedContent + count.ToString());
                            string simplifiedEncode = HttpUtility.UrlEncode(simplifiedContent, Encoding.GetEncoding("GB18030"));

                            //先放這邊 改名字
                            Model.product listsave = new Model.product();
                            listsave.Id = countNO;
                            listsave.ch = simplifiedEncode;
                            list.Add(listsave);
                        }
                       
                    }
                }
                int NONO = 0;
                foreach (var s in list)
                    using (MySqlCommand UPDATmySqlCommand = new MySqlCommand("update db_tw.my_product_tw set Prod_Title_TW='" + s.ch + "' WHERE Prod_ID =" + s.Id, mySqlConnection))
                    {
                        NONO = NONO + 1;
                        UPDATmySqlCommand.ExecuteNonQuery();
                        sw.WriteLine("流水號:"+ NONO + "  "+s.Id+ ":"+ s.ch);
                    }

                mySqlConnection.Close();
            }
            catch (MySqlException ex) {
                sw.WriteLine(ex);
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
           ts.Hours, ts.Minutes, ts.Seconds,
           ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            sw.Close();
        }
    }
}
//string traditionalContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
//Console.WriteLine(traditionalContent);
//sw.WriteLine(traditionalContent);
//string simplifiedContent = Utility.ToSimplified(traditionalContent, "ToSimplified");
//Console.WriteLine(simplifiedContent);
//sw.WriteLine(simplifiedContent);

