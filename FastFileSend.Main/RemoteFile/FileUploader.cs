using FastFileSend.Main.Models;
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
            string json = await HttpClient.GetStringAsync(fexGetUploadTokenUri);
            return (string) JObject.Parse(json)["token"];
        }

        async Task<FexFileUploadDataInfo> GetUploadDataInfoAsync(JObject json_payload)
        {
            HttpContent postData = new StringContent(json_payload.ToString(), Encoding.Unicode, "application/json");

            Uri fexFileUri = new Uri("https://api.fex.net/api/v1/anonymous/file");
            HttpResponseMessage response = await HttpClient.PostAsync(fexFileUri, postData);

            string response_str = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FexFileUploadDataInfo>(response_str);
        }

        public async Task<FileItem> UploadAsync(string filename, Stream stream)
        {
            string fileName = filename;
            Size = stream.Length;

            await PrepareAuthorizedHttpClient(fileName, Size);

            JObject json_payload = GenerateJsonPayload(fileName, Size);
            FexFileUploadDataInfo uploadDataInfo = await GetUploadDataInfoAsync(json_payload);

            Uri uploadUri = new Uri(uploadDataInfo.location);

            // 5 times retry.
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    await PrepareUploadLink(uploadUri);
                    break;
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(500);
                }
            }

            JObject uploadedFileInfo = await StartUploadAsync(stream, uploadUri);

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

                StreamContent streamContent = new StreamContent(bufferStream);
                streamContent.Headers.Add("Content-Type", "application/octet-stream");

                HttpResponseMessage response = await HttpClient.PatchAsync(uploadUri, streamContent, sendPosition);

                bool finalPush = stream.Position == stream.Length;

                if (finalPush)
                {
                    stream.Close();
                    response.EnsureSuccessStatusCode();

                    string response_str = await response.Content.ReadAsStringAsync();

                    JObject uploadedFileInfo = JObject.Parse(response_str);
                    return uploadedFileInfo;
                }

                Position = stream.Position;
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
    }
}
