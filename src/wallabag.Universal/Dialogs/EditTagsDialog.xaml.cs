using System.Collections.Generic;
using wallabag.Models;
using Windows.UI.Xaml.Controls;

namespace wallabag.Dialogs
{
    public sealed partial class EditTagsDialog : ContentDialog
    {

        public ICollection<Tag> Tags
        {
            get { return DataContext as ICollection<Tag>; }
            set { DataContext = value; }
        }

        public EditTagsDialog()
        {
            this.InitializeComponent();
        }

    }
}
