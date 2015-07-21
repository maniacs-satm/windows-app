using System;
using System.Collections.Generic;
using System.ComponentModel;
using PropertyChanged;
using Windows.Security.Credentials;
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

        private static T GetProperty<T>(string key, T defaultValue = default(T))
        {
            return (T)(roamingSettings.Values[key] ?? defaultValue);
        }
        private static void SetProperty(string key, object value)
        {
            roamingSettings.Values[key] = value;
            RaisePropertyChanged(key);
        }
        #endregion

        #region PasswordVault
        private static void GetFromPasswordVault()
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
        private static void SaveToPasswordVault()
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

        private static string _Username;
        public static string Username
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
        private static  string _Password;
        public static string Password
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
        private static  string _wallabagUrl;
        public static string wallabagUrl
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

        public static double FontSize
        {
            get { return GetProperty<double>(nameof(FontSize), 16); }
            set { SetProperty(nameof(FontSize), value); }
        }
        public static double LineHeight
        {
            get { return GetProperty(nameof(LineHeight), 1.7); }
            set { SetProperty(nameof(LineHeight), value); }
        }
        public static string FontFamily
        {
            get { return GetProperty(nameof(FontFamily), "serif"); }
            set { SetProperty(nameof(FontFamily), value); }
        }
        public static string ColorScheme
        {
            get { return GetProperty(nameof(ColorScheme), "light"); }
            set { SetProperty(nameof(ColorScheme), value); }
        }

        public static bool UseSystemAccentColor
        {
            get { return GetProperty(nameof(UseSystemAccentColor), false); }
            set { SetProperty(nameof(UseSystemAccentColor), value); }
        }
        public static bool HamburgerPositionIsRight
        {
            get { return GetProperty(nameof(HamburgerPositionIsRight), false); }
            set { SetProperty(nameof(HamburgerPositionIsRight), value); }
        }
        public static bool NavigateBackAfterReadingAnArticle
        {
            get { return GetProperty(nameof(NavigateBackAfterReadingAnArticle), true); }
            set { SetProperty(nameof(NavigateBackAfterReadingAnArticle), value); }
        }
    }
}
