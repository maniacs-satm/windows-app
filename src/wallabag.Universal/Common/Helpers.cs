using System.Threading.Tasks;
using wallabag.Services;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace wallabag.Common
{
    public static class Helpers
    {
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
        public static bool IsConnectedToInternet()
        {
            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            return (connectionProfile != null && connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
        }
    }
}
