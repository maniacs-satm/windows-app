using System;
using System.Threading.Tasks;
using PropertyChanged;
using Template10.Mvvm;
using wallabag.Common;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    class FirstStartPageViewModel : ViewModelBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string wallabagUrl { get; set; }

        public string StatusText { get; set; }

        public DelegateCommand LoginCommand { get; private set; }
        public async Task Login()
        {
            StatusText = "Logging in…";
            if (wallabagUrl.EndsWith("/"))
                wallabagUrl = wallabagUrl.Remove(wallabagUrl.Length - 1);

            if (Helpers.IsPhone)
                AppSettings.FontSize += 28;

            if (await Services.AuthorizationService.RequestTokenAsync(Username, Password, wallabagUrl))
            {
                StatusText = "Logged in.";
                AppSettings.wallabagUrl = wallabagUrl;
                StatusText = "Create database…";
                await Services.DataService.InitializeDatabaseAsync();
                NavigationService.Navigate(typeof(Views.ContentPage));
                NavigationService.ClearHistory();
            }
            else
                StatusText = "Login failed.";

        }

        public FirstStartPageViewModel()
        {
            LoginCommand = new DelegateCommand(async () => await Login());
        }
    }
}
