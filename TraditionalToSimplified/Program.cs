using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Text;
using System.Web;

namespace TraditionalToSimplified
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectString = "server=127.0.0.1;uid=root;pwd=H1242457411qaz;database=db_event_tw";
            string showTableSql = "SHOW TABLES;";
            string sqlSelectSql = "SELECT * FROM db_tw.my_country_tw;";

            MySqlConnection mySqlConnection = new MySqlConnection(connectString);

            // 連線到資料庫
            try
            {
                mySqlConnection.Open();
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

            try
            {
                using (MySqlCommand mySqlcommand = new MySqlCommand(showTableSql, mySqlConnection))
                {
                    StreamWriter sw = new StreamWriter(@"D:\secret_plan.txt");
                    MySqlDataReader myData = mySqlcommand.ExecuteReader();

                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {
                        while (myData.Read())
                        {
                            string traditionalContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                            Console.WriteLine(traditionalContent);
                            sw.WriteLine(traditionalContent);
                            string simplifiedContent = Utility.ToSimplified(traditionalContent, "ToSimplified");
                            Console.WriteLine(simplifiedContent);
                            sw.WriteLine(simplifiedContent);
                            
                        }
                        sw.Close();
                        myData.Close();
                    }
                    Console.ReadLine();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
                Console.ReadLine();
            }
        }
    }
}


