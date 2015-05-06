using Windows.Storage;
using System;
using System.ComponentModel;
using Windows.UI;

namespace wallabag.Common
{
    /// <summary>
    /// This class provides easy access to the roaming settings (sync over multiple devices).
    /// </summary>
    public class AppSettings : INotifyPropertyChanged
    {
        #region General things
        // This ugly code block allows it to access on the same instance even if we use the 'static' parameter.
        private static AppSettings _instance;
        private AppSettings() { }
        public static AppSettings Instance { get { return _instance ?? (_instance = new AppSettings()); } }

        private ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;

        /// <summary>
        /// Why a ObservableDictionary instead of instant access?
        /// Because it's faster.
        /// </summary>
        private ObservableDictionary _settings;
        public ObservableDictionary Settings
        {
            get
            {
                // For the first access on the settings, we load the data from the current sync state.
                if (_settings == null || _settings.Count == 0)
                {
                    _settings = new ObservableDictionary();
                    foreach (var s in ApplicationData.Current.RoamingSettings.Values)
                    {
                        _settings.Add(s.Key, s.Value);
                    }
                }
                return _settings;
            }
        }

        /// <summary>
        /// Allows easier access to the Settings and saves them directly in both the ObservableDictionary and the ApplicationDataContainer.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public dynamic this[string key, object defaultValue = default(object)]
        {
            get { return GetProperty(key, defaultValue); }
            set { SetProperty(key, value); }
        }

        /// <summary>
        /// To inform the View about a changed property, we raise a PropertyChangedEvent.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private T GetProperty<T>(string key, T defaultValue = default(T))
        {
            if (Settings.ContainsKey(key))
            {
                return (T)Settings[key];
            }
            return defaultValue;
        }
        private void SetProperty(string key, object value)
        {
            Settings[key] = value;
            RaisePropertyChanged(key);
            roamingSettings.Values[key] = value;
        }
        #endregion

        private const string UsernameKey = "Username";
        private const string PasswordKey = "Password";
        private const string WallabagUrlKey = "WallabagUrl";
        private const string FontSizeKey = "FontSize";
        private const string LineHeightKey = "LineHeight";
        private const string TextColorKey = "TextColor";
        private const string BackgroundColorKey = "BackgroundColor";

        public string Username
        {
            get { return GetProperty(UsernameKey, "wallabag"); }
            set { SetProperty(UsernameKey, value); }
        }
        public string Password
        {
            get { return GetProperty(PasswordKey, "wallabag"); }
            set { SetProperty(PasswordKey, value); }
        }
        public string WallabagUrl
        {
            get { return GetProperty(WallabagUrlKey, "http://v2.wallabag.org"); }
            set { SetProperty(WallabagUrlKey, value); }
        }
        public double FontSize
        {
            get { return GetProperty(FontSizeKey, 18); }
            set { SetProperty(FontSizeKey, value); }
        }
        public double LineHeight
        {
            get { return GetProperty(LineHeightKey, 1.5); }
            set { SetProperty(LineHeightKey, value); }
        }
        public Color TextColor
        {
            get { return GetProperty(TextColorKey, ColorHelper.FromArgb(255, 0, 0, 0)); } //#000000
            set { SetProperty(TextColorKey, value); }
        }
        public Color BackgroundColor
        {
            get { return GetProperty(BackgroundColorKey, ColorHelper.FromArgb(255, 255, 255, 255)); } //#ffffff
            set { SetProperty(BackgroundColorKey, value); }
        }
    }
}
