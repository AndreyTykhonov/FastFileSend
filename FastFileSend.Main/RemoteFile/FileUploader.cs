using FastFileSend.Main.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main.RemoteFile
{
    /// <summary>
    /// Uploads file to Fex.net.
    /// </summary>
    public class FileUploader : ProgressableFile
    {
        async Task<string> GetUploadTokenAsync()
        {
            Uri fexGetUploadTokenUri = new Uri("https://api.fex.net/api/v1/anonymous/upload-token");
            string json = await HttpClient.GetStringAsync(fexGetUploadTokenUri).ConfigureAwait(false);
            return (string) JObject.Parse(json)["token"];
        }

        async Task<FexFileUploadDataInfo> GetUploadDataInfoAsync(JObject json_payload)
        {
            using (HttpContent postData = new StringContent(json_payload.ToString(), Encoding.Unicode, "application/json"))
            {

                Uri fexFileUri = new Uri("https://api.fex.net/api/v1/anonymous/file");
                HttpResponseMessage response = await HttpClient.PostAsync(fexFileUri, postData).ConfigureAwait(false);

                string response_str = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<FexFileUploadDataInfo>(response_str);
            }
        }

        public async Task<FileItem> UploadAsync(string filename, Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            string fileName = filename;
            Size = stream.Length;

            await PrepareAuthorizedHttpClient(fileName, Size).ConfigureAwait(false);

            JObject json_payload = GenerateJsonPayload(fileName, Size);
            FexFileUploadDataInfo uploadDataInfo = await GetUploadDataInfoAsync(json_payload).ConfigureAwait(false);

            Uri uploadUri = new Uri(uploadDataInfo.location);

            // 5 times retry.
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    await PrepareUploadLink(uploadUri).ConfigureAwait(false);
                    break;
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(500).ConfigureAwait(false);
                }
            }

            JObject uploadedFileInfo = await StartUploadAsync(stream, uploadUri).ConfigureAwait(false);

            FileItem uploadedFile = UploadedInfoToFileItem(uploadedFileInfo);

            return uploadedFile;
        }

        private static FileItem UploadedInfoToFileItem(JObject uploadedFileInfo)
        {
            DateTime uploadedDateTime = DateTimeOffset.FromUnixTimeMilliseconds((long)uploadedFileInfo["created_at"]).DateTime;
            string crc32_str = (string)uploadedFileInfo["crc32"];
            int crc32 = int.Parse(crc32_str, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            FileItem uploadedFile = new FileItem(0, (string)uploadedFileInfo["name"], (long)uploadedFileInfo["size"], crc32, uploadedDateTime, new List<Uri> { new Uri((string)uploadedFileInfo["download_url"]) });
            return uploadedFile;
        }

        private async Task<JObject> StartUploadAsync(Stream stream, Uri uploadUri)
        {
            int bufferSize = 4 * 1024 * 1024;
            do
            {
                long sendPosition = stream.Position;
                long readUntil = Math.Min(stream.Length, stream.Position + bufferSize);
                int readSize = (int)(readUntil - stream.Position);

                byte[] buffer = new byte[readSize];
                stream.Read(buffer, 0, readSize);

                Stream bufferStream = new MemoryStream();
                bufferStream.Write(buffer, 0, readSize);
                bufferStream.Position = 0;

                using (StreamContent streamContent = new StreamContent(bufferStream))
                {
                    streamContent.Headers.Add("Content-Type", "application/octet-stream");

                    using (HttpResponseMessage response = await HttpClient.PatchAsync(uploadUri, streamContent, sendPosition).ConfigureAwait(false))
                    {

                        bool finalPush = stream.Position == stream.Length;

                        if (finalPush)
                        {
                            //stream.Close();
                            response.EnsureSuccessStatusCode();

                            string response_str = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                            JObject uploadedFileInfo = JObject.Parse(response_str);
                            return uploadedFileInfo;
                        }

                        Position = stream.Position;
                    }
                }
            } while (true);
        }

        private async Task PrepareUploadLink(Uri uploadUri)
        {
            HttpResponseMessage response = await HttpClient.PostAsync(uploadUri, null).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        private async Task PrepareAuthorizedHttpClient(string filename, long size)
        {
            HttpClient = new HttpClient();
            string token = await GetUploadTokenAsync().ConfigureAwait(false);
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpClient.DefaultRequestHeaders.Add("fsp-filename", filename);
            HttpClient.DefaultRequestHeaders.Add("fsp-size", size.ToString(CultureInfo.InvariantCulture));
            HttpClient.DefaultRequestHeaders.Add("fsp-version", "1.0.0");
        }

        private static JObject GenerateJsonPayload(string filename, long size)
        {
            JObject json_payload = new JObject
            {
                { "directory_id", null },
                { "size", size },
                { "name", filename }
            };
            return json_payload;
        }
    }
}
