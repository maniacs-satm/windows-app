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
            get
            {
                if (Settings.ContainsKey(key))
                {
                    return Settings[key];
                }
                return defaultValue;
            }
            set
            {
                Settings[key] = value;
                RaisePropertyChanged(key);
                roamingSettings.Values[key] = value;
            }
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
            get { return this[UsernameKey, "wallabag"]; }
            set { this[UsernameKey] = value; }
        }
        public string Password
        {
            get { return this[PasswordKey, "wallabag"]; }
            set { this[PasswordKey] = value; }
        }
        public string WallabagUrl
        {
            get { return this[WallabagUrlKey, string.Empty]; }
            set { this[WallabagUrlKey] = value; }
        }
        public double FontSize
        {
            get { return this[FontSizeKey, 18]; }
            set { this[FontSizeKey] = value; }
        }
        public double LineHeight
        {
            get { return this[LineHeightKey, 1.5]; }
            set { this[LineHeightKey] = value; }
        }
        public Color TextColor
        {
            get { return this[TextColorKey, ColorHelper.FromArgb(255, 189, 189, 189)]; } //#bdbdbd
            set { this[TextColorKey] = value; }
        }
        public Color BackgroundColor
        {
            get { 
                return this[BackgroundColorKey, ColorHelper.FromArgb(255, 29, 29, 29)]; //#1d1d1d
            }
            set { this[BackgroundColorKey] = value; }
        }
    }
}
