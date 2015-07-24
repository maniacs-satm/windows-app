using System.Collections.Generic;
using System.Linq;
using wallabag.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Benutzersteuerelement" ist unter http://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace wallabag.Controls
{
    public sealed partial class TagControl : UserControl
    {
        public TagControl()
        {
            InitializeComponent();
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

            if (currentText.Length > 1 && currentText[currentText.Length - 1] == comma)
            {
                currentText = currentText.Remove(currentText.Length - 1);

                var newTag = new Tag() { Label = currentText };

                if (Tags.Where(t => t.Label == currentText).Count() == 0)
                    Tags.Add(newTag);

                textBox.Text = string.Empty;
                UpdateRootGridBorderThickness();
                listView.ScrollIntoView(newTag);
            }
        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Tags.Remove(e.ClickedItem as Tag);
            UpdateRootGridBorderThickness();
        }

        private void UpdateRootGridBorderThickness()
        {
            if (Tags.Count > 0)
                RootGrid.BorderThickness = new Thickness(2, 2, 2, 0);
            else
                RootGrid.BorderThickness = new Thickness(2, 0, 2, 0);
        }
    }
}
