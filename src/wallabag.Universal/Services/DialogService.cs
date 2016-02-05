using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace wallabag.Services
{
    class DialogService
    {
        public static async Task ShowDialogAsync(Dialog dialog)
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

            await c.ShowAsync();
        }

        public enum Dialog
        {
            AddItem,
            EditTags
        }
    }
}
