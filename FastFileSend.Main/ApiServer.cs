using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        readonly static string ServerHost = "http://fastfilesend.somee.com/api/";
        //readonly static string ServerHost = "https://localhost:44342/api/";

        Timer TimerHeartbeat { get; set; }

        static HttpMessageHandler HttpMessageHandler { get; set; }

        public ApiServer(int id, string password, HttpMessageHandler httpClientHandler)
        {
            HttpMessageHandler = httpClientHandler;

            Id = id;
            Password = password;
            FriendlyName = id.ToString();
        }

        private static string AccessToken { get; set; } = string.Empty;

        public async Task Login()
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["username"] = Id.ToString();
            nameValueCollection["password"] = Password;

            JObject response = await SendQuery<JObject>("account/token", nameValueCollection.ToString());
            AccessToken = (string)response["access_token"];

            await FakePageVisit();

            StartHeartbeatTimer();
        }

        private async Task FakePageVisit()
        {
            using (HttpClient httpClient = new HttpClient(HttpMessageHandler, false))
            {
                await httpClient.GetStringAsync("http://fastfilesend.somee.com/");
            }
        }

        private void StartHeartbeatTimer()
        {
            TimerHeartbeat = new Timer(15000);
            TimerHeartbeat.Elapsed += TimerHeartbeat_Elapsed;
            TimerHeartbeat.AutoReset = false;

            TimerHeartbeat_Elapsed(this, null);
        }

       static async Task<T> SendQuery<T>(string api, string query)
        {
            using (HttpClient httpClient = new HttpClient(HttpMessageHandler, false))
            {
                httpClient.BaseAddress = new Uri(ServerHost);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                // 5 times retry
                for (int i = 0; i < 5; i++)
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"{api}?{query}");

                    if (!response.IsSuccessStatusCode)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    string content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(content);
                }

                throw new HttpRequestException($"{api} failed after 5 retry!");
            }
        }

        public static async Task<ApiServer> CreateNewAccount(HttpMessageHandler httpClientHandler)
        {
            HttpMessageHandler = httpClientHandler;

            JObject jObject = await SendQuery<JObject>("account/register", "");

            int Id = (int)jObject["user_idx"];
            string Password = (string)jObject["user_password"];
            string FriendlyName = (string)jObject["user_friendlyname"];

            return new ApiServer(Id, Password, httpClientHandler);
        }

        private async void TimerHeartbeat_Elapsed(object sender, ElapsedEventArgs e)
        {
            await NotifyOnline();
            TimerHeartbeat.Start();
        }

        public async Task NotifyDownloadedAsync(int download)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["download"] = download.ToString();
            nameValueCollection["status"] = "1";

            await SendQuery<object>("file/setstatus", nameValueCollection.ToString());
        }

        public async Task NotifyOnline()
        {
            await SendQuery<object>("online/update", "");
        }

        public async Task<DateTime> GetLastOnline(int id)
        {
            try
            {
                var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
                nameValueCollection["id"] = id.ToString();

                return await SendQuery<DateTime>("online/get", nameValueCollection.ToString());
            }
            catch (HttpRequestException)
            {
                return DateTime.MinValue;
            }
        }

        public async Task<List<HistoryItem>> GetHistory()
        {
            List<HistoryItem> historyItems = await SendQuery<List<HistoryItem>>("history/get", "");

            foreach (HistoryItem item in historyItems)
            {
                item.File.Name = item.File.Name.Trim();
            }

            return historyItems;
        }

        public async Task<FileItem> Upload(FileItem fileItem)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["name"] = fileItem.Name;
            query["size"] = fileItem.Size.ToString();
            query["crc32"] = fileItem.CRC32.ToString();
            query["url"] = fileItem.Url;

            fileItem.Id = await SendQuery<int>("file/upload", query.ToString());

            return fileItem;
        }

        public async Task<int> Send(FileItem fileItem, int targetId)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["id"] = Id.ToString();
            nameValueCollection["password"] = Password.ToString();
            nameValueCollection["target"] = targetId.ToString();
            nameValueCollection["file"] = fileItem.Id.ToString();

            return await SendQuery<int>("file/send", nameValueCollection.ToString());
        }
    }
}
