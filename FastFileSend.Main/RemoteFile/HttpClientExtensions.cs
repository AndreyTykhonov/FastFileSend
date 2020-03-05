using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (requestUri is null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            if (iContent is null)
            {
                throw new ArgumentNullException(nameof(iContent));
            }

            var method = new HttpMethod("PATCH");
            using (var request = new HttpRequestMessage(method, requestUri) { Content = iContent })
            {
                request.Headers.Add("fsp-offset", position.ToString(CultureInfo.InvariantCulture));

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                return response;
            }
        }
    }
}
