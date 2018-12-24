using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace TraditionalToSimplified
{
    public class DataService
    {
        public void UpdateDataToSim(MySqlConnection mySqlConnection)
        {
            //呼叫json資源
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            //建構子
            Utility utility = new Utility();
            List<Model.my_category_tw> modelList = new List<Model.my_category_tw>();

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                mySqlConnection.Open();
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT " + configuration.GetSection("tables:my_category_tw:0").Value
                                               + "," + configuration.GetSection("tables:my_category_tw:1").Value
                                               + " FROM " + configuration.GetSection("dbname").Value + "." +
                                               configuration.GetSection("tables:my_category_tw").Key, mySqlConnection))
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
                            string traditionalContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                            string simplifiedContent = utility.ToSimplified(traditionalContent, "ToSimplified");
                            string simplifiedEncode = HttpUtility.UrlEncode(simplifiedContent, Encoding.GetEncoding("gb18030"));

                            Model.my_category_tw modelData = new Model.my_category_tw();
                            modelData.Category_ID = pk;
                            modelData.Category_Name_TW = simplifiedEncode;
                            modelList.Add(modelData);
                        }

                    }
                }

                foreach (var s in modelList)
                    using (MySqlCommand UPDATmySqlCommand =
                        new MySqlCommand("update db_tw.my_category_tw set Category_Name_TW='" + s.Category_Name_TW +
                                         "' WHERE Category_ID ='" + s.Category_ID + "'", mySqlConnection))
                    {
                        UPDATmySqlCommand.ExecuteNonQuery();
                        Console.WriteLine("處理KEY: " + s.Category_ID + " : " + s.Category_Name_TW);
                    }

                mySqlConnection.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }
    }
}
