using Common.Services.ApplicationServices;
using Newtonsoft.Json;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices
{
    public interface IWhosWatchingTvService: IApplicationService
    {
        List<WhosWatchingTvSearchResult> FindPrograms(string keyword);
    }

    public class WhosWatchingTvService : IWhosWatchingTvService
    {
        public List<WhosWatchingTvSearchResult> FindPrograms(string keyword)
        {
            var requestUrl = "http://fyi.uat.ecdapi.net/stores-active/contentInstance/program/search";
            requestUrl = requestUrl + "?attributes=[\"searchableTitles.value.en\"]";
            requestUrl = requestUrl + "&numberOfResults=20";
            requestUrl = requestUrl + "&annotation={\"unconditionalAttributes\": [\"searchableTitles.value.en\", \"searchableTitles.type\"]}";
            requestUrl = requestUrl + "&attributeWeights={\"searchableTitles.value.en\":1}";
            requestUrl = requestUrl + "&query=" + keyword;

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("api_key:e35ad4e3d9b47d060160c24de1dfd5bc");
                var jsonResult = webClient.DownloadString(requestUrl);
                var result = JsonConvert.DeserializeObject<List<WhosWatchingTvSearchResult>>(jsonResult);
                return result;
            }
        }
    }
}
