using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Windows.ApplicationModel.Resources;

namespace wallabag.Common
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ApplicationSettings AppSettings { get { return ApplicationSettings.Instance; } }

        private bool _IsActive;
        private string _StatusText;

        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                _IsActive = value;
                RaisePropertyChanged("IsActive");

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
                _StatusText = value;
                RaisePropertyChanged("StatusText");

#if WINDOWS_PHONE_APP
                if (!string.IsNullOrWhiteSpace(value))
                    Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ProgressIndicator.Text = value;
#endif
            }
        }

        public virtual void RaisePropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
