using System;
using System.Collections.Generic;
using System.Text;
using GalaSoft.MvvmLight;
using Windows.ApplicationModel.Resources;

namespace wallabag.Common
{
    /// <summary>
    /// To extend the capabilites of the GalaSoft.MvvmLigt.ViewModelBase we use this class, which provides an
    /// easy access to the ApplicationSettings and make it very easy to change the text of the StatusBar.
    /// </summary>
    public class viewModelBase : ViewModelBase
    {
        public ApplicationSettings AppSettings { get { return ApplicationSettings.Instance; } }
        
        private bool _IsActive;
        private string _StatusText;
       
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                Set(() => IsActive, ref _IsActive, value);

#if WINDOWS_PHONE_APP    
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                if (value)
                    statusBar.ProgressIndicator.ShowAsync();
                else 
                    statusBar.ProgressIndicator.HideAsync();
#endif
            }
        }
        public string StatusText
        {
            get { return _StatusText; }
            set
            {
                Set(() => StatusText, ref _StatusText, value);

#if WINDOWS_PHONE_APP
                if (!string.IsNullOrWhiteSpace(value))
                    Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ProgressIndicator.Text = value;
#endif
            }
        }
        
    }
}
