using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using wallabag.DataModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" ist unter http://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace wallabag.Controls
{
    public sealed partial class TagControl : UserControl
    {
        public TagControl()
        {
            this.InitializeComponent();
        }

        public ICollection<Tag> Tags
        {
            get { return (ICollection<Tag>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register("Tags", typeof(ICollection<Tag>), typeof(TagControl), new PropertyMetadata(DependencyProperty.UnsetValue));

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var currentText = textBox.Text.ToLower();
            char comma;
            char.TryParse(",", out comma);
            if (currentText.Length > 0 && currentText[currentText.Length - 1] == comma)
            {
                currentText = currentText.Remove(currentText.Length - 1);
                Tags.Add(new Tag() { Label = currentText });
                textBox.Text = string.Empty;
            }

        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Tags.Remove(e.ClickedItem as Tag);
        }
    }
}
