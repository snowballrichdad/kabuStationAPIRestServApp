using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web;
using System.Dynamic;

namespace CSharp_sample
{
    public class Kabusapi_Symbolname_Future
    {
        public static dynamic SymbolNameFuture(string token, string FutureCode, int DerivMonth)
        {
            var builder = new UriBuilder(Constants.Url + "/symbolname/future");
            var param = HttpUtility.ParseQueryString(builder.Query);

            if (!string.IsNullOrEmpty(FutureCode))
            {
                param["FutureCode"] = FutureCode;
            }
            param["DerivMonth"] = DerivMonth.ToString();

            builder.Query = param.ToString();
            string url = builder.ToString();
            
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-API-KEY", token);
            HttpResponseMessage response = client.SendAsync(request).Result;
            Console.WriteLine("{0} \n {1}", JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result), response.Headers);
            dynamic ResponseObject = JsonConvert.DeserializeObject<ExpandoObject>(response.Content.ReadAsStringAsync().Result);

            return ResponseObject;
        }
    }
}
