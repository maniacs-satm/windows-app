using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using wallabag.Common;
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

        public ICollection<Tag> ItemsSource
        {
            get { return (ICollection<Tag>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public List<string> possibleTags { get; set; }
        public ObservableCollection<string> Suggestions { get; set; } = new ObservableCollection<string>();

        private static async void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TagControl control = d as TagControl;
            control.UpdateNoTagsExistingStackPanelVisibility();

            if (control.possibleTags == null)
            {
                control.possibleTags = new List<string>();

                foreach (var item in await ViewModels.ViewModelLocator.CurrentDataService.GetTagsAsync())
                    control.possibleTags.Add(item.Label);
            }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ICollection<Tag>), typeof(TagControl), new PropertyMetadata(DependencyProperty.UnsetValue, new PropertyChangedCallback(OnTagsChanged)));

        private void textBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var possibleResults = new ObservableCollection<string>();
            if (possibleTags != null)
                possibleResults.Replace(possibleTags.Where(t => t.ToLower().Contains(sender.Text.ToLower())).ToList());

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                Suggestions.Clear();
                foreach (var item in possibleResults)
                    Suggestions.Add(item);
            }
        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ItemsSource.Remove(e.ClickedItem as Tag);
            UpdateNoTagsExistingStackPanelVisibility();
        }

        public void UpdateNoTagsExistingStackPanelVisibility()
        {
            if (ItemsSource.Count > 0)
                noTagsExistingStackPanel.Visibility = Visibility.Collapsed;
            else
                noTagsExistingStackPanel.Visibility = Visibility.Visible;
        }

        private void textBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(sender.Text))
            {
                List<string> tags = sender.Text.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries).ToList();

                Tag _lastTag = null;
                foreach (string item in tags)
                {
                    if (!string.IsNullOrWhiteSpace(item) &&
                        ItemsSource.Where(t => t.Label == item).Count() == 0)
                    {
                        var newTag = new Tag() { Label = item };
                        ItemsSource.Add(newTag);
                        _lastTag = newTag;
                    }
                }


                textBox.Text = string.Empty;
                listView.ScrollIntoView(_lastTag);
                UpdateNoTagsExistingStackPanelVisibility();
            }
        }
    }
}
