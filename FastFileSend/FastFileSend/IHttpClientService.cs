using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace FastFileSend
{
    public interface IHttpClientService
    {
        HttpClientHandler Handler { get; set; }
    }
}
