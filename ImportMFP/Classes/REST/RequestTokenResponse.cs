using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportMFP.Classes.REST
{
    public class RequestTokenResponse
    {
        public string oauth_callback_confirmed { get; set; }

        public string oauth_token { get; set; }

        public string oauth_token_secret { get; set; }
    }
}
