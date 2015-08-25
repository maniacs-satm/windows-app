using PropertyChanged;
using wallabag.Common;
using wallabag.Common.Mvvm;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    class FirstStartPageViewModel : ViewModelBase
    {
        public override string ViewModelIdentifier { get; set; } = "FirstStartPageViewModel";

        public string Username { get; set; }
        public string Password { get; set; }
        public string wallabagUrl { get; set; }

        public Command LoginCommand { get; private set; }
        public void Login()
        {
            if (wallabagUrl.EndsWith("/"))
               wallabagUrl= wallabagUrl.Remove(wallabagUrl.Length - 1);

            AppSettings.Username = Username;
            AppSettings.Password = Password;
            AppSettings.wallabagUrl = wallabagUrl;

            Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(Views.ContentPage));
        }

        public FirstStartPageViewModel()
        {
            LoginCommand = new Command(() => Login());
        }
    }
}
