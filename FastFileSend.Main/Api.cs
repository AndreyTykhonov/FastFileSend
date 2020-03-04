using FastFileSend.Main.Models;
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
    /// <summary>
    /// This class used to communicate with API server. Must be authorized to use this.
    /// </summary>
    public class Api
    {
        public AccountDetails AccountDetails
        {
            get
            {
                return new AccountDetails(Id, Password);
            }
        }

        private int Id { get; set; }
        private string Password { get; set; }

        readonly static string ServerHost = "http://fastfilesend.somee.com/api/";
        //readonly static string ServerHost = "https://localhost:44342/api/";

        Timer TimerHeartbeat { get; set; }

        private Api(AccountDetails accountDetails)
        {
            Id = accountDetails.Id;
            Password = accountDetails.Password;
        }

        /// <summary>
        /// This method used to authorization on the server with the specified account.
        /// </summary>
        /// <param name="accountDetails">Account logid details (id and password)</param>
        /// <returns>New authorized API server.</returns>
        public static async Task<Api> Login(AccountDetails accountDetails)
        {
            Api api = new Api(accountDetails);
            await Authorize(accountDetails);
            return api;
        }

        /// <summary>
        /// This method creating new random account and returns authorized API.
        /// </summary>
        /// <returns>Authorized API with randomly created account.</returns>
        public static async Task<Api> CreateNewAccount()
        {
            JObject jObject = await SendQuery<JObject>("account/register", "");

            int Id = (int)jObject["user_idx"];
            string Password = (string)jObject["user_password"];

            AccountDetails accountDetails = new AccountDetails(Id, Password);

            return new Api(accountDetails);
        }

        private static string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Trying to authorize on server. Setting AccessToken variable to JWT token.
        /// </summary>
        /// <returns></returns>
        private static async Task Authorize(AccountDetails accountDetails)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["username"] = accountDetails.Id.ToString();
            nameValueCollection["password"] = accountDetails.Password;

            JObject response = await SendQuery<JObject>("account/token", nameValueCollection.ToString());
            AccessToken = (string)response["access_token"];

            await FakePageVisit();
        }

        /// <summary>
        /// Free hosting removes page if not visited for X days.
        /// </summary>
        /// <returns></returns>
        private static async Task FakePageVisit()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                await httpClient.GetStringAsync("http://fastfilesend.somee.com/");
            }
        }

        /// <summary>
        /// Universal API query sender. Creates new HttpClient for every call (better for threaded usage).
        /// </summary>
        /// <typeparam name="T">Server response type.</typeparam>
        /// <param name="api">Target API.</param>
        /// <param name="query">Target API params.</param>
        /// <returns>API response as T.</returns>
        static async Task<T> SendQuery<T>(string api, string query)
        {
            // Unicode query fix
            query = Uri.EscapeUriString(HttpUtility.UrlDecode(query));

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ServerHost);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                // Retry for 5 times.
                for (int i = 0; i < 5; i++)
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"{api}?{query}");

                    // Last try. Throw exception if not success.
                    if (i == 4)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    string content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(content);
                }
            }

            // Will be never executed.
            return default;
        }

        /// <summary>
        /// Set File status to Downloaded.
        /// </summary>
        /// <param name="download">File id.</param>
        /// <returns></returns>
        public async Task NotifyDownloadedAsync(int download)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["download"] = download.ToString();
            nameValueCollection["status"] = "1";

            await SendQuery<object>("file/setstatus", nameValueCollection.ToString());
        }

        /// <summary>
        /// Get last user online by id. Throws exception if this user not exists.
        /// </summary>
        /// <param name="id">Target user Id.</param>
        /// <returns>DateTime with last online.</returns>
        /// <exception cref="HttpRequestException">Throws exception if this user not exists.</exception>
        public async Task<DateTime> GetLastOnline(int id)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["id"] = id.ToString();

            return await SendQuery<DateTime>("online/get", nameValueCollection.ToString());
        }

        /// <summary>
        /// Get all user files list.
        /// </summary>
        /// <param name="minimumDate">Minimum date to be retrieved.</param>
        /// <returns></returns>
        public async Task<List<HistoryModel>> GetHistory(DateTime minimumDate)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["minimum"] = minimumDate.ToString();

            return await SendQuery<List<HistoryModel>>("history/get", nameValueCollection.ToString());
        }

        /// <summary>
        /// Uploads file info to server. Returns FileItem.
        /// </summary>
        /// <param name="fileItem">Information about file without Id.</param>
        /// <returns>Same as input but now with Id.</returns>
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

        /// <summary>
        /// Send to target user file.
        /// </summary>
        /// <param name="fileItem">FileItem must contains Id (uploaded before use).</param>
        /// <param name="targetId">Target user Id.</param>
        /// <returns>Returns transaction id.</returns>
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
