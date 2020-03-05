using FastFileSend.Main.Models;
using FastFileSend.Main.Utils;
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

namespace FastFileSend.Main.RemoteFile
{
    /// <summary>
    /// Downloads FileItem from Fex.net.
    /// </summary>
    public class FileDownloader : ProgressableFile
    {
        async Task<string> GetUploadTokenAsync()
        {
            Uri fexGetUploadTokenUri = new Uri("https://api.fex.net/api/v1/anonymous/upload-token");
            string json = await HttpClient.GetStringAsync(fexGetUploadTokenUri).ConfigureAwait(false);
            return (string)JObject.Parse(json)["token"];
        }

        private string Folder { get; set; }

        public FileDownloader(string folder)
        {
            Folder = folder;
        }

        public async Task DownloadAsync(FileItem fileItem)
        {
            if (fileItem is null)
            {
                throw new ArgumentNullException(nameof(fileItem));
            }

            string path = Path.Combine(Folder, fileItem.Name);

            if (File.Exists(path))
            {
                path = FindEmptyPath(fileItem);
            }

            path = path.Trim();

            FileStream fs = new FileStream(path, FileMode.Create);

            string token = await GetUploadTokenAsync().ConfigureAwait(false);
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await HttpClient.GetAsync(fileItem.Url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            Size = fileItem.Size;

            var totalRead = 0L;
            var totalReads = 0L;
            var buffer = new byte[16384];
            var isMoreToRead = true;

            do
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (read == 0)
                {
                    isMoreToRead = false;
                }
                else
                {
                    await fs.WriteAsync(buffer, 0, read).ConfigureAwait(false);

                    totalRead += read;
                    totalReads += 1;

                    Position = totalRead;
                }
            }
            while (isMoreToRead);

            fs.Close();
        }

        string FindEmptyPath(FileItem fileItem)
        {
            for (int i = 1; i < 100; i++)
            {
                string name = Path.GetFileNameWithoutExtension(fileItem.Name);
                string ext = Path.GetExtension(fileItem.Name);

                string path = Path.Combine(Folder, $"{name} ({i}){ext}");
                if (!File.Exists(path))
                {
                    return path;
                }
            }

            #pragma warning disable CA1303
            throw new IOException("100 duplicates?!");
        }
    }
}
