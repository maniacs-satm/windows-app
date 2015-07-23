using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace wallabag.Universal
{
    sealed partial class App : Common.BootStrapper
    {
        public App() : base()
        {
            InitializeComponent();
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            await Services.DataSource.InitializeDatabaseAsync();           
            NavigationService.Navigate(typeof(Views.ContentPage));
        }
    }
}
