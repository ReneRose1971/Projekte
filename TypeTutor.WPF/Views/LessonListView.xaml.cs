using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using TypeTutor.Logic.Core;

namespace TypeTutor.WPF
{
    /// <summary>
    /// Interaktionslogik für LessonListView.xaml
    /// </summary>
    public partial class LessonListView : UserControl
    {
        private CollectionViewSource? _cvs;

        public LessonListView()
        {
            InitializeComponent();
            Loaded += LessonListView_Loaded;
        }

        private void LessonListView_Loaded(object? sender, RoutedEventArgs e)
        {
            _cvs = (CollectionViewSource?)Resources["LessonsView"];
            if (_cvs != null)
            {
                _cvs.Filter += LessonsView_Filter;
            }

            // Listen to ViewModel property changes for filter updates
            if (this.DataContext is LessonListViewModel vm)
            {
                vm.PropertyChanged += Vm_PropertyChanged;
            }
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LessonListViewModel.SelectedModuleFilter) && _cvs != null)
            {
                _cvs.View?.Refresh();
            }
        }

        private void LessonsView_Filter(object? sender, FilterEventArgs e)
        {
            if (e.Item is Lesson l && this.DataContext is LessonListViewModel vm)
            {
                var filter = vm.SelectedModuleFilter;
                if (string.IsNullOrWhiteSpace(filter))
                {
                    e.Accepted = true;
                    return;
                }

                e.Accepted = string.Equals(l.Meta.ModuleId?.Trim() ?? string.Empty, filter.Trim(), StringComparison.OrdinalIgnoreCase);
                return;
            }
            e.Accepted = false;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);

            if (this.DataContext is LessonListViewModel vm)
            {
                vm.UpdateSelectionFromView(InnerList.SelectedItems.Cast<object>());
            }
        }

        public System.Collections.IList SelectedItems => InnerList.SelectedItems;

        // New: expose selection changed to parent and provide access to selected items
        public event SelectionChangedEventHandler? SelectionChanged;

        private void OnClearFilter(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LessonListViewModel vm)
            {
                vm.SelectedModuleFilter = null;
            }
        }

        // Sorting support
        private GridViewColumnHeader _lastHeaderClicked = null!;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void OnItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (InnerList.SelectedItem == null)
                return;

            var source = e.OriginalSource as DependencyObject;
            var listBoxItem = FindAncestor<ListViewItem>(source);
            if (listBoxItem == null)
                return;

            ItemDoubleClick?.Invoke(InnerList, e);
        }

        // Event forwarder so parent windows can react to double-click
        public event MouseButtonEventHandler? ItemDoubleClick;

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t)
                    return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void OnHeaderClick(object sender, RoutedEventArgs e)
        {
            if (sender is GridViewColumnHeader header && header.Tag is string sortBy)
            {
                ListSortDirection direction = ListSortDirection.Ascending;
                if (_lastHeaderClicked == header && _lastDirection == ListSortDirection.Ascending)
                    direction = ListSortDirection.Descending;

                var view = CollectionViewSource.GetDefaultView(InnerList.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription(sortBy, direction));
                view.Refresh();

                _lastHeaderClicked = header;
                _lastDirection = direction;
            }
        }
    }
}
