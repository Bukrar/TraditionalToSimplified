using Microsoft.Extensions.Configuration;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static IConfigurationRoot GetJson()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("settings.json");
            var configuration = builder.Build();
            return configuration;
        }
    }
}
