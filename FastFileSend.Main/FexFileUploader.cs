using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main
{

    public class FexFileUploader : IProgress<long>
    {
        HttpClient HttpClient { get; set; }
        Stopwatch SpeedWatch { get; set; }

        long FileSize { get; set; }
        string FileName { get; set; }

        public event Action<double, double> OnProgress = delegate { };

        async Task<string> GetUploadTokenAsync()
        {
            Uri fexGetUploadTokenUri = new Uri("https://api.fex.net/api/v1/anonymous/upload-token");
            HttpClient = new HttpClient();
            string json = await HttpClient.GetStringAsync(fexGetUploadTokenUri);
            return (string) JObject.Parse(json)["token"];
        }

        async Task<UploadDataInfo> GetUploadDataInfoAsync(JObject json_payload)
        {
            HttpContent postData = new StringContent(json_payload.ToString(), Encoding.Unicode, "application/json");

            Uri fexFileUri = new Uri("https://api.fex.net/api/v1/anonymous/file");
            HttpResponseMessage response = await HttpClient.PostAsync(fexFileUri, postData);

            string response_str = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UploadDataInfo>(response_str);
        }

        public async Task<FileItem> UploadAsync(string path)
        {
            FileName = Path.GetFileName(path);
            FileSize = new FileInfo(path).Length;

            await PrepareAuthorizedHttpClient(FileName, FileSize);

            JObject json_payload = GenerateJsonPayload(FileName, FileSize);
            UploadDataInfo uploadDataInfo = await GetUploadDataInfoAsync(json_payload);

            Uri uploadUri = new Uri(uploadDataInfo.location);

            do
            {
                try
                {
                    await PrepareUploadLink(uploadUri);
                    break;
                }
                catch (HttpRequestException)
                {
                    // Retry delay
                    await Task.Delay(500);
                }
            } while (true);

            SpeedWatch = Stopwatch.StartNew();

            JObject uploadedFileInfo = await StartUploadAsync(path, uploadUri);

            SpeedWatch.Stop();

            FileItem uploadedFile = UploadedInfoToFileItem(uploadedFileInfo);

            return uploadedFile;
        }

        private static FileItem UploadedInfoToFileItem(JObject uploadedFileInfo)
        {
            DateTime uploadedDateTime = DateTimeOffset.FromUnixTimeMilliseconds((long)uploadedFileInfo["created_at"]).DateTime;
            string crc32_str = (string)uploadedFileInfo["crc32"];
            int crc32 = int.Parse(crc32_str, System.Globalization.NumberStyles.HexNumber);

            FileItem uploadedFile = new FileItem(0, (string)uploadedFileInfo["name"], (long)uploadedFileInfo["size"], crc32, uploadedDateTime, (string)uploadedFileInfo["download_url"]);
            return uploadedFile;
        }

        private async Task<JObject> StartUploadAsync(string path, Uri uploadUri)
        {
            FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read);

            int bufferSize = 4 * 1024 * 1024;
            do
            {
                long sendPosition = fs.Position;
                long readUntil = Math.Min(fs.Length, fs.Position + bufferSize);
                int readSize = (int)(readUntil - fs.Position);

                byte[] buffer = new byte[readSize];
                fs.Read(buffer, 0, readSize);

                Stream bufferStream = new MemoryStream();
                bufferStream.Write(buffer, 0, readSize);
                bufferStream.Position = 0;

                StreamContent streamContent = new StreamContent(bufferStream);
                streamContent.Headers.Add("Content-Type", "application/octet-stream");

                HttpResponseMessage response = await HttpClient.PatchAsync(uploadUri, streamContent, sendPosition);

                bool finalPush = fs.Position == fs.Length;

                if (finalPush)
                {
                    fs.Close();
                    response.EnsureSuccessStatusCode();

                    string response_str = await response.Content.ReadAsStringAsync();

                    JObject uploadedFileInfo = JObject.Parse(response_str);
                    return uploadedFileInfo;
                }

                Report(fs.Position);
            } while (true);
        }

        private async Task PrepareUploadLink(Uri uploadUri)
        {
            HttpResponseMessage response = await HttpClient.PostAsync(uploadUri, null);
            response.EnsureSuccessStatusCode();
        }

        private async Task PrepareAuthorizedHttpClient(string filename, long size)
        {
            HttpClient = new HttpClient();
            string token = await GetUploadTokenAsync();
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpClient.DefaultRequestHeaders.Add("fsp-filename", filename);
            HttpClient.DefaultRequestHeaders.Add("fsp-size", size.ToString());
            HttpClient.DefaultRequestHeaders.Add("fsp-version", "1.0.0");
        }

        private static JObject GenerateJsonPayload(string filename, long size)
        {
            JObject json_payload = new JObject();
            json_payload.Add("directory_id", null);
            json_payload.Add("size", size);
            json_payload.Add("name", filename);
            return json_payload;
        }

        public void Report(long value)
        {
            long bytesDownloaded = value;
            double speedMb = bytesDownloaded  / 1048576.0 / SpeedWatch.Elapsed.TotalSeconds;
            OnProgress((double)bytesDownloaded / FileSize, speedMb);
        }
    }
}
