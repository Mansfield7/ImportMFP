using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImportMFP.Classes.REST
{
    public class KeyValuePairSerializer : IDeserializer
    {
        public string ContentType
        {
            get { return "text/html"; }
            set { }
        }

        public string RootElement
        {
            get { return string.Empty; }
            set { }
        }

        public string DateFormat
        {
            get { return string.Empty; }
            set { }
        }

        public string Namespace
        {
            get { return string.Empty; }
            set { }
        }

        public KeyValuePairSerializer()
        {

        }

        public T Deserialize<T>(IRestResponse response)
        {
            T newT = (T)Activator.CreateInstance(typeof(T));

            List<KeyValuePair<string, string>> parameters = ParseParams(response.Content);

            foreach(KeyValuePair<string, string> prop in parameters)
            {
                PropertyInfo propInfo = newT.GetType().GetProperty(prop.Key, BindingFlags.Public | BindingFlags.Instance);

                if (propInfo != null)
                {
                    propInfo.SetValue(newT, prop.Value, null);
                }
            }           

            return newT;
        }

        private List<KeyValuePair<string, string>> ParseParams(string input)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            string[] pairs = input.Split('&');

            foreach (string pair in pairs)
            {
                string[] vals = pair.Split('=');

                KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(vals[0], vals[1]);

                parameters.Add(kvp);
            }

            return parameters;
        }
            
        public static KeyValuePairSerializer Default
        {
            get
            {
                return new KeyValuePairSerializer();
            }
        }
    }
}
