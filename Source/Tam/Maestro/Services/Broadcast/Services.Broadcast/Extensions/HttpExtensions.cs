using System;
using System.Net.Http;

namespace Services.Broadcast.Extensions
{
    public static class HttpExtensions
    {
        public static T Get<T>(this HttpClient httpClient, string url)
        {
            var response = httpClient.GetAsync(url).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }

            return response.Content.ReadAsAsync<T>().GetAwaiter().GetResult();
        }
    }
}