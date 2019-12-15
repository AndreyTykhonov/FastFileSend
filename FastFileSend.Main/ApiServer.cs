using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace FastFileSend.Main
{
    public class ApiServer
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string FriendlyName { get; set; }

        //readonly static string ServerHost = "http://91.123.153.211:8080/api/";
        readonly static string ServerHost = "http://fastfilesend.somee.com/api/";

        Timer TimerHeartbeat { get; set; }

        public static async Task<ApiServer> CreateNewAccount()
        {
            HttpClient HttpClient = new HttpClient();

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
            HttpClient HttpClient = new HttpClient();
            await HttpClient.GetAsync(ServerHost + $"downloaded?download={download}");
        }

        public async Task NotifyOnline()
        {
            HttpClient HttpClient = new HttpClient();
            await HttpClient.GetAsync(ServerHost + $"online?id={Id}");
        }

        public async Task<DateTime> GetLastOnline(int id)
        {
            try
            {
                HttpClient HttpClient = new HttpClient();
                string json = await HttpClient.GetStringAsync(ServerHost + $"lastonline?id={id}");
                return JsonConvert.DeserializeObject<DateTime>(json);
            }
            catch (HttpRequestException)
            {
                return DateTime.MinValue;
            }
            catch (TaskCanceledException)
            {
                // Timeout. Server 
                return DateTime.MinValue;
            }
        }

        public async Task<List<HistoryItem>> GetHistory()
        {
            HttpClient HttpClient = new HttpClient();

            HttpResponseMessage httpResponseMessage = await HttpClient.GetAsync(ServerHost + $"history?id={Id}&password={Password}");
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                List<HistoryItem> historyList = JsonConvert.DeserializeObject<List<HistoryItem>>(json);
                foreach (HistoryItem item in historyList)
                {
                    item.File.Name = item.File.Name.Trim();
                }

                return historyList;
            }
            else
            {
                string reason = await httpResponseMessage.Content.ReadAsStringAsync();
                throw new Exception(reason);
            }
        }

        public async Task<FileItem> Upload(CloudFile cloudFile)
        {
            var builder = new UriBuilder(ServerHost + "upload");

            var query = HttpUtility.ParseQueryString(builder.Query);
            query["name"] = cloudFile.FileName;
            query["size"] = cloudFile.Size.ToString();
            query["crc32"] = cloudFile.CRC32.ToString();
            query["url"] = cloudFile.Url;
            builder.Query = query.ToString();

            HttpClient HttpClient = new HttpClient();

            Uri targetUri = new Uri(builder.ToString());

            string fileIdStr = await HttpClient.GetStringAsync(targetUri);
            int fileId = Convert.ToInt32(fileIdStr);

            return CloudFileToFileItem(cloudFile, fileId);
        }

        public async Task<int> Send(FileItem fileItem, int targetId)
        {
            HttpClient HttpClient = new HttpClient();

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
