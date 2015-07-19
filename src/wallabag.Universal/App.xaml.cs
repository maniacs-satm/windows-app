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
            await Services.DataSource.InitializeDatabase();

            // Change the accent color to #666666 if enabled in settings
            if (!Common.AppSettings.UseSystemAccentColor)
                Current.Resources["SystemAccentColor"] = Windows.UI.ColorHelper.FromArgb(0xFF, 0x66, 0x66, 0x66);

            NavigationService.Navigate(typeof(Views.ContentPage));
        }
    }
}
