using System.Threading.Tasks;
using PropertyChanged;
using Template10.Mvvm;
using wallabag.Common;
using Template10.Services.NavigationService;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    class FirstStartPageViewModel : ViewModelBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string wallabagUrl { get; set; }

        public DelegateCommand LoginCommand { get; private set; }
        public async Task Login()
        {
            if (wallabagUrl.EndsWith("/"))
                wallabagUrl = wallabagUrl.Remove(wallabagUrl.Length - 1);

            AppSettings.Username = Username;
            AppSettings.Password = Password;
            AppSettings.wallabagUrl = wallabagUrl;

            if (Helpers.IsPhone)
                AppSettings.FontSize += 28;

            await Services.DataService.InitializeDatabaseAsync();
            NavigationService.Navigate(typeof(Views.ContentPage));
        }

        public FirstStartPageViewModel()
        {
            LoginCommand = new DelegateCommand(async () => await Login());
        }
    }
}
