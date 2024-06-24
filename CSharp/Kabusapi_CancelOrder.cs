using System;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Dynamic;

namespace CSharp_sample
{
    public class Kabusapi_CancelOrder
    {
        public static dynamic CancelOrder(string token, string orderId)
        {
            var obj = new
            {
                OrderId = orderId,
                Password = Constants.LoginPassword
            };
            var url = Constants.Url + "/cancelorder";

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Put, url);
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
