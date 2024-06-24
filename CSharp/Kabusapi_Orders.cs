using System;
using Newtonsoft.Json;
using System.Net.Http;

namespace CSharp_sample
{
    public class Kabusapi_Orders
    {
        public static List<dynamic> Orders(
            string token,
            string product = null, 
            string id = null, 
            string updTime = null, 
            string details = null, 
            string symbol = null, 
            string state = null,
            string side = null,
            string cashMargin = null
            )
        {

            var builder = new UriBuilder(Constants.Url + "/orders");
            var param = System.Web.HttpUtility.ParseQueryString(builder.Query);
            if (!string.IsNullOrEmpty(product))
            {
                param["product"] = product;
            }
            if (!string.IsNullOrEmpty(id))
            {
                param["id"] = id;
            }
            if (!string.IsNullOrEmpty(updTime))
            {
                param["updtime"] = updTime;
            }
            if (!string.IsNullOrEmpty(details))
            {
                param["details"] = details;
            }
            if (!string.IsNullOrEmpty(symbol))
            {
                param["symbol"] = symbol;
            }
            if (!string.IsNullOrEmpty(state))
            {
                param["state"] = state;
            }
            if (!string.IsNullOrEmpty(side))
            {
                param["side"] = side;
            }
            if (!string.IsNullOrEmpty(cashMargin))
            {
                param["cashmargin"] = cashMargin;
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
