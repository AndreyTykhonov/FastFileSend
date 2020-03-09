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



        public async Task DownloadAsync(Stream fileStream, Uri uri, long size)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (fileStream is null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            // for progress reporting
            Size = size;

            string token = await GetUploadTokenAsync().ConfigureAwait(false);
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

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
                    await fileStream.WriteAsync(buffer, 0, read).ConfigureAwait(false);

                    totalRead += read;
                    totalReads += 1;

                    Position = totalRead;
                }
            }
            while (isMoreToRead);
        }
    }
}
