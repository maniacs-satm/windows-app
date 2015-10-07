using System.Threading.Tasks;
using wallabag.Common;
using Windows.ApplicationModel.Activation;

namespace wallabag.Universal
{
    sealed partial class App : BootStrapper
    {
        public App() : base()
        {
            InitializeComponent();
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            if (!string.IsNullOrEmpty(AppSettings.AccessToken))
            {
                await Services.DataService.InitializeDatabaseAsync();
                NavigationService.Navigate(typeof(Views.ContentPage));
            }
            else
                NavigationService.Navigate(typeof(Views.FirstStartPage));
        }
    }
}
