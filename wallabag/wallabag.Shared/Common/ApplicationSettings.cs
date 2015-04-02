using Windows.Storage;
using System;
using System.ComponentModel;

namespace wallabag.Common
{
    /// <summary>
    /// This class provides easy access to the roaming settings (sync over multiple devices).
    /// </summary>
    public class ApplicationSettings : INotifyPropertyChanged
    {
        // This ugly code block allows it to access on the same instance even if we use the 'static' parameter.
        private static ApplicationSettings _instance;
        private ApplicationSettings() { }
        public static ApplicationSettings Instance { get { return _instance ?? (_instance = new ApplicationSettings()); } }

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
                System.Diagnostics.Debug.WriteLine("Set property: " + propertyName );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
