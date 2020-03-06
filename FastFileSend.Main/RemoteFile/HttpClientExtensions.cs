using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "PATCH";

            foreach (var header in client.DefaultRequestHeaders)
            {
                request.Headers.Add(header.Key, header.Value.First());
            }

            request.Headers.Add("fsp-offset", position.ToString(CultureInfo.InvariantCulture));

            using (var stream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                await iContent.CopyToAsync(stream).ConfigureAwait(false);
            }

            // Send the request to the server and wait for the response:  
            using (var response = await request.GetResponseAsync().ConfigureAwait(false))
            {
                // Get a stream representation of the HTTP web response:  
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var message = reader.ReadToEnd();
                        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(message) };
                    }
                }
            }

        }
    }
}
