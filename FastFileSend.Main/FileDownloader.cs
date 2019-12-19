using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main
{
    public class FileDownloader : IProgress<long>
    {
        HttpClient HttpClient { get; set; } = new HttpClient();
        Stopwatch SpeedWatch { get; set; }

        long FileSize { get; set; }

        public event Action<double, double> OnProgress = delegate { };

        async Task<string> GetUploadTokenAsync()
        {
            Uri fexGetUploadTokenUri = new Uri("https://api.fex.net/api/v1/anonymous/upload-token");
            HttpClient = new HttpClient();
            string json = await HttpClient.GetStringAsync(fexGetUploadTokenUri);
            return (string)JObject.Parse(json)["token"];
        }

        public long GetFileSize(string url)
        {
            long result = -1;

            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            req.Method = "HEAD";
            using (System.Net.WebResponse resp = req.GetResponse())
            {
                if (long.TryParse(resp.Headers.Get("Content-Length"), out long ContentLength))
                {
                    result = ContentLength;
                }
            }

            return result;
        }

        public async Task DownloadAsync(FileItem fileItem)
        {
            string path = Path.Combine(FilePathHelper.Downloads, fileItem.Name);

            if (File.Exists(path))
            {
                path = FindEmptyPath(fileItem);
            }

            path = path.Trim();

            FileStream fs = new FileStream(path, FileMode.Create);

            SpeedWatch = Stopwatch.StartNew();

            string token = await GetUploadTokenAsync();
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Uri fileUri = new Uri(fileItem.Url.Trim());

            HttpResponseMessage response = await HttpClient.GetAsync(fileUri, HttpCompletionOption.ResponseHeadersRead);
            Stream stream = await response.Content.ReadAsStreamAsync();

            FileSize = fileItem.Size;

            var totalRead = 0L;
            var totalReads = 0L;
            var buffer = new byte[16384];
            var isMoreToRead = true;

            do
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    isMoreToRead = false;
                }
                else
                {
                    await fs.WriteAsync(buffer, 0, read);

                    totalRead += read;
                    totalReads += 1;

                    Report(totalRead);

                    /*
                    if (totalReads % 2000 == 0)
                    {
                        Report(totalRead);
                    }
                    */
                }
            }
            while (isMoreToRead);

            SpeedWatch.Stop();

            fs.Close();
        }

        public void Report(long value)
        {
            long bytesDownloaded = value;
            double speedMb = bytesDownloaded / 1048576.0 / SpeedWatch.Elapsed.TotalSeconds;
            OnProgress((double)bytesDownloaded / FileSize, speedMb);
        }

        string FindEmptyPath(FileItem fileItem)
        {
            for (int i = 1; i < 100; i++)
            {
                string name = Path.GetFileNameWithoutExtension(fileItem.Name);
                string ext = Path.GetExtension(fileItem.Name);

                string path = Path.Combine(FilePathHelper.Downloads, $"{name} ({i}){ext}");
                if (!File.Exists(path))
                {
                    return path;
                }
            }

            throw new IOException("100 duplicates?!");
        }
    }
}
