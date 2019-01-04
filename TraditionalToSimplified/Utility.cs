using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TraditionalToSimplified
{
    public class Utility
    {
        internal const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        internal const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        internal const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);
        //使用OS的kernel.dll做為繁簡轉換工具
        public string ToSimplified(string SourceString, string Language)
        {
            String tTarget = new String(' ', SourceString.Length);
            int tReturn = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, SourceString, SourceString.Length, tTarget, SourceString.Length);
            return tTarget;

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
