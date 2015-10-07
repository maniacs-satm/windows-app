using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wallabag.Services
{
    class AuthorizationService
    {
        private const string _ClientID = "1_3bcbxd9e24g0gk4swg0kwgcwg4o8k8g4g888kwc44gcc0gwwk4";
        private const string _ClientSecret = "4ok2x70rlfokc8g0wws8c8kwcokw80k44sg48goc0ok4w0so0k";

        private string _RefreshToken;
        private string _oAuthToken;

        public void RequestToken(string Username, string Password, string Url)
        {

        }
        public void RefreshToken()
        {

        }
    }
}
