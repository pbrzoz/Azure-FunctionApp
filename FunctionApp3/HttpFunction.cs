using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace FunctionApp3
{
    public static class HttpFunction
    {
        static string conString = Environment.GetEnvironmentVariable("sql_connection");
        [FunctionName("GetItem")]
        public static async Task<IActionResult> GetItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string id = req.Query["id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id ??= data?.id;

            List<Announcements> item = null;
            
            using (var con = new SqlConnection(conString))
            {
                string query = "Select * from Announcements where id=@id;";

                item = (List<Announcements>)con.Query<Announcements>(query,new { id=$"{id}" });
            }

            return new OkObjectResult(item[0]);
        }

        [FunctionName("GetItemsWithFilters")]
        public static async Task<IActionResult> GetItemsWithFilters(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string title = req.Query["title"];
            string contentType = req.Query["contentType"];
            string page = req.Query["page"];

            log.LogInformation($"{title} {contentType} {page}");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            title ??= data?.title;
            contentType ??= data?.contentType;
            page ??= data?.page;

            List<Announcements> items = null;

            using (var con = new SqlConnection(conString))
            {
                if (title != null && contentType==null)
                {
                    string query = "Select * from Announcements where Title = @title order by Id OFFSET (cast ((@page + '0') as int)) rows;";
                    items = (List<Announcements>)con.Query<Announcements>(query, new { title = $"{title}", page = $"{page}" });
                }
                if (title==null && contentType != null)
                {
                    string query = "Select * from Announcements where ContentType = @contentType order by Id OFFSET (cast ((@page + '0') as int)) rows;";
                    items = (List<Announcements>)con.Query<Announcements>(query, new { contentType = $"{contentType}", page = $"{page}" });
                }
            }        

            return new OkObjectResult(items);
        }

        [FunctionName("AddItem")]
        public static async Task<IActionResult> AddItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string title = req.Query["Title"];
            string content = req.Query["Content"];
            string contentType = req.Query["ContentType"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            title ??= data?.title;
            content ??= data?.content;
            contentType ??= data.contentType;

            using (var con = new SqlConnection(conString))
            {
                string query = "Insert into Announcements values(@title,@content,@contentType);";

                var rows = con.Execute(query, new { title = $"{title}", content=$"{content}",contentType=$"{contentType}"});
                log.LogInformation($"Affected rows: {rows}");
            }

            string responseMessage = "Item inserted";
            return new OkObjectResult(responseMessage);
        }

        [FunctionName("DeleteItem")]
        public static async Task<IActionResult> DeleteItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string id = req.Query["id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id ??= data?.id;

            using (var con = new SqlConnection(conString))
            {
                string query = "Delete from Announcements where Id = @id;";

                var rows =con.Execute(query, new { id = $"{id}" });
                log.LogInformation($"Affected rows: {rows}");
            }

            string responseMessage = "Item removed";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("UpdateItem")]
        public static async Task<IActionResult> UpdateItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string id = req.Query["id"];
            string title = req.Query["Title"];
            string content = req.Query["Content"];
            string contentType = req.Query["ContentType"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id ??= data?.id;
            title ??= data?.title;
            content ??= data?.content;
            contentType ??= data.contentType;

            using (var con = new SqlConnection(conString))
            {
                string query = "Update Announcements Set Title = @title, Content=@content, ContentType=@contentType  where Id = @id;";

                var rows = con.Execute(query, new { id = $"{id}", title = $"{title}", content = $"{content}", contentType = $"{contentType}" });
                log.LogInformation($"Affected rows: {rows}");
            }

            string responseMessage = "Item updated";

            return new OkObjectResult(responseMessage);
        }
    }
}
