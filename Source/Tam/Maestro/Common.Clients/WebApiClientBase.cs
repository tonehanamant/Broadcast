using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Services.Cable.Entities;

namespace Tam.Maestro.Common.Clients
{
    public class WebApiClientBase
    {
        protected readonly HttpClient client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });

        protected T _Post<T, U>(string uri, U payload)
        {
            T result = default(T);

            var stringPayload = Task.Run(() => JsonConvert.SerializeObject(payload)).Result;
            var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            result = _MakeRequestAndCheckResponse<T>(() => client.PostAsync(uri, content).Result);

            return result;
        }

        protected T _Get<T>(string uri)
        {
            var result = _MakeRequestAndCheckResponse<T>(() => client.GetAsync(uri).Result);

            return result;
        }

        protected T _Put<T, U>(string uri, U payload)
        {
            T result = default(T);

            var stringPayload = Task.Run(() => JsonConvert.SerializeObject(payload)).Result;
            var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            result = _MakeRequestAndCheckResponse<T>(() => client.PutAsync(uri, content).Result);

            return result;
        }

        protected T _GetWithBaseResponse<T>(string uri)
        {
            var result = _MakeRequestAndCheckResponse<BaseResponse<T>>(() => client.GetAsync(uri).Result);

            if (result.Success == false)
            {
                throw new Exception("Error during web API call: " + result.Message);
            }

            return result.Data;
        }

        protected T _PostWithBaseResponse<T, U>(string uri, U payload)
        {
            var stringPayload = Task.Run(() => JsonConvert.SerializeObject(payload)).Result;
            var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            var result = _MakeRequestAndCheckResponse<BaseResponse<T>>(() => client.PostAsync(uri, content).Result);

            if (result.Success == false)
            {
                throw new Exception("Error on server: " + result.Message);
            }

            return result.Data;
        }

        protected T _MakeRequestAndCheckResponse<T>(Func<HttpResponseMessage> request)
        {
            var response = request();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"If 500 error could be authentication/role issue, Error with web API call: {response}");
            }

            T result;

            try
            {
                result = response.Content.ReadAsAsync<T>().Result;
            }
            catch (Exception e)
            {
                throw new Exception($"Error occurred during deserialization of response: {e}");
            }

            return result;
        }

        private T _MakeRequestAndCheckResponseHttps<T>(Func<HttpResponseMessage> request)
        {
            var response = request();
            T result;
            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new WebException(response.ToString());
                }
                result = response.Content.ReadAsAsync<T>().Result;
            }
            catch (WebException e)
            {
                throw;
            }

            return result;
        }

        protected T _PostWithHttps<T, U>(string uri, U payload)
        {
            ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

            T result = default(T);

            var stringPayload = Task.Run(() => JsonConvert.SerializeObject(payload)).Result;
            var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            result = _MakeRequestAndCheckResponseHttps<T>(() => client.PostAsync(uri, content).Result);

            return result;
        }

        protected T _PostFormData<T>(string uri, Dictionary<string, string> formValues)
        {
            ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

            var content = new FormUrlEncodedContent(formValues);

            var result = _MakeRequestAndCheckResponse<T>(() => client.PostAsync(uri, content).Result);
            return result;
        }
    }
}