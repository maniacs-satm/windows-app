using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wallabag.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Common.Mvvm
{
    public abstract class ViewModelBase : BindableBase, INavigable
    {
        public ViewModelBase()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Dispatch = (Windows.UI.Xaml.Application.Current as BootStrapper).Dispatch;
                NavigationService = (Windows.UI.Xaml.Application.Current as BootStrapper).NavigationService;
            }
        }
        public Action<Action> Dispatch { get; set; }
        public NavigationService NavigationService { get; private set; }
        public abstract string ViewModelIdentifier { get; set; }

        public virtual void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state) { /* nothing by default */ }
        public virtual Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending) { return Task.FromResult<object>(null); }
        public virtual void OnNavigatingFrom(NavigatingEventArgs args) { /* nothing by default */ }
    }
}