using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Security.Credentials;
using Windows.Storage;

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
        /// Why a Dictionary instead of instant access?
        /// Because it's faster.
        /// </summary>
        private Dictionary<string,object> _settings;
        public Dictionary<string, object> Settings
        {
            get
            {
                // For the first access on the settings, we load the data from the current sync state.
                if (_settings == null || _settings.Count == 0)
                {
                    _settings = new Dictionary<string, object>();
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
            get { return GetProperty<double>(nameof(FontSize), 16); }
            set { SetProperty(nameof(FontSize), value); }
        }
        public double LineHeight
        {
            get { return GetProperty(nameof(LineHeight), 1.7); }
            set { SetProperty(nameof(LineHeight), value); }
        }
        public string FontFamily
        {
            get { return GetProperty(nameof(FontFamily), "serif"); }
            set { SetProperty(nameof(FontFamily), value); }
        }
        public string ColorScheme
        {
            get { return GetProperty(nameof(ColorScheme), "light"); }
            set { SetProperty(nameof(ColorScheme), value); }
        }

        public bool UseSystemAccentColor
        {
            get { return GetProperty(nameof(UseSystemAccentColor), false); }
            set { SetProperty(nameof(UseSystemAccentColor), value); }
        }
        public bool HamburgerPositionIsRight
        {
            get { return GetProperty(nameof(HamburgerPositionIsRight), false); }
            set { SetProperty(nameof(HamburgerPositionIsRight), value); }
        }
    }
}
