using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;

namespace CSharp_sample
{
    public class Kabusapi_Positions
    {
        public static List<dynamic> Positions(
            string token,  
            string product = null, 
            string symbol = null, 
            string side = null, 
            string addinfo = null
            )
        {

            var builder = new UriBuilder(Constants.Url + "/positions");
            var param = System.Web.HttpUtility.ParseQueryString(builder.Query);
            if (!string.IsNullOrEmpty(product))
            {
                param["Product"] = product;
            }
            if (!string.IsNullOrEmpty(symbol))
            {
                param["symbol"] = symbol;
            }
            if (!string.IsNullOrEmpty(side))
            {
                param["side"] = side;
            }
            if (!string.IsNullOrEmpty(addinfo))
            {
                param["addinfo"] = addinfo;
            }

            builder.Query = param.ToString();
            string url = builder.ToString();
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-API-KEY", token);
            HttpResponseMessage response = client.SendAsync(request).Result;
            Console.WriteLine("{0} \n {1}", JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result), response.Headers);
            List<dynamic> ResponseObjectList = JsonConvert.DeserializeObject<List<dynamic>>(response.Content.ReadAsStringAsync().Result);
            return ResponseObjectList;

        }
    }
}
