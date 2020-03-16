using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel.Syndication;
using StackExchange.Redis;
using RedisTest;
using System.Xml;

namespace just_in_time_solutions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SqlConnectionStringBuilder stringBuilder = new SqlConnectionStringBuilder();
            stringBuilder["Server"] = "127.0.0.1,1433";
            stringBuilder["Database"] = "vnexpres";
            stringBuilder["User Id"] = "sa";
            stringBuilder["Password"] = "KhoaiChau1995";

            SqlConnection sqlConnection = new SqlConnection(stringBuilder.ToString());
            sqlConnection.StatisticsEnabled = true; // cho phép thu thập thông tin

            // Bắt đầu sự kiện thay dổi trạng thái kết nối
            sqlConnection.StateChange += (object sender, StateChangeEventArgs e) => {
                Console.WriteLine($"Trạng thái hiện tại: {e.CurrentState}, trạng thái trước:" + $"{e.OriginalState}");
            };

            sqlConnection.Open(); // mở kết nối

            // dùng SqlCommand thực hiện SQL
            using (SqlCommand command = sqlConnection.CreateCommand()) {
                command.CommandText = "SELECT title, content FROM dbo.News";
                using (SqlDataReader reader = command.ExecuteReader()) //sử dụng phương thức ExecuteReader
                {
                    Console.WriteLine("CÁC TIN TỨC:");
                    Console.WriteLine($"{"Title"}{"Content"}");
                    while (reader.Read())
                    {
                        Console.WriteLine(String.Format("{Title}, {Content}", reader["Title"], reader["Content"]));
                    }
                }
            }

            sqlConnection.Close(); // đóng kết nối

            // tao ket noi voi redis
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.1.5:6379,password=NguyenManhHa");

            // lay database
            IDatabase db = redis.GetDatabase(-1);
            
            // đọc nguồn cấp dữ liệu rss từ vnexpress
           Rss20FeedFormatter rssFormatter;
 
using(var xmlReader = XmlReader.Create
   ("http://vnexpress.net/rss/"))
{
   rssFormatter = new Rss20FeedFormatter();
   rssFormatter.ReadFrom(xmlReader);
 
}
 
var title = rssFormatter.Feed.Title.Text;
 
foreach (var syndicationItem in rssFormatter.Feed.Items)
{
   Console.WriteLine("Article: {0}",
      syndicationItem.Title.Text);
   Console.WriteLine("URL: {0}",
      syndicationItem.Links[0].Uri);
   Console.WriteLine("Summary: {0}",
      syndicationItem.Summary.Text);
   Console.WriteLine();
}
            
            CreateHostBuilder(args).Build().Run();
        }
        

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

