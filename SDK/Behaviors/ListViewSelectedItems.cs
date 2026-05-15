using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SDK.Behaviors
{
    public static class ListViewSelectedItems
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(ListViewSelectedItems),
                new PropertyMetadata(null, OnSelectedItemsChanged));

        public static IList GetSelectedItems(DependencyObject obj) => (IList)obj.GetValue(SelectedItemsProperty);
        public static void SetSelectedItems(DependencyObject obj, IList value) => obj.SetValue(SelectedItemsProperty, value);

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView listView)
            {
                listView.SelectionChanged -= ListView_SelectionChanged;
                if (e.NewValue is IList)
                {
                    listView.SelectionChanged += ListView_SelectionChanged;
                }
            }
        }

        private static void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView)sender;
            var boundList = GetSelectedItems(listView);
            if (boundList == null) return;

            // Keep the bound collection in sync
            foreach (var item in e.RemovedItems)
                if (boundList.Contains(item))
                    boundList.Remove(item);

            foreach (var item in e.AddedItems)
                if (!boundList.Contains(item))
                    boundList.Add(item);
        }
    }
}