using System;
using System.Collections.Generic;
using System.ComponentModel;
using wallabag.Common.MVVM;
using Windows.Security.Credentials;
using Windows.Storage;
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

        #region PasswordVault
        private void GetFromPasswordVault()
        {
            try
            {
                PasswordVault vault = new PasswordVault();
                IReadOnlyList<PasswordCredential> creds = vault.RetrieveAll();

                if (creds != null && creds.Count != 0)
                {
                    creds[0].RetrievePassword();
                    _Username = creds[0].UserName;
                    _Password = creds[0].Password;
                    _wallabagUrl = creds[0].Resource;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void SaveToPasswordVault()
        {
            if (!string.IsNullOrWhiteSpace(_Username) &&
                !string.IsNullOrWhiteSpace(_Password) &&
                !string.IsNullOrWhiteSpace(_wallabagUrl))
            {
                PasswordVault vault = new PasswordVault();
                PasswordCredential cred = new PasswordCredential(_wallabagUrl, _Username, _Password);

                if (vault.RetrieveAll().Count == 0)
                    vault.Add(cred);
                else
                {
                    vault.Remove(vault.RetrieveAll()[0]);
                    vault.Add(cred);
                }
            }
        }
        #endregion

        private const string UsernameKey = "Username";
        private const string PasswordKey = "Password";
        private const string WallabagUrlKey = "WallabagUrl";
        private const string FontSizeKey = "FontSize";
        private const string LineHeightKey = "LineHeight";
        private const string TextColorKey = "TextColor";
        private const string BackgroundColorKey = "BackgroundColor";

        private string _Username;
        public string Username
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_Username))
                    GetFromPasswordVault();
                return _Username;
            }
            set
            {
                _Username = value;
                SaveToPasswordVault();

                RaisePropertyChanged(nameof(Username));
            }
        }
        private string _Password;
        public string Password
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_Password))
                    GetFromPasswordVault();
                return _Password;
            }
            set
            {
                _Password = value;
                SaveToPasswordVault();
                RaisePropertyChanged(nameof(Password));
            }
        }
        private string _wallabagUrl;
        public string wallabagUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_wallabagUrl)) GetFromPasswordVault();
                return _wallabagUrl;
            }
            set
            {
                _wallabagUrl = value;
                RaisePropertyChanged(nameof(wallabagUrl));
                SaveToPasswordVault();
            }
        }

        public double FontSize
        {
            get { return GetProperty<double>(FontSizeKey, 18); }
            set { SetProperty(FontSizeKey, value); }
        }
        public double LineHeight
        {
            get { return GetProperty<double>(LineHeightKey, 1.5); }
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
