using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using wallabag.Common;
using wallabag.ViewModel;
using Windows.UI;

namespace wallabag.DataModel
{
    public class Item
    {
        public ApplicationSettings AppSettings { get { return ApplicationSettings.Instance; } }

        public Item()
        {
            UniqueId = Guid.NewGuid().ToString();
        }
        public Item(String uniqueId, String title, String content, Uri url, bool isRead, bool isFavourite)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Content = content;
            this.Url = url;
            this.IsRead = isRead;
            this.IsFavourite = isFavourite;
        }

        public string UniqueId { get; set; }
        private string _title;
        public string Title
        {
            get
            {
                // Regular expression to remove multiple whitespaces (including newline etc.) in title.
                Regex r = new Regex("\\s+");
                return r.Replace(_title, " ");
            }
            set { _title = value; }
        }
        public string Content { get; set; }
        public string ContentWithTitle
        {
            get
            {
                var content =
                    "<html><head><link rel=\"stylesheet\" href=\"ms-appx-web:///Assets/css/wallabag.css\" type=\"text/css\" media=\"screen\" />" + CSS() + "</head>" +
                        "<h1 class=\"wallabag-header\">" + Title + "</h1>" +
                        this.Content +
                    "</html>";
                return content;
            }
        }
        public Uri Url { get; set; }
        public bool IsRead { get; set; }
        public bool IsFavourite { get; set; }

        private string CSSproperty(string name, object value)
        {
            if (value.GetType() != typeof(Color))
            {
                return string.Format("{0}: {1};", name, value.ToString());
            }
            else
            {
                var color = (Color)value;
                var tmpColor = string.Format("rgba({0}, {1}, {2}, {3})", color.R, color.G, color.B, color.A);
                return string.Format("{0}: {1};", name, tmpColor);
            }
        }
        private string CSS()
        {
            double fontSize = AppSettings["fontSize", 18];
            double lineHeight = AppSettings["lineHeight", 1.5];

            SettingsViewModel tmpSettingsVM = new SettingsViewModel();

            string css = "body {" +
                CSSproperty("font-size", fontSize + "px") +
                CSSproperty("line-height", lineHeight.ToString().Replace(",", ".")) +
                CSSproperty("color", tmpSettingsVM.textColor.Color) +
                CSSproperty("background", tmpSettingsVM.Background.Color) +
#if WINDOWS_APP
 CSSproperty("max-width", "960px") +
                CSSproperty("margin", "0 auto") +
                CSSproperty("padding", "0 20px") +
#endif
 "}";
            return "<style>" + css + "</style>";
        }

        public override string ToString()
        {
            return this.Title;
        }
    }


}
