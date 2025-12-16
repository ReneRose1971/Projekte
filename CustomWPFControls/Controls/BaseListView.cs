using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace CustomWPFControls.Controls
{
    public class BaseListView : ListView
    {
        static BaseListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseListView), new FrameworkPropertyMetadata(typeof(BaseListView)));
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(int), typeof(BaseListView), new PropertyMetadata(0));

        public int Count
        {
            get => (int)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            SetValue(CountProperty, Items?.Count ?? 0);
        }
    }
}