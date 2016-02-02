using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using wallabag.Models;
using Windows.UI.Xaml.Controls;

namespace wallabag.Dialogs
{
    public sealed partial class EditTagsDialog : ContentDialog
    {
        private ICollection<Tag> Tags;

        public EditTagsDialog()
        {
            this.InitializeComponent();
            Tags = new ObservableCollection<Tag>();
            Messenger.Default.Register<NotificationMessage<ICollection<Tag>>>(this, message => { this.Tags = message.Content; });
        }

        private void UnregisterMessenger(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Messenger.Default.Unregister(this);
        }
    }
}
