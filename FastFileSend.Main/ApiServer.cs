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

        private static HttpClient HttpClient { get; } = new HttpClient();
        readonly static string ServerHost = "https://localhost:44350/api/";

        public static async Task<ApiServer> CreateNewAccount()
        {
            string registerJson = await HttpClient.GetStringAsync(ServerHost + "register");
            JObject jObject = JObject.Parse(registerJson);

            int Id = (int)jObject["user_idx"];
            string Password = (string)jObject["user_password"];
            string FriendlyName = (string)jObject["user_friendlyname"];

            return new ApiServer(Id, Password);
        }

        public ApiServer(int id, string password)
        {
            Id = id;
            Password = password;
            FriendlyName = id.ToString();
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
