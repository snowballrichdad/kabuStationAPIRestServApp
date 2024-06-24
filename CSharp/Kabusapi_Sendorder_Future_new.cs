using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text;

namespace CSharp_sample
{
    public class Kabusapi_Sendorder_Future_new
    {
        public static dynamic SendOrderFuture(string token, dynamic obj)
        {
            var url = Constants.Url + "/sendorder/future";

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("ContentType", "application/json");
            request.Headers.Add("X-API-KEY", token);
            request.Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.SendAsync(request).Result;
            Console.WriteLine("{0} \n {1}", JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result), response.Headers);
            dynamic ResponseObject = JsonConvert.DeserializeObject<ExpandoObject>(response.Content.ReadAsStringAsync().Result);


            return ResponseObject;
        }
    }
}
