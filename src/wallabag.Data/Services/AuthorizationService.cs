using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using wallabag.Common;
using Windows.Web.Http;

namespace wallabag.Data.Services
{
    public class AuthorizationService
    {
        private const string _ClientID = "1_60ngzbjmcd0cg80scsw84sooko4okco48occk404c8kkg4040s";
        private const string _ClientSecret = "2w2zaixkgyioo0wws4wwc0sgsg8cko444wcs84swwoc84cs0o8";
        private static Uri _RequestUri { get { return new Uri($"{AppSettings.wallabagUrl}/oauth/v2/token"); } }

        private static DateTime _LastRequestDateTime
        {
            get
            {
                return DateTime.Parse(Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(_LastRequestDateTime)] as string
                    ?? DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0)).ToString());
            }
            set { Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(_LastRequestDateTime)] = value.ToString(); }
        }

        public static async Task<string> GetAccessTokenAsync()
        {
            TimeSpan duration = DateTime.UtcNow.Subtract(_LastRequestDateTime);
            if (duration.TotalSeconds > 3600)
                await RefreshTokenAsync();

            return AppSettings.AccessToken;
        }

        public static async Task<bool> RequestTokenAsync(string Username, string Password, string Url)
        {
            using (HttpClient http = new HttpClient())
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("grant_type", "password");
                parameters.Add("client_id", _ClientID);
                parameters.Add("client_secret", _ClientSecret);
                parameters.Add("username", Username);
                parameters.Add("password", Password);

                var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                var response = await http.PostAsync(_RequestUri, content);

                if (!response.IsSuccessStatusCode)
                    return false;

                var responseString = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(responseString);
                AppSettings.AccessToken = result.access_token;
                AppSettings.RefreshToken = result.refresh_token;

                _LastRequestDateTime = DateTime.UtcNow;

                return true;
            }
        }
        public static async Task<bool> RefreshTokenAsync()
        {
            using (HttpClient http = new HttpClient())
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("grant_type", "refresh_token");
                parameters.Add("client_id", _ClientID);
                parameters.Add("client_secret", _ClientSecret);
                parameters.Add("refresh_token", AppSettings.RefreshToken);

                var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                var response = await http.PostAsync(_RequestUri, content);

                if (!response.IsSuccessStatusCode)
                    return false;

                var responseString = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(responseString);
                AppSettings.AccessToken = result.access_token;
                AppSettings.RefreshToken = result.refresh_token;
                _LastRequestDateTime = DateTime.UtcNow;

                return true;
            }
        }
    }
}
