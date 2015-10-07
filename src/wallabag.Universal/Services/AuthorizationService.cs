using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Web.Http;

namespace wallabag.Services
{
    class AuthorizationService
    {
        private const string _ClientID = "1_3bcbxd9e24g0gk4swg0kwgcwg4o8k8g4g888kwc44gcc0gwwk4";
        private const string _ClientSecret = "4ok2x70rlfokc8g0wws8c8kwcokw80k44sg48goc0ok4w0so0k";

        private static DateTime _LastRequestDateTime;
        private static string _RefreshToken = string.Empty;
        private static string _AccessToken = string.Empty;

        public static string GetAccessToken()
        {
            TimeSpan duration = DateTime.UtcNow.Subtract(_LastRequestDateTime);
            if (duration.Seconds > 3600)
                RefreshToken();

            return _AccessToken;
        }

        public static async Task<bool> RequestTokenAsync(string Username, string Password, string Url)
        {
            Uri requestUri = new Uri($"{Url}/oauth/v2/token");

            using (HttpClient http = new HttpClient())
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("grant_type", "password");
                parameters.Add("client_id", _ClientID);
                parameters.Add("client_secret", _ClientSecret);
                parameters.Add("username", Username);
                parameters.Add("password", Password);

                var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                var response = await http.PostAsync(requestUri, content);

                if (!response.IsSuccessStatusCode)
                    return false;

                var responseString = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(responseString);
                _AccessToken = result.access_token;
                _RefreshToken = result.refresh_token;
                _LastRequestDateTime = DateTime.UtcNow;

                return true;
            }
        }
        public static void RefreshToken()
        {

        }
    }
}
