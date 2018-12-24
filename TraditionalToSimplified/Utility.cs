using Microsoft.Extensions.Configuration;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace TraditionalToSimplified
{
    public class Utility
    {
        //繁簡轉換Funtion,參數 Language 為 Big5 則轉繁體、GB2312 則轉簡體，其他狀況則輸出原字串
        public string ToSimplified(string SourceString, string Language)
        {
            string newString = string.Empty;
            switch (Language)
            {
                case "ToTraditional":
                    newString = ChineseConverter.Convert(SourceString, ChineseConversionDirection.SimplifiedToTraditional);
                    break;
                case "ToSimplified":
                    newString = ChineseConverter.Convert(SourceString, ChineseConversionDirection.TraditionalToSimplified);
                    break;
                default:
                    newString = SourceString;
                    break;
            }
            return newString;
        }

        public void StartDb()
        {
            string connectString = "server=127.0.0.1;uid=root;pwd=H1242457411qaz;database=db_tw";
            MySqlConnection mySqlConnection = new MySqlConnection(connectString);
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
        }

     
    }
}
