using Duotify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TraditionalToSimplified
{
    class Program
    {
        static void Main(string[] args)
        {              
            using (var serviceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider())
            {
                var timer = serviceProvider.GetService<Stopwatch>();
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();               
                var traditionalToSimplified = serviceProvider.GetService<TraditionalToSimplified>();
                //計時
                timer.Start();
                //執行修改繁體資料庫資料至簡體資料庫(繁體BIG5轉GB18030)
                traditionalToSimplified.DbDataHandle();
                timer.Stop();
                TimeSpan ts = timer.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                     ts.Hours, ts.Minutes, ts.Seconds,
                                     ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);
            }
            
            Console.ReadLine();  
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services
                .AddLogging(builder =>
                {
                    builder.AddConsole(opt => opt.IncludeScopes = true);
                })
                .AddTransient<HanConvert>()
                .AddSingleton<TraditionalToSimplified>()
                .AddSingleton<Stopwatch>()
                .AddSingleton<Utility>();
        }

    }
}


