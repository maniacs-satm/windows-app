using wallabag.Common;

namespace wallabag.ViewModels
{
    public class SettingsPageViewModel : Common.Mvvm.ViewModelBase
    {
        public override string ViewModelIdentifier { get; set; } = "SettingsPageViewModel";

        public string Username
        {
            get { return AppSettings.Username; }
            set { AppSettings.Username = value; }
        }
        public string Password
        {
            get { return AppSettings.Password; }
            set { AppSettings.Password = value; }
        }
        public string wallabagUrl
        {
            get { return AppSettings.wallabagUrl; }
            set { AppSettings.wallabagUrl = value; }
        }

        public double FontSize
        {
            get { return AppSettings.FontSize; }
            set { AppSettings.FontSize = value; }
        }
        public double LineHeight
        {
            get { return AppSettings.LineHeight; }
            set { AppSettings.LineHeight = value; }
        }
        public string FontFamily
        {
            get { return AppSettings.FontFamily; }
            set { AppSettings.FontFamily = value; }
        }
        public string ColorScheme
        {
            get { return AppSettings.ColorScheme; }
            set { AppSettings.ColorScheme = value; }
        }

        public bool UseSystemAccentColor
        {
            get { return AppSettings.UseSystemAccentColor; }
            set { AppSettings.UseSystemAccentColor = value; }
        }
        public bool HamburgerPositionIsRight
        {
            get { return AppSettings.HamburgerPositionIsRight; }
            set { AppSettings.HamburgerPositionIsRight = value; }
        }
        public bool NavigateBackAfterReadingAnArticle
        {
            get { return AppSettings.NavigateBackAfterReadingAnArticle; }
            set { AppSettings.NavigateBackAfterReadingAnArticle = value; }
        }
    }
}
