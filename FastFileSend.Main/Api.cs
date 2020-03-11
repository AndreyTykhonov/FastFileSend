using FastFileSend.Main.Enum;
using FastFileSend.Main.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
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
        /// <summary>
        /// Giving acces to user account details.
        /// </summary>
        public AccountDetails AccountDetails
        {
            get
            {
                return new AccountDetails(Id, Password);
            }
        }

        private int Id { get; set; }
        private string Password { get; set; }

        const string ServerHost = "http://fastfilesend.somee.com/api/";
        //const string ServerHost = "https://localhost:44342/api/";

        private Api(AccountDetails accountDetails, string accessToken)
        {
            Id = accountDetails.Id;
            Password = accountDetails.Password;
            AccessToken = accessToken;
        }

        /// <summary>
        /// This method used to authorization on the server with the specified account.
        /// </summary>
        /// <param name="accountDetails">Account logid details (id and password)</param>
        /// <returns>New authorized API server.</returns>
        public static async Task<Api> Login(AccountDetails accountDetails)
        {
            if (accountDetails is null)
            {
                throw new ArgumentNullException(nameof(accountDetails));
            }

            string accessToken = await Authorize(accountDetails).ConfigureAwait(false);
            return new Api(accountDetails, accessToken);
        }

        /// <summary>
        /// This method creating new random account and returns authorized API.
        /// </summary>
        /// <returns>Authorized API with randomly created account.</returns>
        public static async Task<Api> CreateNewAccount()
        {
            JObject jObject = await SendQueryAnonymous<JObject>("account/register", "").ConfigureAwait(false);

            int Id = (int)jObject["id"];
            string Password = (string)jObject["password"];

            AccountDetails accountDetails = new AccountDetails(Id, Password);
            string accessToken = await Authorize(accountDetails).ConfigureAwait(false);

            return new Api(accountDetails, accessToken);
        }

        private string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Trying to authorize on server. Setting AccessToken variable to JWT token.
        /// </summary>
        /// <param name="accountDetails">Account details to authorize.</param>
        /// <returns>JWT token that need to API constructor.</returns>
        private static async Task<string> Authorize(AccountDetails accountDetails)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["username"] = accountDetails.Id.ToString(CultureInfo.InvariantCulture);
            nameValueCollection["password"] = accountDetails.Password;

            JObject response = await SendQueryAnonymous<JObject>("account/token", nameValueCollection.ToString()).ConfigureAwait(false);
            //AccessToken = (string)response["access_token"];

            await FakePageVisit().ConfigureAwait(false);

            return (string)response["access_token"];
        }

        /// <summary>
        /// Free hosting removes page if not visited for X days.
        /// </summary>
        /// <returns></returns>
        private static async Task FakePageVisit()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                await httpClient.GetStringAsync("http://fastfilesend.somee.com/").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Universal API query sender. Creates new HttpClient for every call (better for threaded usage).
        /// </summary>
        /// <typeparam name="T">Server response type.</typeparam>
        /// <param name="api">Target API.</param>
        /// <param name="query">Target API params.</param>
        /// <returns>API response as T.</returns>
        async Task<T> SendQuery<T>(string api, string query)
        {
            // Unicode query fix
            query = Uri.EscapeUriString(HttpUtility.UrlDecode(query));

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ServerHost);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                return await RetryGetAsync<T>(httpClient, api, query).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Universal API query sender. Creates new HttpClient for every call (better for threaded usage). Anonymous version.
        /// </summary>
        /// <typeparam name="T">Server response type.</typeparam>
        /// <param name="api">Target API.</param>
        /// <param name="query">Target API params.</param>
        /// <returns>API response as T.</returns>
        static async Task<T> SendQueryAnonymous<T>(string api, string query)
        {
            // Unicode query fix
            query = Uri.EscapeUriString(HttpUtility.UrlDecode(query));

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ServerHost);
                return await RetryGetAsync<T>(httpClient, api, query).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retry GetAsync for X times.
        /// </summary>
        /// <typeparam name="T">Response target.</typeparam>
        /// <param name="httpClient">Input HttpClient.</param>
        /// <param name="api">API.</param>
        /// <param name="query">Query.</param>
        /// <param name="retryCount">Times to retry.</param>
        /// <returns></returns>
        private static async Task<T> RetryGetAsync<T>(HttpClient httpClient, string api, string query, int retryCount = 5)
        {
            // Retry for 5 times.
            for (int i = 0; i < retryCount; i++)
            {
                HttpResponseMessage response = await httpClient.GetAsync($"{api}?{query}").ConfigureAwait(false);

                // Last try. Throw exception if not success.
                if (i == retryCount - 1)
                {
                    response.EnsureSuccessStatusCode();
                }

                if (!response.IsSuccessStatusCode)
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    continue;
                }

                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(content);
            }

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
            nameValueCollection["download"] = download.ToString(CultureInfo.InvariantCulture);
            nameValueCollection["status"] = "1";

            await SendQuery<object>("file/setstatus", nameValueCollection.ToString()).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets status of File by id.
        /// </summary>
        /// <param name="download">File id.</param>
        /// <returns></returns>
        public async Task<HistoryModelStatus> GetFileStatus(int download)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["download"] = download.ToString(CultureInfo.InvariantCulture);

            return await SendQuery<HistoryModelStatus>("file/getstatus", nameValueCollection.ToString()).ConfigureAwait(false);
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
            nameValueCollection["id"] = id.ToString(CultureInfo.InvariantCulture);

            return await SendQuery<DateTime>("online/get", nameValueCollection.ToString()).ConfigureAwait(false);
        }

        /// <summary>
        /// Get all user files list.
        /// </summary>
        /// <param name="minimumDate">Minimum date to be retrieved.</param>
        /// <returns></returns>
        public async Task<List<HistoryModel>> GetHistory(DateTime minimumDate)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["ticks"] = minimumDate.Ticks.ToString(CultureInfo.InvariantCulture);

            return await SendQuery<List<HistoryModel>>("history/get", nameValueCollection.ToString()).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads file info to server. Returns FileItem.
        /// </summary>
        /// <param name="fileItem">Information about file without Id.</param>
        /// <returns>Same as input but now with Id.</returns>
        public async Task<FileItem> Upload(FileItem fileItem)
        {
            if (fileItem is null)
            {
                throw new ArgumentNullException(nameof(fileItem));
            }

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["file"] = JsonConvert.SerializeObject(fileItem);

            fileItem.Id = await SendQuery<int>("file/upload", query.ToString()).ConfigureAwait(false);

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
            if (fileItem is null)
            {
                throw new ArgumentNullException(nameof(fileItem));
            }

            var nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            nameValueCollection["id"] = Id.ToString(CultureInfo.InvariantCulture);
            nameValueCollection["password"] = Password.ToString(CultureInfo.InvariantCulture);
            nameValueCollection["target"] = targetId.ToString(CultureInfo.InvariantCulture);
            nameValueCollection["file"] = fileItem.Id.ToString(CultureInfo.InvariantCulture);

            return await SendQuery<int>("file/send", nameValueCollection.ToString()).ConfigureAwait(false);
        }
    }
}
