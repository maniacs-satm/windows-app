﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using wallabag.Models;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace wallabag.Common
{
    public static class WebViewExtensions
    {
        public static string GetHTML(DependencyObject obj)
        {
            return (string)obj.GetValue(HTMLProperty);
        }

        public static void SetHTML(DependencyObject obj, string value)
        {
            obj.SetValue(HTMLProperty, value);
        }

        // Using a DependencyProperty as the backing store for HTML.  This enables animation, styling, binding, etc... 
        public static readonly DependencyProperty HTMLProperty =
            DependencyProperty.RegisterAttached("HTML", typeof(string), typeof(WebViewExtensions), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnHTMLChanged)));

        private static void OnHTMLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebView wv = d as WebView;
            if (wv != null && e.NewValue != null)
            {
                wv.NavigateToString((string)e.NewValue);
            }
        }
    }
    public static class TagExtensions
    {
        public static string ToCommaSeparatedString<T>(this ICollection<T> list)
        {
            string result = string.Empty;
            if (list != null && list.Count > 0)
            {
                List<string> tempList = new List<string>();
                foreach (var item in list)
                    tempList.Add(item.ToString());

                // The usage of Distinct avoids duplicates in the list.
                // If the type isn't string, it won't work.
                List<string> distinctList = tempList.Distinct().ToList();

                foreach (var item in distinctList)
                {
                    if (!string.IsNullOrWhiteSpace(item.ToString()))
                        result += item.ToString() + ",";
                }
                if (result.EndsWith(","))
                    result = result.Remove(result.Length - 1);
            }

            return result;
        }
        public static ObservableCollection<Tag> ToObservableCollection(this string str)
        {
            ObservableCollection<Tag> result = new ObservableCollection<Tag>();
            if (!string.IsNullOrWhiteSpace(str))
            {
                var temp = str.Split(",".ToCharArray()).ToList();
                foreach (var item in temp)
                {
                    result.Add(new Tag()
                    {
                        Id = 1,
                        Label = item
                    });
                }
            }
            return result;
        }
    }
    public static class TitleBarExtensions
    {
        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.RegisterAttached("ForegroundColor", typeof(Color),
            typeof(TitleBarExtensions),
            new PropertyMetadata(null, OnForegroundColorPropertyChanged));

        public static Color GetForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ForegroundColorProperty);
        }

        public static void SetForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ForegroundColorProperty, value);
        }

        private static void OnForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = color;

            if (Helpers.IsPhone)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = color;
            }
        }

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.RegisterAttached("BackgroundColor", typeof(Color),
            typeof(TitleBarExtensions),
            new PropertyMetadata(null, OnBackgroundColorPropertyChanged));

        public static Color GetBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(BackgroundColorProperty);
        }

        public static void SetBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(BackgroundColorProperty, value);
        }

        private static void OnBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = color;

            if (Helpers.IsPhone)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundOpacity = 1;
                statusBar.BackgroundColor = color;
            }
        }

        public static readonly DependencyProperty ButtonForegroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonForegroundColor", typeof(Color),
            typeof(TitleBarExtensions),
            new PropertyMetadata(null, OnButtonForegroundColorPropertyChanged));

        public static Color GetButtonForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonForegroundColorProperty);
        }

        public static void SetButtonForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonForegroundColorProperty, value);
        }

        private static void OnButtonForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = color;
        }

        public static readonly DependencyProperty ButtonBackgroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonBackgroundColor", typeof(Color),
            typeof(TitleBarExtensions),
            new PropertyMetadata(null, OnButtonBackgroundColorPropertyChanged));

        public static Color GetButtonBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonBackgroundColorProperty);
        }

        public static void SetButtonBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonBackgroundColorProperty, value);
        }

        private static void OnButtonBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = color;
        }
    }
    public static class StringExtensions
    {
        public static string FormatWith(this string format, object source)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            Regex r = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            string rewrittenFormat = r.Replace(format, delegate (Match m)
            {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                var value = (propertyGroup.Value == null)
                           ? source
                           : source.GetType().GetRuntimeProperty(propertyGroup.Value).GetValue(source);

                return value.ToString();
            });
            return rewrittenFormat;
        }
    }
    public static class ListExtensions
    {
        public static void AddSorted<T>(this IList<T> List, T Item, IComparer<T> Comparer = null, bool SortDescending = false)
        {
            if (Comparer == null)
                Comparer = Comparer<T>.Default;

            int i = 0;

            if (!SortDescending)
                while (i < List.Count && Comparer.Compare(List[i], Item) < 0)
                    i++;
            else
                while (i < List.Count && Comparer.Compare(List[i], Item) > 0)
                    i++;

            List.Insert(i, Item);
        }
        public static void Replace<T>(this IList<T> oldList, IList<T> newList)
        {
            oldList.Clear();
            foreach (var item in newList)
                oldList.Add(item);
        }
    }
    public static class ObservableCollectionExtensions
    {
        public enum SortOrder { Ascending, Descending }
        public static void Sort<T>(this ObservableCollection<T> collection, SortOrder sortOrder = SortOrder.Descending) where T : IComparable
        {
            List<T> sorted;

            if (sortOrder == SortOrder.Ascending)
                sorted = collection.OrderBy(x => x).ToList();
            else
                sorted = collection.OrderByDescending(x => x).ToList();

            Sort(collection, sorted);
        }
        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> collection, Func<TSource, TKey> keySelector, SortOrder sortOrder = SortOrder.Descending) where TSource : IComparable
        {
            List<TSource> sorted;

            if (sortOrder == SortOrder.Ascending)
                sorted = collection.OrderBy(keySelector).ToList();
            else
                sorted = collection.OrderByDescending(keySelector).ToList();

            Sort(collection, sorted);
        }
        private static void Sort<T>(ObservableCollection<T> collection, List<T> list)
        {
            for (int i = 0; i < list.Count(); i++)
                collection.Move(collection.IndexOf(list[i]), i);
        }
    }
}
