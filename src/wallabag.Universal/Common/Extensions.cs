using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace wallabag.Common
{
    public static class WebViewExtensions
    {
        public static string GetHTML(DependencyObject obj)
        {
            return (string)obj.GetValue(HTMLProperty);
        }

        public static void SetHTML(DependencyObject obj, string value)
        {
            obj.SetValue(HTMLProperty, value);
        }

        // Using a DependencyProperty as the backing store for HTML.  This enables animation, styling, binding, etc... 
        public static readonly DependencyProperty HTMLProperty =
            DependencyProperty.RegisterAttached("HTML", typeof(string), typeof(WebViewExtensions), new PropertyMetadata(0, new PropertyChangedCallback(OnHTMLChanged)));

        private static void OnHTMLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            WebView wv = d as WebView;
            if (wv != null)
            {
                wv.NavigateToString((string)e.NewValue);
            }
        }
    }
    public static class HttpClientExtensions
    {
        // copied and modified from StackOverflow <http://stackoverflow.com/a/26218765> - thanks to 'ricochete' :)
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, IHttpContent iContent)
        {
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = iContent
            };

            HttpResponseMessage response = new HttpResponseMessage();
            response = await client.SendRequestAsync(request);

            return response;
        }
    }
    public static class TagExtensions
    {
        public static string ToCommaSeparatedString(this IList<string> list)
        {
            string result = string.Empty;
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    result += item.ToString();
                    result += ",";
                }
                if (result.EndsWith(","))
                    result = result.Remove(result.Length - 1);
            }

            return result;
        }
        public static IList<string> ToList(this string str)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrWhiteSpace(str))
            {
                result = str.Split(",".ToCharArray()).ToList();
            }
            return result;
        }
    }
}
