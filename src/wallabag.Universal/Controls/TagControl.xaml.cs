using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using wallabag.Models;
using wallabag.Services;
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
        public List<string> possibleTags { get; set; }
        public ObservableCollection<string> Suggestions { get; set; } = new ObservableCollection<string>();

        private static async void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TagControl control = d as TagControl;
            control.UpdateNoTagsExistingStackPanelVisibility();

            if (control.possibleTags == null)
            {
                control.possibleTags = new List<string>();
                foreach (var item in await DataService.GetTagsAsync())
                    control.possibleTags.Add(item.Label);
            }
        }

        // Using a DependencyProperty as the backing store for Tags.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register("Tags", typeof(ICollection<Tag>), typeof(TagControl), new PropertyMetadata(DependencyProperty.UnsetValue, new PropertyChangedCallback(OnTagsChanged)));

        private void textBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var possibleResults = new ObservableCollection<string>(possibleTags.Where(t => t.Contains(sender.Text.ToLower())));

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                Suggestions.Clear();
                foreach (var item in possibleResults)
                    Suggestions.Add(item);
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

        private void textBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(sender.Text))
            {
                List<string> tags = sender.Text.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries).ToList();

                Tag _lastTag = null;
                foreach (string item in tags)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        if (Tags.Where(t => t.Label == item).Count() == 0)
                        {
                            var newTag = new Tag() { Label = item };
                            Tags.Add(newTag);
                            _lastTag = newTag;
                        }
                    }
                }

                textBox.Text = string.Empty;
                listView.ScrollIntoView(_lastTag);
                UpdateNoTagsExistingStackPanelVisibility();
            }
        }
    }
}
