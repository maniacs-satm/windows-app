using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Windows.ApplicationModel.Resources;

namespace wallabag.Common
{
    public class ViewModelBase : INotifyPropertyChanged
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
