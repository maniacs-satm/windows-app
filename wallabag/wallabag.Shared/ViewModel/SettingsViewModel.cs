using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using wallabag.Common;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace wallabag.ViewModel
{
    public class SettingsViewModel : viewModelBase
    {
        // The properties below are containing all the settings that are required for this app.
        public string wallabagUrl
        {
            get { return AppSettings["wallabagUrl", string.Empty]; }
            set
            {
                string tmp = "http://" + value;
                if (value.StartsWith("http://") || value.StartsWith("https://")) tmp = value;
                AppSettings["wallabagUrl"] = tmp;
                RaisePropertyChanged(() => wallabagUrl);
            }
        }
        public int userId
        {
            get { return AppSettings["userId", 1]; }
            set
            {
                AppSettings["userId"] = value;
                RaisePropertyChanged(() => userId);
            }
        }
        public string Token
        {
            get { return AppSettings["Token", string.Empty]; }
            set
            {
                AppSettings["Token"] = value;
                RaisePropertyChanged(() => Token);
            }
        }
        public bool AllowSelfSignedCertificates
        {
            get { return AppSettings["AllowSelfSignedCertificates", false]; }
            set
            {
                AppSettings["AllowSelfSignedCertificates"] = value;
                RaisePropertyChanged(() => AllowSelfSignedCertificates);
            }
        }
        public bool enableAddLink
        {
            get { return AppSettings["enableAddLink", false]; }
            set
            {
                AppSettings["enableAddLink"] = value;
                RaisePropertyChanged(() => enableAddLink);
            }
        }
        public double fontSize
        {
            get { return AppSettings["fontSize", 18]; }
            set
            {
                AppSettings["fontSize"] = value;
                RaisePropertyChanged(() => fontSize);
            }
        }
        public double lineHeight
        {
            get { return AppSettings["lineHeight", 1.5]; }
            set
            {
                AppSettings["lineHeight"] = value;
                RaisePropertyChanged(() => lineHeight);
            }
        }
        public bool isLightMode
        {
            get { return AppSettings["isLightMode", false]; }
            set
            {
                AppSettings["isLightMode"] = value;
                RaisePropertyChanged(() => isLightMode);
            }
        }

        public SolidColorBrush textColor
        {
            get {
                if (isLightMode)            
                    return new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 0)); // #000000
                else
                    return new SolidColorBrush(ColorHelper.FromArgb(255, 189, 189, 189)); // #bdbdbd
            }
        }
        public SolidColorBrush Background
        {
            get {
#if WINDOWS_PHONE_APP
                if (isLightMode)
                    return new SolidColorBrush(ColorHelper.FromArgb(255, 250, 247, 238)); // #faf7ee
                else
                    return new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 0)); // #000000
#else
                if (isLightMode)
                    return new SolidColorBrush(ColorHelper.FromArgb(255, 250, 247, 238)); // #faf7ee
                else
                    return new SolidColorBrush(ColorHelper.FromArgb(255, 29, 29, 29)); // #1D1D1D
#endif
            }
        }
        
        /// <summary>
        /// Represents a command to reset all settings.
        /// </summary>
        public RelayCommand resetCommand { get; private set; }

        public SettingsViewModel()
        {
            // Initialize the resetCommand.
            resetCommand = new RelayCommand(() => {
                AppSettings.Settings.Clear();
                Windows.Storage.ApplicationData.Current.RoamingSettings.Values.Clear();
            });
        }
    }
}
