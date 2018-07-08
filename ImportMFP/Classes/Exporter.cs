using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ImportMFP.Classes
{
    public class Exporter
    {
        private const string API_ACCESS_KEY = "86a65da9782b4e78bb6ed87097bc16b9";

        private const string REST_API_CONSUMER_KEY = "86a65da9782b4e78bb6ed87097bc16b9";

        private const string REST_API_SHARED_SECRET = "c121c635772e4fabb8c294cf7fd16ac9";

        private const string REQUEST_TOKEN_URL = "http://www.fatsecret.com/oauth/request_token";

        private const string SERVER_URL = "http://platform.fatsecret.com/rest/server.api";

        public void Export(MFPWeightData weightData)
        {
            RestClient requestTokenClient = new RestClient(REQUEST_TOKEN_URL);

            RestRequest request = new RestRequest(string.Empty, Method.GET);

            request.AddParameter("oauth_consumer_key", REST_API_CONSUMER_KEY);
            request.AddParameter("oauth_signature_method", "HMAC-SHA1");
            request.AddParameter("oauth_timestamp", DateTime.Now.Ticks);
            request.AddParameter("oauth_nonce", Guid.NewGuid().ToString("D"));
            request.AddParameter("oauth_version", "1.0");
            request.AddParameter("oauth_callback", "oob");

            request = SignRequest(request);

            IRestResponse response = requestTokenClient.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {

            }
            else
            {
                throw new Exception("Invalid status code from request token: " + response.StatusCode.ToString());
            }
        }

        private RestRequest SignRequest(RestRequest request)
        {
            string signatureBaseString = "{0}&{1}&{2}";

            StringBuilder normalizedParameters = new StringBuilder();

            List<Parameter> uriParams = request.Parameters.OrderBy(x => x.Name).ToList();

            foreach(Parameter p in uriParams)
            {
                if (normalizedParameters.Length > 0)
                {
                    normalizedParameters.Append("&");
                }

                normalizedParameters.AppendFormat("{0}={1}", p.Name, p.Value);
            }

            signatureBaseString = string.Format(signatureBaseString, request.Method, UrlEncode(REQUEST_TOKEN_URL), UrlEncode(normalizedParameters.ToString()));

            string signature = Encode(signatureBaseString, string.Format("{0}&{1}", REST_API_SHARED_SECRET, string.Empty));

            request.AddParameter("oauth_signature", UrlEncode(signature));

            return request;
        }

        private string UrlEncode(string input)
        {
            string lower = HttpUtility.UrlEncode(input);

            Regex reg = new Regex(@"%[a-f0-9]{2}");

            return reg.Replace(lower, m => m.Value.ToUpperInvariant());
        }


        private string Encode(string input, string key)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            byte[] keyArray = Encoding.ASCII.GetBytes(key);

            using (var myhmacsha1 = new HMACSHA1(keyArray))
            {
                var hashArray = myhmacsha1.ComputeHash(byteArray);
                return hashArray.Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
            }
        }
    }
}
