using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using wallabag.Services;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace wallabag.Common
{
    public static class Helpers
    {
        public static string DATABASE_PATH { get; } = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "wallabag.db");

        /// <summary>
        /// There are several languages. To access them from code-behind this way is required.
        /// </summary>
        /// <param name="resourceName">The name of the resource in the dictionary.</param>
        /// <returns>A string representing the translation for the current language.</returns>
        public static string LocalizedString(string resourceName)
        {
            return ResourceLoader.GetForCurrentView().GetString(resourceName);
        }
        public static async Task AddHttpHeadersAsync(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("WSSE", "profile=\"UsernameToken\"");
            client.DefaultRequestHeaders.Add("X-WSSE", await AuthenticationService.GetAuthenticationHeaderAsync());
            client.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("wallabag for Windows"));
        }
        public static bool IsConnectedToTheInternet
        {
            get
            {
                ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                return (connectionProfile != null && connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
            }
        }

        public static async Task<HttpResponseMessage> ExecuteHttpRequestAsync(HttpRequestMethod httpRequestMethod, string RelativeUriString, Dictionary<string, object> parameters = default(Dictionary<string, object>))
        {
            using (HttpClient http = new HttpClient())
            {
                await AddHttpHeadersAsync(http);

                Uri requestUri = new Uri($"{AppSettings.wallabagUrl}/api{RelativeUriString}.json");
                var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

                string httpMethodString = "GET";
                switch (httpRequestMethod)
                {
                    case HttpRequestMethod.Delete: httpMethodString = "DELETE"; break;
                    case HttpRequestMethod.Patch: httpMethodString = "PATCH"; break;
                    case HttpRequestMethod.Post: httpMethodString = "POST"; break;
                    case HttpRequestMethod.Put: httpMethodString = "PUT"; break;
                }

                var method = new HttpMethod(httpMethodString);
                var request = new HttpRequestMessage(method, requestUri) { Content = content };
                try { return await http.SendRequestAsync(request); }
                catch { return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable); }
            }
        }
        public enum HttpRequestMethod { Delete, Get, Patch, Post, Put }

        public static bool IsPhone
        {
            get
            {
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
                    return true;
                else return false;
            }
        }
    }
}
