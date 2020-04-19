using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main
{
    public class GithubVersionInfo
    {
        public string Tag { get; set; }
        public string Link { get; set; }
    }

    public class GithubVersion
    {
        public async Task<GithubVersionInfo> GetLastVersion(string assetName)
        {
            using (HttpClient http = new HttpClient())
            {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Mobile Safari/537.36");
                Uri uriGithubApiReleases = new Uri("https://api.github.com/repos/AndreyTykhonov/FastFileSend/releases");
                string response = await http.GetStringAsync(uriGithubApiReleases).ConfigureAwait(false);

                GithubVersionInfo versionInfo = new GithubVersionInfo();

                JArray releaseArray = JArray.Parse(response);
                JObject lastRelease = (JObject)releaseArray[0];

                versionInfo.Tag = lastRelease["tag_name"].Value<string>();
                JArray assets = (JArray)lastRelease["assets"];

                JObject targetAsset = assets.Select(x => (JObject)x).First(x => (string)x["name"] == assetName);
                versionInfo.Link = (string)targetAsset["browser_download_url"];

                return versionInfo;
            }
        }
    }
}
