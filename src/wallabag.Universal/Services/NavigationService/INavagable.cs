﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Services.NavigationService
{
    public interface INavigable 
    {
        void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state);
        Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending);
        void OnNavigatingFrom(Services.NavigationService.NavigatingEventArgs args);
        Action<Action> Dispatch { get; set; }
        string ViewModelIdentifier { get; set; }
    }
}
