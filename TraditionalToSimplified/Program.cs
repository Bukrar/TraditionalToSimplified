﻿using Microsoft.Extensions.Configuration;
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
            //讀取json DB連線資訊和TABLE名稱等資料
            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("settings.json");
            var configuration = builder.Build();

            Db_Tw_Service db_Tw_Service = new Db_Tw_Service();
            Db_Event_Tw_Service db_Event_Tw_Service = new Db_Event_Tw_Service();

            //計時
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //修改db_Tw資料庫
            db_Tw_Service.UpdateTableMy_Category_Tw();
            db_Tw_Service.UpdateTableMy_Country_Tw();
            db_Tw_Service.UpdateTableMy_Press_Tw();
            db_Tw_Service.UpdateTableMy_Product_Tw();
            db_Tw_Service.UpdateTableMy_Publisher_Tw();
            db_Tw_Service.UpdateTableMy_Region_Tw();
            db_Tw_Service.UpdateTableMy_Subtopic_Tw();
            db_Tw_Service.UpdateTableMy_Topic_Tw();
            //修改db_Event_Tw資料庫
            db_Event_Tw_Service.UpdateTableMy_Event_Category_Tw();
            db_Event_Tw_Service.UpdateTableMy_My_Event_Press_Tw();
            db_Event_Tw_Service.UpdateTableMy_Event_Region_Tw();
            db_Event_Tw_Service.UpdateTableMy_Partnar_Category_Tw();

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                 ts.Hours, ts.Minutes, ts.Seconds,
                                 ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            Console.ReadLine();
        }
    }
}


