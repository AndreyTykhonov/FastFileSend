using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FastFileSend.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace FastFileSend.Tests
{
    internal class HttpMessageHandlerApiTests : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string content = string.Empty;
            HttpStatusCode status = HttpStatusCode.OK;

            switch (request.RequestUri.LocalPath)
            {
                case "/api/register":
                    content = "{\"user_idx\":406028,\"user_friendlyname\":\"406028\",\"user_registerdate\":\"2019-12-20T04:36:41.8534542-08:00\",\"user_password\":\"520568373\",\"user_lastonline\":\"0001-01-01T00:00:00\",\"transactions_receiver\":[],\"transactions_sender\":[]}";
                    break;
                case "/api/lastonline":
                    if (request.RequestUri.Query == "?id=406028")
                    {
                        content = JsonConvert.SerializeObject(DateTime.Now);
                    }
                    else
                    {
                        status = HttpStatusCode.InternalServerError;
                    }
                    break;
                case "/api/downloaded":
                    Assert.IsTrue(request.RequestUri.Query == "?download=100", "Wrong download request!");
                    break;
                case "/api/online":
                    Assert.IsTrue(request.RequestUri.Query == "?id=406028", "Wrong online report Id!");
                    break;
                case "/api/history":
                    Assert.IsTrue(request.RequestUri.Query == "?id=406028&password=520568373", "Wrong history query!");

                    FileItem fileItem = new FileItem(500, "file", 600, 700, DateTime.MaxValue, "url");
                    HistoryItem mockItem = new HistoryItem { Date = DateTime.MaxValue, Id = 1001, Receiver = 100, Sender = 200, Status = 5, File = fileItem };
                    List<HistoryItem> mockList = new List<HistoryItem> { mockItem };
                    content = JsonConvert.SerializeObject(mockList);
                    break;
                case "/api/upload":
                    Assert.IsTrue(request.RequestUri.Query == "?name=file&size=600&crc32=700&url=url", "Wrong upload query!");
                    content = JsonConvert.SerializeObject(10000);
                    break;
                case "/api/send":
                    Assert.IsTrue(request.RequestUri.Query == "?id=406028&password=520568373&target=5000&file=500", "Wrong send query!");
                    content = JsonConvert.SerializeObject(1000);
                    break;
                default:
                    throw new Exception("Unknown request: " + request.RequestUri);
                    break;
            }

            HttpResponseMessage responseMessage = new HttpResponseMessage(status)
            {
                Content = new StringContent(content)
            };

            return await Task.FromResult(responseMessage);
        }
    }

    [TestClass]
    public class ApiServerTests
    {
        ApiServer ApiServer { get; set; }
        public ApiServerTests()
        {
            ApiServer = new ApiServer(406028, "520568373", new HttpMessageHandlerApiTests());
        }

        [TestMethod]
        public async Task CreateAccountTest()
        {
            ApiServer apiServerDebug = await ApiServer.CreateNewAccount(new HttpMessageHandlerApiTests());

            Assert.IsTrue(apiServerDebug.Id == 406028, "Wrong id!");
            Assert.IsTrue(apiServerDebug.Password == "520568373", "Wrong password!");
            Assert.IsTrue(apiServerDebug.FriendlyName == "406028", "Wrong password!");
        }

        [TestMethod]
        public async Task OnlineTest()
        {
            Assert.IsTrue((await ApiServer.GetLastOnline(406028)) > DateTime.MinValue, "Wrong last online!");
            Assert.IsTrue((await ApiServer.GetLastOnline(0)).Equals(DateTime.MinValue), "Wrong last online for unknown user!");
        }

        [TestMethod]
        public async Task NotifyDownloadedTest()
        {
            await ApiServer.NotifyDownloadedAsync(100);
        }

        [TestMethod]
        public async Task NotifyOnlineTest()
        {
            await ApiServer.NotifyOnline();
        }

        [TestMethod]
        public async Task GetHistoryTest()
        {
            List<HistoryItem> historyItemList = await ApiServer.GetHistory();

            Assert.IsTrue(historyItemList.Count == 1, "Wrong history count!");

            HistoryItem historyItem = historyItemList.First();
            Assert.IsTrue(historyItem.Date == DateTime.MaxValue, "Invalid date!");
            Assert.IsTrue(historyItem.Id == 1001, "Invalid id!");
            Assert.IsTrue(historyItem.Receiver == 100, "Invalid receiver!");
            Assert.IsTrue(historyItem.Sender == 200, "Invalid sender!");
            Assert.IsTrue(historyItem.Status == 5, "Invalid status!");

            Assert.IsTrue(historyItem.File.Id == 500, "Invalid file id!");
            Assert.IsTrue(historyItem.File.Size == 600, "Invalid file size!");
            Assert.IsTrue(historyItem.File.CRC32 == 700, "Invalid file crc!");
            Assert.IsTrue(historyItem.File.CreationDate == DateTime.MaxValue, "Invalid file creation date!");
            Assert.IsTrue(historyItem.File.Url == "url", "Invalid file url!");
            Assert.IsTrue(historyItem.File.Name == "file", "Invalid file name!");
        }

        [TestMethod]
        public async Task UploadTest()
        {
            FileItem fileItem = new FileItem(500, "file", 600, 700, DateTime.MaxValue, "url");
            Assert.IsTrue((await ApiServer.Upload(fileItem)).Id == 10000, "Wrong upload id!");
        }

        [TestMethod]
        public async Task SendTest()
        {
            FileItem fileItem = new FileItem(500, "file", 600, 700, DateTime.MaxValue, "url");
            Assert.IsTrue((await ApiServer.Send(fileItem, 5000)) == 1000, "Wrong send id!");
        }
    }
}
