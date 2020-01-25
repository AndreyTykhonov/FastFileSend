using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Android.Net;

namespace FastFileSend.Droid
{
    class HttpClientService : IHttpClientService
    {
        public HttpClientHandler Handler { get; set; } = new AndroidClientHandler();
    }
}