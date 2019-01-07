using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Duotify;
using Microsoft.Extensions.Logging;

namespace TraditionalToSimplified
{
    public class Utility
    {
        private readonly ILogger<TraditionalToSimplified> logger;
        private readonly HanConvert hanConvert;

        public Utility(ILogger<TraditionalToSimplified> logger, HanConvert hanConvert)
        {
            this.logger = logger;
            this.hanConvert = hanConvert;
        }

        public string ToSimplified(byte[] SourceString)
        {
            byte[] gbkByte = hanConvert.Big5_to_GBK(SourceString);
            return hanConvert.Trad_to_Simp(gbkByte, Encoding.GetEncoding(54936));       
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
            byte[] tradBytes = Encoding.GetEncoding(950).GetBytes(traditionalContent);
            string simplifiedContent = ToSimplified(tradBytes);
            string simplifiedEncodeContent = HttpUtility.UrlEncode(simplifiedContent, Encoding.GetEncoding("gb18030"));
            return simplifiedEncodeContent;
        }

    }
}
