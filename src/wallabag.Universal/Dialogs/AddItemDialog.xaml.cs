using wallabag.ViewModels;
using Windows.UI.Xaml.Controls;

namespace wallabag.Dialogs
{
    public sealed partial class AddItemDialog : ContentDialog
    {
        public AddItemPageViewModel ViewModel { get { return this.DataContext as AddItemPageViewModel; } }

        public AddItemDialog()
        {
            this.InitializeComponent();
        }
    }
}
