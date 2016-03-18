using PropertyChanged;
using System;
using System.ComponentModel;
using Windows.Storage;

namespace wallabag.Common
{
    /// <summary>
    /// This class provides easy access to the roaming settings (sync over multiple devices).
    /// </summary>
    [ImplementPropertyChanged]
    public class AppSettings
    {
        #region General things

        private static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        /// <summary>
        /// To inform the View about a changed property, we raise a PropertyChangedEvent.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        private static void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged.Invoke(e, e);
            }
        }
        public static event PropertyChangedEventHandler PropertyChanged;

        private static T GetProperty<T>(string key, T defaultValue = default(T), bool SaveLocal = false)
        {
            if (SaveLocal)
                return (T)(localSettings.Values[key] ?? defaultValue);
            else
                return (T)(roamingSettings.Values[key] ?? defaultValue);
        }
        private static void SetProperty(string key, object value, bool IsSavedLocal = false)
        {
            if (IsSavedLocal)
                localSettings.Values[key] = value;
            else
                roamingSettings.Values[key] = value;
            RaisePropertyChanged(key);
        }
        #endregion

        public static string wallabagUrl
        {
            get { return GetProperty(nameof(wallabagUrl), string.Empty); }
            set { SetProperty(nameof(wallabagUrl), value); }
        }
        public static string AccessToken
        {
            get { return GetProperty(nameof(AccessToken), string.Empty, true); }
            set { SetProperty(nameof(AccessToken), value, true); }
        }
        public static string RefreshToken
        {
            get { return GetProperty(nameof(RefreshToken), string.Empty, true); }
            set { SetProperty(nameof(RefreshToken), value, true); }
        }
        public static string ClientId
        {
            get { return GetProperty(nameof(ClientId), string.Empty); }
            set { SetProperty(nameof(ClientId), value); }
        }
        public static string ClientSecret
        {
            get { return GetProperty(nameof(ClientSecret), string.Empty); }
            set { SetProperty(nameof(ClientSecret), value); }
        }

        public static double FontSize
        {
            get { return GetProperty<double>(nameof(FontSize), 18, true); }
            set { SetProperty(nameof(FontSize), value, true); }
        }
        public static string FontFamily
        {
            get { return GetProperty(nameof(FontFamily), "serif"); }
            set { SetProperty(nameof(FontFamily), value); }
        }
        public static string ColorScheme
        {
            get { return GetProperty(nameof(ColorScheme), "light", true); }
            set { SetProperty(nameof(ColorScheme), value, true); }
        }
        public static string TextAlignment
        {
            get { return GetProperty(nameof(TextAlignment), "left"); }
            set { SetProperty(nameof(TextAlignment), value); }
        }

        public static bool NavigateBackAfterReadingAnArticle
        {
            get { return GetProperty(nameof(NavigateBackAfterReadingAnArticle), true); }
            set { SetProperty(nameof(NavigateBackAfterReadingAnArticle), value); }
        }
        public static bool SyncReadingProgress
        {
            get { return GetProperty(nameof(SyncReadingProgress), true); }
            set { SetProperty(nameof(SyncReadingProgress), value); }
        }
        public static bool SyncOnStartup
        {
            get { return GetProperty(nameof(SyncOnStartup), true); }
            set { SetProperty(nameof(SyncOnStartup), value); }
        }
        public static bool UseBackgroundTask
        {
            get { return GetProperty(nameof(UseBackgroundTask), false); }
            set { SetProperty(nameof(UseBackgroundTask), value); }
        }
        public static uint BackgroundTaskInterval
        {
            get { return GetProperty<uint>(nameof(BackgroundTaskInterval), 15); }
            set { SetProperty(nameof(BackgroundTaskInterval), value); }
        }
        public static bool OpenTheFilterPaneWithTheSearch
        {
            get { return GetProperty(nameof(OpenTheFilterPaneWithTheSearch), true); }
            set { SetProperty(nameof(OpenTheFilterPaneWithTheSearch), value); }
        }

        public static DateTimeOffset? LastOpeningDateTime
        {
            get { return GetProperty(nameof(LastOpeningDateTime), DateTimeOffset.Now, true); }
            set { SetProperty(nameof(LastOpeningDateTime), value, true); }
        }
    }
}
