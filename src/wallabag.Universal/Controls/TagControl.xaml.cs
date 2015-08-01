using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using wallabag.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
        public ObservableCollection<string> possibleTags { get; set; }
        public ObservableCollection<string> Suggestions { get; set; } = new ObservableCollection<string>();

        private static async void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TagControl control = d as TagControl;
            control.UpdateNoTagsExistingStackPanelVisibility();

            if (control.possibleTags == null)
            {
                control.possibleTags = new ObservableCollection<string>();
                List<Tag> tags = new List<Tag>(); ;
                SQLite.SQLiteAsyncConnection conn = new SQLite.SQLiteAsyncConnection(Common.Helpers.DATABASE_PATH);
                tags = await conn.Table<Tag>().ToListAsync();

                foreach (var item in tags)
                    control.possibleTags.Add(item.Label);
            }
        }

        // Using a DependencyProperty as the backing store for Tags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register("Tags", typeof(ICollection<Tag>), typeof(TagControl), new PropertyMetadata(DependencyProperty.UnsetValue, new PropertyChangedCallback(OnTagsChanged)));

        private void textBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // TODO: Fast up this method!
            var possibleResults = new ObservableCollection<string>(possibleTags.Where(t=>t.Contains(sender.Text.ToLower())));

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                Suggestions.Clear();
                foreach (var item in possibleResults)
                    Suggestions.Add(item);
            }

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
                UpdateNoTagsExistingStackPanelVisibility();
                listView.ScrollIntoView(newTag);
            }
        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Tags.Remove(e.ClickedItem as Tag);
            UpdateNoTagsExistingStackPanelVisibility();
        }

        public void UpdateNoTagsExistingStackPanelVisibility()
        {
            if (Tags.Count > 0)
                noTagsExistingStackPanel.Visibility = Visibility.Collapsed;
            else
                noTagsExistingStackPanel.Visibility = Visibility.Visible;
        }

        private void textBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var newTag = new Tag() { Label = (sender as AutoSuggestBox).Text };

                if (Tags.Where(t => t.Label == (sender as AutoSuggestBox).Text).Count() == 0)
                    Tags.Add(newTag);

                textBox.Text = string.Empty;
            }
        }
    }
}
