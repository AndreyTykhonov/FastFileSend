using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main.RemoteFile
{
    /// <summary>
    /// Fex.net requires PATCH method to upload files.
    /// </summary>
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent iContent, long position)
        {
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = iContent
            };

            request.Headers.Add("fsp-offset", position.ToString());

            HttpResponseMessage response = await client.SendAsync(request);

            return response;
        }
    }
}
