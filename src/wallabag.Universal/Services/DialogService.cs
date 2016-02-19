using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace wallabag.Services
{
    class DialogService
    {
        public static IAsyncOperation<ContentDialogResult> ShowDialogAsync(Dialog dialog, object parameter = null)
        {
            ContentDialog c = new ContentDialog();

            switch (dialog)
            {
                case Dialog.AddItem:
                    c = new Dialogs.AddItemDialog();
                    break;
                case Dialog.EditTags:
                    c = new Dialogs.EditTagsDialog();
                    break;
            }

            if (parameter != null)
                c.DataContext = parameter;
            return c.ShowAsync();
        }       

        public enum Dialog
        {
            AddItem,
            EditTags
        }
    }
}
