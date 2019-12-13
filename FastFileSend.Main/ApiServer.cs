using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FastFileSend.Main
{
    public class ApiServer
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string FriendlyName { get; set; }

        private static HttpClient HttpClient { get; } = new HttpClient();
        readonly static string ServerHost = "http://91.123.153.211:8080/api/";

        Timer TimerHeartbeat { get; set; }

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

            TimerHeartbeat = new Timer(15000);
            TimerHeartbeat.Elapsed += TimerHeartbeat_Elapsed;
            TimerHeartbeat.Start();

            TimerHeartbeat_Elapsed(this, null);
        }

        private async void TimerHeartbeat_Elapsed(object sender, ElapsedEventArgs e)
        {
            await NotifyOnline();
        }

        public async Task NotifyDownloadedAsync(int download)
        {
            await HttpClient.GetAsync(ServerHost + $"downloaded?download={download}");
        }

        public async Task NotifyOnline()
        {
            await HttpClient.GetAsync(ServerHost + $"online?id={Id}");
        }

        public async Task<DateTime> GetLastOnline(int id)
        {
            string json = await HttpClient.GetStringAsync(ServerHost + $"lastonline?id={id}");
            return JsonConvert.DeserializeObject<DateTime>(json);
        }

        public async Task<List<HistoryItem>> GetHistory()
        {
            string json = await HttpClient.GetStringAsync(ServerHost + $"history?id={Id}&password={Password}");
            return JsonConvert.DeserializeObject<List<HistoryItem>>(json);
        }

        public async Task<FileItem> Upload(CloudFile cloudFile)
        {
            string url = ServerHost + $"upload?name={cloudFile.FileName}&size={cloudFile.Size}&crc32={cloudFile.CRC32}&url={cloudFile.Url}";
            Uri targetUri = new Uri(url);

            string fileIdStr = await HttpClient.GetStringAsync(targetUri);
            int fileId = Convert.ToInt32(fileIdStr);

            return CloudFileToFileItem(cloudFile, fileId);
        }

        public async Task<int> Send(FileItem fileItem, int targetId)
        {
            string json = await HttpClient.GetStringAsync(ServerHost + $"send?id={Id}&password={Password}&target={targetId}&file={fileItem.Id}");
            return JsonConvert.DeserializeObject<int>(json);
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
