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
            await Services.DataService.InitializeDatabaseAsync();
            if (!string.IsNullOrEmpty(AppSettings.Username) &&
                !string.IsNullOrEmpty(AppSettings.wallabagUrl))
                NavigationService.Navigate(typeof(Views.ContentPage));
            else
                NavigationService.Navigate(typeof(Views.FirstStartPage));
        }
    }
}
