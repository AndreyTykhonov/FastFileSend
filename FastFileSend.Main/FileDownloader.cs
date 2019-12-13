using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public event Action OnEnd = delegate { };

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

            HttpResponseMessage response = await HttpClient.GetAsync(fileItem.Url);
            Stream stream = await response.Content.ReadAsStreamAsync();

            FileSize = stream.Length;

            var totalRead = 0L;
            var totalReads = 0L;
            var buffer = new byte[8192];
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
            OnEnd();
        }

        long GetRemoteLength(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://example.com/");
            req.Method = "HEAD";
            using (HttpWebResponse resp = (HttpWebResponse)(req.GetResponse()))
            {
                return resp.ContentLength;
            }
        }

        public void Report(long value)
        {
            long bytesDownloaded = value;
            double speedMb = bytesDownloaded / 1048576.0 / SpeedWatch.Elapsed.TotalSeconds;
            OnProgress((double)bytesDownloaded / FileSize * 100, speedMb);
        }

        string FindEmptyPath(FileItem fileItem)
        {
            for (int i = 0; i < 100; i++)
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
