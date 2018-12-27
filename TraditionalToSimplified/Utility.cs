using Microsoft.Extensions.Configuration;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

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

        public static IConfigurationRoot GetJson()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("settings.json");
            var configuration = builder.Build();
            return configuration;
        }

        public string Encode(string data)
        {
            string result = "";
            string key = "abcdefgh";
            string iv = "12345678";
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = Encoding.ASCII.GetBytes(key);
            des.IV = Encoding.ASCII.GetBytes(iv);
            byte[] byteArray = Encoding.ASCII.GetBytes(data);
            ICryptoTransform desencrypt = des.CreateEncryptor();
            return result = BitConverter.ToString(desencrypt.TransformFinalBlock(byteArray, 0, byteArray.Length)).Replace("-", string.Empty);

        }

        public string Decode(string data)
        {
            string key = "abcdefgh";
            string iv = "12345678";

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = Encoding.ASCII.GetBytes(key);
            des.IV = Encoding.ASCII.GetBytes(iv);
            byte[] byteArray = new byte[data.Length / 2];
            int j = 0;
            for (int i = 0; i < data.Length / 2; i++)
            {
                byteArray[i] = Byte.Parse(data[j].ToString() + data[j + 1].ToString(), System.Globalization.NumberStyles.HexNumber);
                j += 2;
            }
            ICryptoTransform desencrypt = des.CreateDecryptor();
            string result = Encoding.ASCII.GetString(desencrypt.TransformFinalBlock(byteArray, 0, byteArray.Length));
            return result;
        }

        public string Big5ToGb18030(string data)
        {
            string traditionalContent = HttpUtility.UrlDecode(data, Encoding.GetEncoding("big5"));
            string simplifiedContent = ToSimplified(traditionalContent, "ToSimplified");
            string simplifiedEncodeContent = HttpUtility.UrlEncode(simplifiedContent, Encoding.GetEncoding("gb18030"));
            return simplifiedEncodeContent;
        }
    }
}
