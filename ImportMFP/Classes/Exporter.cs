using ImportMFP.Classes.REST;
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
        
        private const string REQUEST_ACCESS_TOKEN_URL = "http://www.fatsecret.com/oauth/access_token";

        private const string SERVER_API_URL = "http://platform.fatsecret.com/rest/server.api";

        public void Export(MFPWeightData weightData)
        {
            //DoAuthStuff();

            string token = "b70eba355e344b36957749d70f4dc7f8";
            string token_secret = "718b836cb6f446349de85d031cbb3572";

            RestClient updateWeightClient = new RestClient(SERVER_API_URL);

            RestRequest request = new RestRequest(string.Empty, Method.POST);

            string nonce = string.Empty;
            string timestamp = string.Empty;

            timestamp = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            nonce = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(timestamp + timestamp + timestamp));

            request.AddParameter("oauth_consumer_key", REST_API_CONSUMER_KEY);
            request.AddParameter("oauth_signature_method", "HMAC-SHA1");
            request.AddParameter("oauth_timestamp", timestamp);
            request.AddParameter("oauth_nonce", nonce);
            request.AddParameter("oauth_version", "1.0");
            request.AddParameter("method", "weight.update");
            request.AddParameter("oauth_token", token);
            request.AddParameter("current_weight_kg", 100.0);

            request.AddParameter("format", "json");
            request.AddParameter("date", TransformDate(DateTime.Now.AddMonths(-1)));
            request.AddParameter("weight_type", "lb");
            //request.AddParameter("comment", "imported through API from MFP"); //this causes an issue with signature, I think spaces are being encoded incorrectly

            request = SignRequest(request, updateWeightClient.BaseUrl.ToString(), token_secret);

            IRestResponse<RequestTokenResponse> response = updateWeightClient.Execute<RequestTokenResponse>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {

            }
        }

        private int TransformDate(DateTime input)
        {
            return Convert.ToInt32((input - new DateTime(1970, 1, 1)).TotalDays);
        }

        private void DoAuthStuff()
        {
            RestClient requestTokenClient = new RestClient(REQUEST_TOKEN_URL);
            requestTokenClient.AddHandler("text/html", KeyValuePairSerializer.Default);

            RestRequest request = new RestRequest(string.Empty, Method.GET);

            string nonce = Guid.NewGuid().ToString("D");
            string timestamp = DateTime.Now.Ticks.ToString();

            timestamp = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            nonce = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(timestamp + timestamp + timestamp));

            request.AddParameter("oauth_consumer_key", REST_API_CONSUMER_KEY);
            request.AddParameter("oauth_signature_method", "HMAC-SHA1");
            request.AddParameter("oauth_timestamp", timestamp);
            request.AddParameter("oauth_nonce", nonce);
            request.AddParameter("oauth_version", "1.0");
            request.AddParameter("oauth_callback", "oob");

            request = SignRequest(request, requestTokenClient.BaseUrl.ToString(), string.Empty);
            request.OnBeforeDeserialization = resp => { resp.ContentType = "text/html"; };

            IRestResponse<RequestTokenResponse> response = requestTokenClient.Execute<RequestTokenResponse>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string user_code = string.Empty;

                RestClient requestAccessTokenClient = new RestClient(REQUEST_ACCESS_TOKEN_URL);
                requestAccessTokenClient.AddHandler("text/html", KeyValuePairSerializer.Default);

                RestRequest request2 = new RestRequest(string.Empty, Method.GET);

                timestamp = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                nonce = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(timestamp + timestamp + timestamp));

                request2.AddParameter("oauth_consumer_key", REST_API_CONSUMER_KEY);
                request2.AddParameter("oauth_token", response.Data.oauth_token);
                request2.AddParameter("oauth_verifier", user_code);
                request2.AddParameter("oauth_signature_method", "HMAC-SHA1");
                request2.AddParameter("oauth_timestamp", timestamp);
                request2.AddParameter("oauth_nonce", nonce);
                request2.AddParameter("oauth_version", "1.0");

                request2 = SignRequest(request2, requestAccessTokenClient.BaseUrl.ToString(), response.Data.oauth_token_secret);
                request2.OnBeforeDeserialization = resp => { resp.ContentType = "text/html"; };

                IRestResponse<RequestTokenResponse> response2 = requestAccessTokenClient.Execute<RequestTokenResponse>(request2);

                if (response2.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //todo - oauth_token and oauth_token_secret from this request can be used to make real API calls - all code above should be single-use 
                    //token = "b70eba355e344b36957749d70f4dc7f8"
                    //token_secret = "718b836cb6f446349de85d031cbb3572"
                }
                else
                {
                    throw new Exception("Invalid status code from request token: " + response.StatusCode.ToString());
                }
            }
            else
            {
                throw new Exception("Invalid status code from request token: " + response.StatusCode.ToString());
            }
        }

        /// <summary>
        /// Sign a request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private RestRequest SignRequest(RestRequest request, string url, string oauth_token_secret)
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

            signatureBaseString = string.Format(signatureBaseString, request.Method, UrlEncode(url), UrlEncode(normalizedParameters.ToString()));

            string signature = GetSignature(signatureBaseString, string.Format("{0}&{1}", REST_API_SHARED_SECRET, oauth_token_secret));

            request.AddParameter("oauth_signature", signature);

            return request;
        }

        /// <summary>
        /// Apply URL encoding to the input.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The encoded string.</returns>
        private string UrlEncode(string input)
        {
            string lower = HttpUtility.UrlEncode(input);

            Regex reg = new Regex(@"%[a-f0-9]{2}");

            return reg.Replace(lower, m => m.Value.ToUpperInvariant());
        }

        /// <summary>
        /// Get a signature.
        /// </summary>
        /// <param name="input">The input string to sign.</param>
        /// <param name="key">The signing key.</param>
        /// <returns>The signature.</returns>
        private string GetSignature(string input, string key)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            byte[] keyArray = Encoding.ASCII.GetBytes(key);

            using (var myhmacsha1 = new HMACSHA1(keyArray))
            {
                var hashArray = myhmacsha1.ComputeHash(byteArray);
                return Convert.ToBase64String(hashArray);
            }
        }
    }
}
