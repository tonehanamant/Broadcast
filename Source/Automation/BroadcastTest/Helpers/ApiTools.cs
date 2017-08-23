using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Serializers;

namespace BroadcastTest.Helpers
{
    public class ApiTools
    {
        public ApiTools()
        {

        }

        public void GetHttpRequest(string url)
        {
            var client = new RestClient();
            //client.JsonSerializer = new YourCustomSerializer();

            //client.Authenticator = new HttpBasicAuthenticator("username", "password");

            var request = new RestRequest("resource/{id}", Method.POST);
            request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
            request.AddUrlSegment("id", "123"); // replaces matching token in request.Resource

            // easily add HTTP Headers
      

        }

        /*
        public void PostHttpRequestWithAttachedFile(string url, string filePath, string fileName)
        {
            var client = new RestClient("http://example.com");
            // client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest("resource/{id}", Method.POST);
            request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
            request.AddUrlSegment("id", "123"); // replaces matching token in request.Resource

            // easily add HTTP Headers
            request.AddHeader("header", "value");

            // add files to upload (works with compatible verbs)
            request.AddFile(filePath);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string

            // or automatically deserialize result
            // return content type is sniffed but can be explicitly set via RestClient.AddHandler();
            RestResponse<Person> response2 = client.Execute<Person>(request);
            var name = response2.Data.Name;

            // easy async support
            client.ExecuteAsync(request, response =>
            {
                Console.WriteLine(response.Content);
            });

            // async with deserialization
            var asyncHandle = client.ExecuteAsync<Person>(request, response =>
            {
                Console.WriteLine(response.Data.Name);
            });

            // abort the request on demand
            asyncHandle.Abort();
        }
         */


    }
}
