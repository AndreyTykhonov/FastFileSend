using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main
{
    public class ApiServer
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string FriendlyName { get; set; }

        private HttpClient HttpClient { get; } = new HttpClient();
        readonly string ServerHost = "https://localhost:44350/api/";

        public ApiServer()
        {
            RegisterAsync().Wait();
        }

        async Task RegisterAsync()
        {
            string registerJson = await HttpClient.GetStringAsync(ServerHost + "register");
            JObject jObject = JObject.Parse(registerJson);

            Id = (int)jObject["user_idx"];
            Password = (string)jObject["user_password"];
            FriendlyName = (string)jObject["user_friendlyname"];
        }

        public async Task NotifyDownloadedAsync(FileItem file)
        {
            await HttpClient.GetAsync(ServerHost + $"downloaded?file={file.Id}");
        }

        public async Task<List<HistoryItem>> GetHistory()
        {
            string json = await HttpClient.GetStringAsync(ServerHost + $"history?id={Id}&password={Password}");
            return JsonConvert.DeserializeObject<List<HistoryItem>>(json);
        }

        public async Task<FileItem> Upload(CloudFile cloudFile)
        {
            string fileIdStr = await HttpClient.GetStringAsync(ServerHost + $"upload?name={cloudFile.FileName}&size={cloudFile.Size}&crc32={cloudFile.CRC32}&creationDate={cloudFile.CreationDate}&url={cloudFile.Url}");
            int fileId = Convert.ToInt32(fileIdStr);

            return CloudFileToFileItem(cloudFile, fileId);
        }

        public async Task Send(FileItem fileItem, int targetId)
        {
            await HttpClient.GetStringAsync(ServerHost + $"send?id={Id}&password={Password}&target={targetId}&file={fileItem.Id}");
        }

        private static FileItem CloudFileToFileItem(CloudFile cloudFile, int fileId)
        {
            FileItem fileItem = new FileItem
            {
                Name = cloudFile.FileName,
                CRC32 = cloudFile.CRC32,
                Size = cloudFile.Size,
                Id = fileId,
                CreationDate = cloudFile.CreationDate,
                Url = cloudFile.Url
            };

            return fileItem;
        }
    }
}
